using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNet.ActorIds;

public class ClaimActorIdFactory(string claimType) : IActorIdFactory
{
    public Task<string?> ResolveActorId(HttpContext context)
    {
        var id = context.User.FindFirstValue(claimType);
        return Task.FromResult(id);
    }
}