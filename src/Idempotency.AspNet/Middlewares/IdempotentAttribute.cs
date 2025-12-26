namespace Idempotency.AspNet.Middlewares;

[AttributeUsage(AttributeTargets.Method)]
public class IdempotentAttribute : Attribute
{
    public string? Scope { get; set; }
}