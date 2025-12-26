using Idempotency.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNetCore.Fingerprints;

public interface IFingerprintFactory
{
    Task<RequestFingerprint> CreateAsync(HttpContext context, CancellationToken ct = default);
}