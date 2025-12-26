using Idempotency.AspNet.Middlewares;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace Idempotency.AspNet.Extensions;

public static class RouteHandlerBuilderExtensions
{
    [PublicAPI]
    public static TBuilder WithIdempotency<TBuilder>(this TBuilder builder, string? scope = null) where TBuilder : IEndpointConventionBuilder
    {
        return builder.WithMetadata(new IdempotentAttribute { Scope = scope });
    }
}
