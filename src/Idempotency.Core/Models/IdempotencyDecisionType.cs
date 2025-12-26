namespace Idempotency.Core.Models;

public enum IdempotencyDecisionType
{
    Execute = 1,
    Replay = 2,
    Reject = 3
}
