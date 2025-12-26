using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNet.Options;

public sealed class IdempotencyAspNetOptions
{
    public string HeaderName { get; set; } = "X-Idempotency-Key";

    public int ConflictStatusCode { get; set; } = StatusCodes.Status409Conflict;

    public string ConflictMessage { get; set; } = "Request is already in progress or conflicts with previous payload.";

    public Func<int, bool> ShouldStoreResponse { get; set; } = status => status is >= 200 and < 300;

    public string[] HeadersToStore { get; set; } = ["Cache-Control", "Content-Encoding"];
}
