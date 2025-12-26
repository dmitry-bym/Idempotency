using Idempotency.AspNetCore.Middlewares;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace Idempotency.AspNetCore.Extensions;

public static class RouteHandlerBuilderExtensions
{
    [PublicAPI]
    public static TBuilder WithIdempotency<TBuilder>(this TBuilder builder, string? scope = null) where TBuilder : IEndpointConventionBuilder
    {
        return builder.WithMetadata(new IdempotentAttribute { Scope = scope });
    }
}
