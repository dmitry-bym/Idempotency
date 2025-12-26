namespace Idempotency.Store.MongoDb;

public class MongoIdempotencyStoreOptions 
{
    public TimeSpan TimeToLive { get; set; } = TimeSpan.FromMinutes(30);
}