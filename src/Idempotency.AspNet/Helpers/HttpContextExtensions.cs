using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Idempotency.AspNet.Helpers;

public static class HttpContextExtensions
{
    public static T GetRequiredService<T>(this HttpContext http) where T : notnull
    {
        return http.RequestServices.GetRequiredService<T>();
    } 
}