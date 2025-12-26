using Idempotency.Core.Models;

namespace Idempotency.Core.Abstractions;

public interface IIdempotencyService
{
    Task<IdempotencyDecision> DecideAsync(
        IdempotencyRequest request,
        CancellationToken ct = default);

    Task CompleteAsync(
        IdempotencyRequest request,
        IdempotencyData data,
        CancellationToken ct = default);

    Task ReleaseAsync(
        IdempotencyRequest request,
        CancellationToken ct = default);
}