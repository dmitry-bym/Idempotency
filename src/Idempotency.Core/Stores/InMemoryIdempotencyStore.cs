using System.Collections.Concurrent;
using Idempotency.Core.Abstractions;
using Idempotency.Core.Models;

namespace Idempotency.Core.Stores;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<StoreKey, InMemoryRecord> _store = new();

    public Task<IdempotencyClaim> ClaimAsync(
        IdempotencyKey key,
        RequestFingerprint fingerprint,
        CancellationToken ct = default)
    {
        var storeKey = new StoreKey(
            key.ActorId,
            key.Scope,
            key.Key);

        var newRecord = new InMemoryRecord
        {
            Fingerprint = fingerprint,
            Status = IdempotencyStatus.InProgress
        };

        var isOwner = _store.TryAdd(storeKey, newRecord);
        var record = isOwner ? newRecord : _store[storeKey];

        if (record.Status == IdempotencyStatus.Completed)
        {
            return Task.FromResult(new IdempotencyClaim(
                IsOwner: false,
                Status: record.Status,
                StoredFingerprint: record.Fingerprint,
                Data: record.Data));
        }

        return Task.FromResult(new IdempotencyClaim(
            IsOwner: isOwner,
            Status: record.Status,
            StoredFingerprint: record.Fingerprint,
            Data: null));
    }

    public Task CompleteAsync(
        IdempotencyKey key,
        IdempotencyData data,
        CancellationToken ct = default)
    {
        var storeKey = new StoreKey(key.ActorId, key.Scope, key.Key);

        
        if (_store.TryGetValue(storeKey, out var record))
        {
            record.Status = IdempotencyStatus.Completed;
            record.Data = data;
        }

        return Task.CompletedTask;
    }

    public Task ReleaseAsync(
        IdempotencyKey key,
        RequestFingerprint fingerprint,
        CancellationToken ct = default)
    {
        var storeKey = new StoreKey(
            key.ActorId,
            key.Scope,
            key.Key);

        if (_store.TryGetValue(storeKey, out var record) &&
            record.Status == IdempotencyStatus.InProgress &&
            record.Fingerprint == fingerprint)
        {
            _store.TryRemove(storeKey, out _);
        }

        return Task.CompletedTask;
    }

    private record StoreKey(string ActorId, string Scope, string Key);


    private sealed class InMemoryRecord
    {
        public RequestFingerprint Fingerprint { get; init; }
        public IdempotencyStatus Status { get; set; }
        public IdempotencyData? Data { get; set; }
    }
}
