using Idempotency.Core.Abstractions;
using Idempotency.Core.Models;

namespace Idempotency.AspNet.Tests.Fakes;

internal sealed class FakeIdempotencyService : IIdempotencyService
{
    private readonly Queue<IdempotencyDecision> _decisions = new();

    public IdempotencyDecision DefaultDecision { get; set; } =
        new(IdempotencyDecisionType.Execute);

    public List<(IdempotencyRequest Request, IdempotencyData Data)> Completed { get; } = new();
    public List<IdempotencyRequest> Released { get; } = new();

    public void EnqueueDecision(IdempotencyDecision decision)
        => _decisions.Enqueue(decision);

    public Task<IdempotencyDecision> DecideAsync(IdempotencyRequest request, CancellationToken ct = default)
    {
        if (_decisions.Count > 0)
        {
            return Task.FromResult(_decisions.Dequeue());
        }

        return Task.FromResult(DefaultDecision);
    }

    public Task CompleteAsync(IdempotencyRequest request, IdempotencyData data, CancellationToken ct = default)
    {
        Completed.Add((request, data));
        return Task.CompletedTask;
    }

    public Task ReleaseAsync(IdempotencyRequest request, CancellationToken ct = default)
    {
        Released.Add(request);
        return Task.CompletedTask;
    }
}
