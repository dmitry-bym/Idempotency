using Idempotency.Core.Models;

namespace Idempotency.Core.Abstractions;

public interface IIdempotencyDecisionMaker
{
    ValueTask<IdempotencyDecision> DecideAsync(IdempotencyRequest request, IdempotencyClaim claim, CancellationToken ct = default);
}