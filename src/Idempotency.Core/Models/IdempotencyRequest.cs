namespace Idempotency.Core.Models;

public sealed record IdempotencyRequest(
    IdempotencyKey Key,
    RequestFingerprint Fingerprint)
{
    public IdempotencyRequest(
        string actorId,
        string scope,
        string key,
        RequestFingerprint fingerprint) : this(new IdempotencyKey(actorId, scope, key), fingerprint) { }
}
