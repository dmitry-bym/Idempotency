using System.Security.Claims;
using Idempotency.AspNet.ActorIds;
using Idempotency.AspNet.Fingerprints;
using Idempotency.AspNet.Middlewares;
using Idempotency.AspNet.Options;
using Idempotency.AspNet.Scopes;
using Idempotency.Core.Abstractions;
using Idempotency.Core.Services;
using Idempotency.Core.Stores;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Idempotency.AspNet;

public static class ServiceCollectionExtensions
{
    [PublicAPI]
    public static IServiceCollection AddIdempotency(this IServiceCollection services, Action<IdempotencyAspNetOptions>? configure = null)
    {
        services.TryAddSingleton<IFingerprintFactory, ConstantFingerprintFactory>();
        services.TryAddSingleton<IScopeFactory, PathScopeFactory>();
        services.TryAddSingleton<IActorIdFactory>(new ClaimActorIdFactory(ClaimTypes.NameIdentifier));
     
        services.TryAddSingleton<IIdempotencyService, IdempotencyService>();
        services.TryAddSingleton<IIdempotencyDecisionMaker, IdempotencyDecisionMaker>();
        services.AddSingleton<IdempotencyMiddleware>();
        
        services.AddOptions<IdempotencyAspNetOptions>();

        if (configure is not null)
        {
            services.Configure(configure);
        }

        return services;
    }

    [PublicAPI]
    public static IServiceCollection UseInMemoryIdempotencyStore(this IServiceCollection services)
    {
        services.TryAddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
        return services;
    }

    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app)
    {
        app.UseMiddleware<IdempotencyMiddleware>();
        return app;
    }
}
