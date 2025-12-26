using Idempotency.AspNetCore.Infrastructure;
using Idempotency.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNet.Tests.Infrastructure;

public class IdempotencyReplayDataTests
{
    [Fact]
    public async Task ApplyToResponse_sets_status_headers_and_content_type()
    {
        var data = new IdempotencyData(new Dictionary<string, string?>
        {
            ["status-code"] = "202",
            ["content-type"] = "text/plain",
            ["body"] = "hi",
            ["header-cache-control"] = "max-age=0"
        });

        var context = new DefaultHttpContext();

        var bodyStream = new MemoryStream();
        context.Response.Body = bodyStream;
        
        await IdempotencyReplayData.ApplyToResponse(data, context.Response);

        bodyStream.Seek(0, SeekOrigin.Begin);
        var streamReader = new StreamReader(bodyStream);
        var body = await streamReader.ReadToEndAsync();
        Assert.Equal(202, context.Response.StatusCode);
        Assert.Equal("text/plain", context.Response.ContentType);
        Assert.Equal("hi", body);
        Assert.Equal("max-age=0", context.Response.Headers["cache-control"]);
        Assert.Equal(202, context.Response.StatusCode);
        Assert.Equal("text/plain", context.Response.ContentType);
    }
}
