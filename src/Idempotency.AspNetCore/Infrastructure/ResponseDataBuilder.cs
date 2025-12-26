using Idempotency.AspNetCore.Helpers;
using Idempotency.AspNetCore.Options;
using Idempotency.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNetCore.Infrastructure;

internal static class ResponseDataBuilder
{
    public static IdempotencyData BuildData(
        this HttpResponse response,
        ResponseCapturingStream capturingStream,
        IdempotencyAspNetOptions options)
    {
        var body = capturingStream.GetCapturedContent();

        var map = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            [DataKeys.Body] = body,
            [DataKeys.StatusCode] = response.StatusCode.ToString(),
            [DataKeys.ContentType] = response.ContentType
        };

        foreach (var headerName in options.HeadersToStore)
        {
            if (string.IsNullOrWhiteSpace(headerName))
            {
                continue;
            }

            var normalized = headerName.Trim();

            if (response.Headers.TryGetValue(normalized, out var value))
            {
                map[$"{DataKeys.HeaderPrefix}{normalized.ToLowerInvariant()}"] = value.ToString();
            }
        }

        return new IdempotencyData(map);
    }
}