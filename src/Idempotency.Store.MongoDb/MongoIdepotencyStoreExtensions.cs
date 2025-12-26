using Idempotency.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Idempotency.Store.MongoDb;

public static class MongoIdempotencyStoreExtensions
{
    public static IServiceCollection UseMongoIdempotencyStore(this IServiceCollection services, IMongoDatabase database,
        string collectionName = "idempotency_records", Action<MongoIdempotencyStoreOptions>? configure = null)
    {
        services.AddSingleton<IIdempotencyStore, MongoIdempotencyStore>(provider =>
        {
            return new MongoIdempotencyStore(database.GetCollection<IdempotencyRecord>(collectionName),
                provider.GetRequiredService<IOptions<MongoIdempotencyStoreOptions>>());
        });

        services.AddOptions<MongoIdempotencyStoreOptions>();

        if (configure is not null)
        {
            services.Configure(configure);
        }

        return services;
    }
}