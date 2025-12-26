using Idempotency.Core.Abstractions;
using Idempotency.Core.Models;

namespace Idempotency.Core.Services;

public sealed class IdempotencyDecisionMaker : IIdempotencyDecisionMaker
{
    public ValueTask<IdempotencyDecision> DecideAsync(IdempotencyRequest request, IdempotencyClaim claim, CancellationToken ct = default)
    {
        return ValueTask.FromResult(Decide(request, claim));
    }

    private IdempotencyDecision Decide(IdempotencyRequest request, IdempotencyClaim claim)
    {
        if (claim.StoredFingerprint != request.Fingerprint)
        {
            return new IdempotencyDecision(IdempotencyDecisionType.Reject);
        }

        if (claim.Status == IdempotencyStatus.Completed)
        {
            if (claim.Data is null)
            {
                return new IdempotencyDecision(IdempotencyDecisionType.Reject);
            }

            return new IdempotencyDecision(IdempotencyDecisionType.Replay, claim.Data);
        }

        if (!claim.IsOwner)
        {
            return new IdempotencyDecision(IdempotencyDecisionType.Reject);
        }

        return new IdempotencyDecision(IdempotencyDecisionType.Execute);
    }
}