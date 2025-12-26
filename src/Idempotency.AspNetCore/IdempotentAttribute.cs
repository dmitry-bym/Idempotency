namespace Idempotency.AspNetCore;

[AttributeUsage(AttributeTargets.Method)]
public class IdempotentAttribute : Attribute
{
    public string? Scope { get; set; }
}