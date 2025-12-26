using Idempotency.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNet.Infrastructure;

internal static class IdempotencyReplayData
{
    private const string HeaderPrefix = "header-";

    public static async Task ApplyToResponse(IdempotencyData data, HttpResponse response)
    {
        var statusCode = TryGetInt(data, "status-code") ?? StatusCodes.Status200OK;
        var contentType = TryGet(data, "content-type") ?? "application/json";
        var body = TryGet(data, "body") ?? string.Empty;

        foreach (var kvp in data.Data)
        {
            if (kvp.Key.StartsWith(HeaderPrefix, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(kvp.Value))
            {
                var headerName = kvp.Key.Substring(HeaderPrefix.Length);
                if (!string.IsNullOrWhiteSpace(headerName))
                {
                    response.Headers[headerName] = kvp.Value;
                }
            }
        }

        response.StatusCode = statusCode;
        response.ContentType = contentType;
        await response.WriteAsync(body);
    }

    private static string? TryGet(IdempotencyData data, string key)
    {
        return data.Data.GetValueOrDefault(key);
    }

    private static int? TryGetInt(IdempotencyData data, string key)
    {
        if (TryGet(data, key) is { } value && int.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return null;
    }
}
