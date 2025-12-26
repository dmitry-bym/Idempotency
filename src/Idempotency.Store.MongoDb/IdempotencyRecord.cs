using Idempotency.Core.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace Idempotency.Store.MongoDb;

public sealed class IdempotencyRecord
{
    [BsonId]
    public string Id { get; set; } = null!;

    public string ActorId { get; set; } = null!;
    public string Scope { get; set; } = null!;
    public string Key { get; set; } = null!;

    public string? Fingerprint { get; set; }
    
    public IdempotencyStatus Status { get; set; }
    
    public IReadOnlyDictionary<string, string?>? Data { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
