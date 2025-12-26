using Idempotency.Core.Abstractions;
using Idempotency.Core.Models;
using Idempotency.Core.Services;
using Idempotency.Core.Stores;

namespace Idempotency.Core.Tests.Services;

public sealed class IdempotencyDecisionMakerTests
{
    private static IdempotencyRequest CreateRequest(
        string actor = "user",
        string scope = "orders",
        string key = "1",
        string fingerprint = "ff01")
    {
        return new IdempotencyRequest(
            actor,
            scope,
            key,
            new RequestFingerprint(fingerprint));
    }

    private static IdempotencyData CreateData(string body = "ok", int status = 200)
    {
        return new IdempotencyData(new Dictionary<string, string?>
        {
            ["body"] = body,
            ["statuscode"] = status.ToString()
        });
    }

    private static IIdempotencyService CreateService()
        => new IdempotencyService(new InMemoryIdempotencyStore(), new IdempotencyDecisionMaker());

    [Fact]
    public async Task First_call_executes()
    {
        var service = CreateService();
        var request = CreateRequest();

        var decision = await service.DecideAsync(request);

        Assert.Equal(IdempotencyDecisionType.Execute, decision.Type);
    }

    [Fact]
    public async Task Second_call_same_request_in_progress_is_rejected()
    {
        var service = CreateService();
        var request = CreateRequest();

        await service.DecideAsync(request); // first claim
        var second = await service.DecideAsync(request); // second claim should be rejected

        Assert.Equal(IdempotencyDecisionType.Reject, second.Type);
    }

    [Fact]
    public async Task Completed_request_replays_cached_data()
    {
        var service = CreateService();
        var request = CreateRequest();
        var data = CreateData(body: "cached", status: 201);

        await service.DecideAsync(request);
        await service.CompleteAsync(request, data);

        var replay = await service.DecideAsync(request);

        Assert.Equal(IdempotencyDecisionType.Replay, replay.Type);
        Assert.NotNull(replay.CachedData);
        Assert.Equal("cached", replay.CachedData!.Data["body"]);
        Assert.Equal("201", replay.CachedData!.Data["statuscode"]);
    }

    [Fact]
    public async Task Different_fingerprint_is_rejected()
    {
        var service = CreateService();
        var request = CreateRequest(fingerprint: "ff01");
        var differentRequest = CreateRequest(fingerprint: "ff02");

        await service.DecideAsync(request);
        var decision = await service.DecideAsync(differentRequest);

        Assert.Equal(IdempotencyDecisionType.Reject, decision.Type);
    }

    [Fact]
    public async Task Release_allows_subsequent_execute()
    {
        var service = CreateService();
        var request = CreateRequest();

        var first = await service.DecideAsync(request);
        Assert.Equal(IdempotencyDecisionType.Execute, first.Type);

        await service.ReleaseAsync(request);

        var second = await service.DecideAsync(request);
        Assert.Equal(IdempotencyDecisionType.Execute, second.Type);
    }
}
