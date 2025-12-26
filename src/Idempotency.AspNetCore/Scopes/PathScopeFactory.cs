using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNetCore.Scopes;

public class PathScopeFactory : IScopeFactory
{
    public Task<string?> ResolveScope(HttpContext context)
    {
        var scope = context.Request.Path.Value;
        return Task.FromResult(scope);
    }
}