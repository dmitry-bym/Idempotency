using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNetCore.Scopes;

public interface IScopeFactory
{
    public Task<string?> ResolveScope(HttpContext context);
}