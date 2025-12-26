using Idempotency.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNetCore.Infrastructure;

internal static class IdempotencyReplayData
{
    private const string DefaultContentType = "application/json";

    public static async Task ApplyToResponse(IdempotencyData data, HttpResponse response)
    {
        var statusCode = TryGetInt(data, DataKeys.StatusCode) ?? StatusCodes.Status200OK;
        var contentType = TryGet(data, DataKeys.ContentType) ?? DefaultContentType;
        var body = TryGet(data, DataKeys.Body) ?? string.Empty;

        foreach (var kvp in data.Data)
        {
            if (kvp.Key.StartsWith(DataKeys.HeaderPrefix, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(kvp.Value))
            {
                var headerName = kvp.Key.Substring(DataKeys.HeaderPrefix.Length);
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
