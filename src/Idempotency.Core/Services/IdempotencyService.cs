using Idempotency.Core.Abstractions;
using Idempotency.Core.Models;

namespace Idempotency.Core.Services;

public class IdempotencyService(IIdempotencyStore store, IIdempotencyDecisionMaker decisionMaker) : IIdempotencyService
{
    public async Task<IdempotencyDecision> DecideAsync(IdempotencyRequest request, CancellationToken ct = default)
    {
        var claim = await store.ClaimAsync(request.Key, request.Fingerprint, ct);
        return await decisionMaker.DecideAsync(request, claim, ct);
    }

    public Task CompleteAsync(IdempotencyRequest request, IdempotencyData data, CancellationToken ct = default)
    {
        return store.CompleteAsync(request.Key, data, ct);
    }

    public Task ReleaseAsync(IdempotencyRequest request, CancellationToken ct = default)
    {
        return store.ReleaseAsync(request.Key, request.Fingerprint, ct);
    }
}