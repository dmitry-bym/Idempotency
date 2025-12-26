namespace Idempotency.Core.Models;

public sealed record IdempotencyDecision(
    IdempotencyDecisionType Type,
    IdempotencyData? CachedData = null);
