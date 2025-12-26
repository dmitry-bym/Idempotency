# Idempotency

A flexible and extensible idempotency library for ASP.NET Core applications that helps prevent duplicate request processing by tracking and managing request uniqueness.

## Features

- 🔒 **Prevent Duplicate Processing** - Automatically detect and handle duplicate requests
- 🎯 **Flexible Actor Identification** - Support for authenticated users, anonymous users, or custom actor resolution
- 💾 **Multiple Storage Options** - In-memory store for development, MongoDB for production, or implement your own
- 🔑 **Request Fingerprinting** - Detect conflicting requests with different payloads using the same idempotency key
- ⚙️ **Highly Configurable** - Customize headers, status codes, response storage, and more
- 🚀 **Easy Integration** - Simple middleware-based setup for ASP.NET Core

## Installation

```bash
# Core package for ASP.NET Core
dotnet add package Idempotency.AspNetCore

# In-memory store (included in Core)
# No additional package needed

# MongoDB store (for production)
dotnet add package Idempotency.Store.MongoDb
```

## Quick Start

### Basic Setup with In-Memory Store

```csharp
using Idempotency.AspNetCore;
using Idempotency.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add idempotency services with in-memory store
builder.Services
    .AddIdempotency()
    .UseInMemoryIdempotencyStore();

var app = builder.Build();

// Add idempotency middleware
app.UseIdempotency();

// Mark endpoints as idempotent
app.MapPost("/orders", (Order order) =>
{
    // Your order processing logic
    return Results.Created($"/orders/{order.Id}", order);
})
.WithIdempotency("orders");

app.Run();
```

## Getting Started with ASP.NET Core

### 1. Configure Services

```csharp
using Idempotency.AspNetCore;
using Idempotency.AspNetCore.ActorIds;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddIdempotency(options =>
    {
        // Customize the idempotency key header name (default: "X-Idempotency-Key")
        options.HeaderName = "X-Idempotency-Key";

        // Customize conflict status code (default: 409 Conflict)
        options.ConflictStatusCode = StatusCodes.Status409Conflict;

        // Customize conflict message
        options.ConflictMessage = "Request is already in progress or conflicts with previous payload.";

        // Define which responses should be stored (default: 2xx status codes)
        options.ShouldStoreResponse = status => status is >= 200 and < 300;

        // Specify which headers to store with the response
        options.HeadersToStore = new[] { "Cache-Control", "Content-Encoding" };
    })
    .UseInMemoryIdempotencyStore(); // or UseMongoIdempotencyStore()

// Configure actor ID resolution (who is making the request)
// Option 1: Anonymous users (all requests treated as same actor)
builder.Services.AddSingleton<IActorIdFactory, AnonActorIdFactory>();

// Option 2: Authenticated users (default - uses ClaimTypes.NameIdentifier)
builder.Services.AddSingleton<IActorIdFactory>(
    new ClaimActorIdFactory(ClaimTypes.NameIdentifier));

// Option 3: Custom claim type
builder.Services.AddSingleton<IActorIdFactory>(
    new ClaimActorIdFactory("sub")); // or any custom claim type
```

### 2. Add Middleware

```csharp
var app = builder.Build();

// Add idempotency middleware (must be after UseRouting)
app.UseRouting();
app.UseIdempotency();

app.MapControllers();
app.Run();
```

### 3. Mark Endpoints as Idempotent

#### Minimal APIs

```csharp
// With automatic scope (uses request path)
app.MapPost("/payments", async (Payment payment) =>
{
    // Process payment
    return Results.Ok(new { transactionId = Guid.NewGuid() });
})
.WithIdempotency();

// With custom scope
app.MapPost("/orders", async (Order order) =>
{
    // Process order
    return Results.Created($"/orders/{order.Id}", order);
})
.WithIdempotency("orders");
```

## Storage Options

### In-Memory Store

Best for development and testing. Data is lost when the application restarts.

```csharp
builder.Services
    .AddIdempotency()
    .UseInMemoryIdempotencyStore();
```

**Pros:**
- No external dependencies
- Fast
- Simple setup

**Cons:**
- Data lost on restart
- Not suitable for distributed systems
- Limited to single instance

### MongoDB Store

Recommended for production environments.

```csharp
using Idempotency.Store.MongoDb;
using MongoDB.Driver;

var mongoClient = new MongoClient("mongodb://localhost:27017");
var database = mongoClient.GetDatabase("MyAppDatabase");

builder.Services
    .AddIdempotency()
    .UseMongoIdempotencyStore(
        database,
        collectionName: "idempotency_records", // optional, default shown
        configure: options =>
        {
            // How long to keep idempotency records (default: 30 minutes)
            options.TimeToLive = TimeSpan.FromHours(24);
        });
```

**Pros:**
- Persistent storage
- Works with distributed systems
- Automatic TTL (time-to-live) cleanup
- Production-ready

**Cons:**
- Requires MongoDB instance
- Additional infrastructure

**MongoDB Configuration:**
- Records are automatically indexed with TTL
- Old records are automatically cleaned up based on `TimeToLive` setting
- Collection is created automatically if it doesn't exist

### Custom Store Implementation

Implement your own storage backend by implementing the `IIdempotencyStore` interface:

```csharp
using Idempotency.Core.Abstractions;
using Idempotency.Core.Models;

public class CustomIdempotencyStore : IIdempotencyStore
{
    public async Task<IdempotencyClaim> ClaimAsync(
        IdempotencyKey key,
        RequestFingerprint fingerprint,
        CancellationToken ct = default)
    {
        // Try to claim ownership of this idempotency key
        // Return information about existing record if found
        // Key components: key.ActorId, key.Scope, key.Key
        // Fingerprint: fingerprint.Hash (for detecting payload conflicts)
        // Must be atomic

        // Example logic:
        // 1. Try to insert new record with status InProgress
        // 2. If insert succeeds, return IsOwner=true
        // 3. If record exists, return existing record data with IsOwner=false
    }

    public async Task CompleteAsync(
        IdempotencyKey key,
        IdempotencyData data,
        CancellationToken ct = default)
    {
        // Mark the request as completed and store the response data
        // data.Data contains response information (status code, headers, body)
        // Must be atomic
    }

    public async Task ReleaseAsync(
        IdempotencyKey key,
        RequestFingerprint fingerprint,
        CancellationToken ct = default)
    {
        // Release the claim if request processing failed
        // Only release if:
        // - Status is InProgress
        // - Fingerprint matches (same request)
        // Must be atomic
    }
}
```

Register your custom store:

```csharp
builder.Services.AddSingleton<IIdempotencyStore, CustomIdempotencyStore>();
```

**Key Concepts:**

- **IdempotencyKey**: Composite key consisting of `ActorId` (who), `Scope` (what), and `Key` (unique identifier)
- **RequestFingerprint**: Hash of the request payload to detect conflicts
- **IdempotencyClaim**: Result of claiming a key, indicates ownership and existing data
- **IdempotencyData**: Response data to store (status code, headers, body)

## How It Works

1. **Client sends request** with `X-Idempotency-Key` header
2. **Middleware intercepts** requests to endpoints marked with `.WithIdempotency()`
3. **Actor identification** resolves who is making the request (user ID, anonymous, etc.)
4. **Claim attempt** tries to claim ownership of the idempotency key
5. **Fingerprint check** compares request payload hash with stored fingerprint
6. **Decision made**:
   - **First request**: Process normally and store response
   - **Duplicate request**: Return stored response immediately
   - **Conflict**: Same key, different payload → return 409 Conflict
   - **In progress**: Another identical request is being processed → return 409 Conflict
7. **Response stored** (if successful and matches storage criteria)
8. **Claim released** (if request processing failed)

## Advanced Configuration

### Custom Actor ID Resolution

```csharp
using Idempotency.AspNetCore.ActorIds;
using Microsoft.AspNetCore.Http;

public class CustomActorIdFactory : IActorIdFactory
{
    public Task<string?> ResolveActorId(HttpContext context)
    {
        // Example: Use API key from header
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        return Task.FromResult(apiKey);

        // Example: Use combination of claims
        // var userId = context.User.FindFirstValue("user_id");
        // var tenantId = context.User.FindFirstValue("tenant_id");
        // return Task.FromResult($"{tenantId}:{userId}");
    }
}

builder.Services.AddSingleton<IActorIdFactory, CustomActorIdFactory>();
```

### Custom Scope Resolution

```csharp
using Idempotency.AspNetCore.Scopes;
using Microsoft.AspNetCore.Http;

public class CustomScopeFactory : IScopeFactory
{
    public Task<string?> ResolveScope(HttpContext context)
    {
        // Example: Use route pattern
        var endpoint = context.GetEndpoint();
        var routePattern = endpoint?.Metadata
            .GetMetadata<RouteEndpointMetadata>()?.RoutePattern;

        return Task.FromResult(routePattern);
    }
}

builder.Services.AddSingleton<IScopeFactory, CustomScopeFactory>();
```

### Custom Request Fingerprinting

```csharp
using Idempotency.AspNetCore.Fingerprints;
using Idempotency.Core.Models;
using Microsoft.AspNetCore.Http;

public class CustomFingerprintFactory : IFingerprintFactory
{
    public async Task<RequestFingerprint> CreateFingerprint(HttpContext context)
    {
        // Example: Hash request body + specific headers
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        var contentType = context.Request.ContentType;
        var combined = $"{contentType}:{body}";

        // Use your preferred hashing algorithm
        var hash = ComputeHash(combined);

        return new RequestFingerprint(hash);
    }

    private string ComputeHash(string input)
    {
        // Implement your hashing logic
        throw new NotImplementedException();
    }
}

builder.Services.AddSingleton<IFingerprintFactory, CustomFingerprintFactory>();
```

## License

[Add your license here]

## Contributing

[Add contributing guidelines here]
