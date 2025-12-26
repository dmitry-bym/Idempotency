using Idempotency.Core.Abstractions;
using Idempotency.Core.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Idempotency.Store.MongoDb;

public sealed class MongoIdempotencyStore(IMongoCollection<IdempotencyRecord> collection, IOptions<MongoIdempotencyStoreOptions> options) : IIdempotencyStore
{
    private readonly Task _ensureIndexesTask = EnsureIndexesAsync(collection);

    private static Task EnsureIndexesAsync(IMongoCollection<IdempotencyRecord> collection)
    {
        var indexKey = Builders<IdempotencyRecord>.IndexKeys.Ascending(x => x.ExpiresAt);
        var indexOptions = new CreateIndexOptions
        {
            ExpireAfter = TimeSpan.Zero
        };
        return collection.Indexes.CreateOneAsync(new CreateIndexModel<IdempotencyRecord>(indexKey, indexOptions));
    }

    public async Task<IdempotencyClaim> ClaimAsync(
        IdempotencyKey key,
        RequestFingerprint fingerprint,
        CancellationToken ct = default)
    {
        await _ensureIndexesTask;

        var id = GetId(key);

        var createdUtc = DateTime.UtcNow;
        var expiresUtc = createdUtc.Add(options.Value.TimeToLive);
        
        var update = Builders<IdempotencyRecord>.Update
            .SetOnInsert(x => x.ActorId, key.ActorId)
            .SetOnInsert(x => x.Scope, key.Scope)
            .SetOnInsert(x => x.Key, key.Key)
            .SetOnInsert(x => x.Fingerprint, fingerprint.Hash)
            .SetOnInsert(x => x.CreatedAt, createdUtc)
            .SetOnInsert(x => x.ExpiresAt, expiresUtc)
            .SetOnInsert(x => x.Status, IdempotencyStatus.InProgress);

        var operationOptions = new FindOneAndUpdateOptions<IdempotencyRecord>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.Before
        };

        var existing = await collection.FindOneAndUpdateAsync(
            x => x.Id == id,
            update,
            operationOptions,
            ct);

        if (existing == null)
        {
            return new IdempotencyClaim(
                IsOwner: true,
                Status: IdempotencyStatus.InProgress,
                StoredFingerprint: fingerprint,
                Data: null);
        }

        return new IdempotencyClaim(
            IsOwner: false,
            Status: existing.Status,
            StoredFingerprint: new RequestFingerprint(existing.Fingerprint),
            Data: existing.Data != null ? new IdempotencyData(existing.Data) : null);
    }

    public async Task CompleteAsync(
        IdempotencyKey key,
        IdempotencyData data,
        CancellationToken ct = default)
    {
        await _ensureIndexesTask;

        var id = GetId(key);

        var update = Builders<IdempotencyRecord>.Update
            .Set(x => x.Status, IdempotencyStatus.Completed)
            .Set(x => x.Data, data.Data);

        await collection.UpdateOneAsync(
            x => x.Id == id,
            update,
            cancellationToken: ct);
    }

    public async Task ReleaseAsync(
        IdempotencyKey key,
        RequestFingerprint fingerprint,
        CancellationToken ct = default)
    {
        await _ensureIndexesTask;

        var id = GetId(key);

        await collection.DeleteOneAsync(
            x => x.Id == id &&
                 x.Status == IdempotencyStatus.InProgress &&
                 x.Fingerprint == fingerprint.Hash,
            ct);
    }

    private static string GetId(IdempotencyKey key) => $"{key.ActorId}:{key.Scope}:{key.Key}";
}