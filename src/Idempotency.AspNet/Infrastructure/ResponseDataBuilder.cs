using Idempotency.AspNet.Helpers;
using Idempotency.AspNet.Options;
using Idempotency.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNet.Infrastructure;

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
            ["body"] = body,
            ["status-code"] = response.StatusCode.ToString(),
            ["content-type"] = response.ContentType
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
                map[$"header-{normalized.ToLowerInvariant()}"] = value.ToString();
            }
        }

        return new IdempotencyData(map);
    }
}