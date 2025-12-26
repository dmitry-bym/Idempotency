using Idempotency.Core.Models;
using JetBrains.Annotations;

namespace Idempotency.Core.Abstractions;

[PublicAPI]
public interface IIdempotencyStore
{
    Task<IdempotencyClaim> ClaimAsync(
        IdempotencyKey key,
        RequestFingerprint fingerprint,
        CancellationToken ct = default);

    Task CompleteAsync(
        IdempotencyKey key,
        IdempotencyData data,
        CancellationToken ct = default);

    Task ReleaseAsync(
        IdempotencyKey key,
        RequestFingerprint fingerprint,
        CancellationToken ct = default);
}
