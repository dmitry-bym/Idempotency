using Idempotency.AspNetCore.Helpers;
using Idempotency.AspNetCore.Infrastructure;
using Idempotency.AspNetCore.ActorIds;
using Idempotency.AspNetCore.Fingerprints;
using Idempotency.AspNetCore.Options;
using Idempotency.AspNetCore.Results;
using Idempotency.AspNetCore.Scopes;
using Idempotency.Core.Abstractions;
using Idempotency.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Idempotency.AspNetCore.Middlewares;

public class IdempotencyMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var attribute = context.GetEndpoint()?.Metadata.GetMetadata<IdempotentAttribute>();
        if (attribute is null)
        {
            await next(context);
            return;
        }
        
        var http = context;
        var options = http.RequestServices.GetRequiredService<IOptions<IdempotencyAspNetOptions>>().Value;

        var key = GetIdempotencyKey(http, options);
        if (key is null)
        {
            // regular case, we don't care if client don't want to pass key
            await next(context);
            return;
        }
        
        var actorIdFactory = http.GetRequiredService<IActorIdFactory>();
        var actorId = await actorIdFactory.ResolveActorId(http);
        if (string.IsNullOrWhiteSpace(actorId))
        {
            // log warning
            await next(context);
            return;
        }
        
        var scope = attribute.Scope ?? await http.GetRequiredService<IScopeFactory>().ResolveScope(http);
        if (string.IsNullOrWhiteSpace(scope))
        {
            //log warning
            await next(context);
            return;
        }
        
        var fingerprintFactory = http.GetRequiredService<IFingerprintFactory>();
        var fingerprint = await fingerprintFactory.CreateAsync(http, http.RequestAborted);

        var request = new IdempotencyRequest(actorId, scope, key, fingerprint);
        
        var service = http.RequestServices.GetRequiredService<IIdempotencyService>();
        
        var decision = await service.DecideAsync(request, http.RequestAborted);

        switch (decision.Type)
        {
            case IdempotencyDecisionType.Replay when decision.CachedData is not null:
            {
                await new IdempotencyReplayResult(decision.CachedData).ExecuteAsync(http);
                return;
            }
            case IdempotencyDecisionType.Replay:
            case IdempotencyDecisionType.Reject:
            {
                await new IdempotencyRejectResult(options.ConflictStatusCode, options.ConflictMessage).ExecuteAsync(http);
                return;
            }
        }

        var capturingStream = http.Response.EnableBuffering();

        try
        {
            await next(context);

            if (options.ShouldStoreResponse(http.Response.StatusCode))
            {
                var recorded = http.Response.BuildData(capturingStream, options);
                await service.CompleteAsync(request, recorded, http.RequestAborted);
            }
            else
            {
                await service.ReleaseAsync(request, http.RequestAborted);
            }
        }
        catch
        {
            await service.ReleaseAsync(request, http.RequestAborted);
            throw;
        }
    }
    
    public static string? GetIdempotencyKey(HttpContext httpContext, IdempotencyAspNetOptions options)
    {
        if (httpContext.Request.Headers.TryGetValue(options.HeaderName, out var keyValue) && !StringValues.IsNullOrEmpty(keyValue))
        {
            return keyValue.ToString();
        }

        return null;
    }
}