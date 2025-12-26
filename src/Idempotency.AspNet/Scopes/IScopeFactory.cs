using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNet.Scopes;

public interface IScopeFactory
{
    public Task<string?> ResolveScope(HttpContext context);
}