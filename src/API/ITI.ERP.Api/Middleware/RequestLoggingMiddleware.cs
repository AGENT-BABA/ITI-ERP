using System.Diagnostics;
using Serilog;
using Serilog.Events;

namespace ITI.ERP.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private const string ActivitySourceName = "ITI.ERP.Api";

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        Log.Information(
            "HTTP {Method} {Path}{QueryString} requested by {ClientIp} [RequestId: {RequestId}]",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            requestId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var logLevel = context.Response.StatusCode switch
            {
                >= 500 => LogEventLevel.Error,
                >= 400 => LogEventLevel.Warning,
                _ => LogEventLevel.Information
            };

            Log.Write(logLevel,
                "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMs}ms [RequestId: {RequestId}]",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestId);
        }
    }
}
