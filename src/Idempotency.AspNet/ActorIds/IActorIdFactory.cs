using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNet.ActorIds;

public interface IActorIdFactory
{
    public Task<string?> ResolveActorId(HttpContext context);
}