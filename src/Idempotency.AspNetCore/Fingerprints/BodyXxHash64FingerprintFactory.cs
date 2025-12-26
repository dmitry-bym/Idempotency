using System.IO.Hashing;
using Idempotency.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNetCore.Fingerprints;

public class BodyXxHash64FingerprintFactory : IFingerprintFactory
{
    public async Task<RequestFingerprint> CreateAsync(HttpContext context, CancellationToken ct = default)
    {
        var hasher = new XxHash64();

        context.Request.EnableBuffering();

        if (context.Request.Body.CanSeek)
        {
            context.Request.Body.Position = 0;
        }

        var buffer = new byte[81920];
        int read;

        while ((read = await context.Request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
        {
            hasher.Append(buffer.AsSpan(0, read));
        }

        if (context.Request.Body.CanSeek)
        {
            context.Request.Body.Position = 0;
        }

        var hash = Convert.ToHexString(hasher.GetCurrentHash());
        return new RequestFingerprint(hash);
    }
}