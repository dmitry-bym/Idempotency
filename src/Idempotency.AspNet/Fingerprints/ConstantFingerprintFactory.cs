using Idempotency.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNet.Fingerprints;

public class ConstantFingerprintFactory : IFingerprintFactory
{
    private static readonly Task<RequestFingerprint> FingerprintTask = Task.FromResult(new RequestFingerprint());
    
    public Task<RequestFingerprint> CreateAsync(HttpContext context, CancellationToken ct = default)
    {
        return FingerprintTask;
    }
}