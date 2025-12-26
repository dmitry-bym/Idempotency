using Idempotency.AspNetCore.Helpers;
using Idempotency.AspNetCore.Infrastructure;
using Idempotency.AspNetCore.Options;
using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNet.Tests.Infrastructure;

public class ResponseDataBuilderTests
{
    [Fact]
    public async Task BuildData_CapturesStatusBodyAndHeaders()
    {
        var options = new IdempotencyAspNetOptions
        {
            HeadersToStore = ["Cache-Control"]
        };

        var context = new DefaultHttpContext
        {
            Response =
            {
                StatusCode = StatusCodes.Status201Created,
                ContentType = "text/plain",
                Headers =
                {
                    ["Cache-Control"] = "no-store"
                }
            }
        };

        var capturingStream = context.Response.EnableBuffering();
        await context.Response.WriteAsync("hello");

        var data = context.Response.BuildData(capturingStream, options);

        Assert.Equal("201", data.Data["status-code"]);
        Assert.Equal("text/plain", data.Data["content-type"]);
        Assert.Equal("hello", data.Data["body"]);
        Assert.Equal("no-store", data.Data["header-cache-control"]);
    }
}
