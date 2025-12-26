namespace Idempotency.Core.Models;

public sealed record IdempotencyClaim(
    bool IsOwner,
    IdempotencyStatus Status,
    RequestFingerprint StoredFingerprint,
    IdempotencyData? Data);
