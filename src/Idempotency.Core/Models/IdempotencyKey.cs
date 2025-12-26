namespace Idempotency.Core.Models;

public readonly record struct IdempotencyKey(string ActorId, string Scope, string Key);