namespace Idempotency.Core.Models;

public sealed record IdempotencyData(
    IReadOnlyDictionary<string, string?> Data);
