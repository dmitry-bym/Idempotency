using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNet.ActorIds;

public class AnonActorIdFactory : IActorIdFactory
{
    public Task<string?> ResolveActorId(HttpContext context)
    {
        return Task.FromResult("anon")!;
    }
}