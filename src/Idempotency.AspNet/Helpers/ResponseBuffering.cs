using Microsoft.AspNetCore.Http;

namespace Idempotency.AspNet.Helpers;

public static class ResponseBuffering
{
    public static ResponseCapturingStream EnableBuffering(this HttpResponse response, long bufferLimit = 1024 * 1024 /* 1MB default */ )
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.Body is ResponseCapturingStream rcs)
            return rcs;
        
        var body = response.Body;
        var capturingStream = new ResponseCapturingStream(body, bufferLimit);
        response.Body = capturingStream;
        response.HttpContext.Response.RegisterForDispose(capturingStream);

        return capturingStream;
    }
}