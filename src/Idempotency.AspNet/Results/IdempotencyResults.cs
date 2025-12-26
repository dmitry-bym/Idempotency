using Idempotency.AspNet.Infrastructure;
using Idempotency.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNet.Results;

internal sealed class IdempotencyReplayResult(IdempotencyData data) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        return IdempotencyReplayData.ApplyToResponse(data, httpContext.Response);
    }
}

internal sealed class IdempotencyRejectResult(int statusCode, string message) : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType ??= "text/plain";

        if (!string.IsNullOrEmpty(message))
        {
            await httpContext.Response.WriteAsync(message, httpContext.RequestAborted);
        }
    }
}
