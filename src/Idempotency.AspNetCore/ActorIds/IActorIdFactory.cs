using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNetCore.ActorIds;

public interface IActorIdFactory
{
    public Task<string?> ResolveActorId(HttpContext context);
}