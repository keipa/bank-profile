using System.Collections.Concurrent;

namespace BankProfiles.Web.Presentation.Middleware;

public class CircuitRateLimitingMiddleware(
   RequestDelegate next,
   ILogger<CircuitRateLimitingMiddleware> logger,
   IConfiguration configuration)
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _circuitsByIp = new(StringComparer.OrdinalIgnoreCase);

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var circuitId = context.TraceIdentifier;

        // Check if this is a SignalR connection (Blazor circuit)
        var isBlazorRequest = context.Request.Path.StartsWithSegments("/_blazor");

        if (isBlazorRequest)
        {
            var maxCircuits = Math.Max(1, configuration.GetValue<int>("SignalRSettings:MaxCircuitsPerIP"));
            var circuits = _circuitsByIp.GetOrAdd(ipAddress, static _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal));
            var shouldReject = false;

            if (circuits.TryAdd(circuitId, 0))
            {
                var currentCount = circuits.Count;
                if (currentCount > maxCircuits)
                {
                    circuits.TryRemove(circuitId, out _);
                    shouldReject = true;
                    logger.LogWarning(
                        "Circuit rate limit exceeded for IP: {IP}. Current circuits: {Count}, Max allowed: {Max}",
                        ipAddress,
                        currentCount - 1,
                        maxCircuits);
                }
                else
                {
                    logger.LogInformation(
                        "Circuit {CircuitId} added for IP: {IP}. Total circuits: {Count}/{Max}",
                        circuitId,
                        ipAddress,
                        currentCount,
                        maxCircuits);
                }
            }

            if (shouldReject)
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync($"Too many concurrent connections. Maximum {maxCircuits} circuits per IP allowed.");
                return;
            }

            // Register cleanup on request completion
            context.Response.OnCompleted(() =>
            {
                if (_circuitsByIp.TryGetValue(ipAddress, out var trackedCircuits)
                    && trackedCircuits.TryRemove(circuitId, out _))
                {
                    var remainingCircuits = trackedCircuits.Count;
                    logger.LogInformation(
                        "Circuit {CircuitId} removed for IP: {IP}. Remaining circuits: {Count}",
                        circuitId,
                        ipAddress,
                        remainingCircuits);

                    if (remainingCircuits == 0)
                    {
                        _circuitsByIp.TryRemove(ipAddress, out _);
                    }
                }

                return Task.CompletedTask;
            });
        }

        await next(context);
    }

    // Cleanup method for periodic maintenance
    public static void CleanupStaleEntries()
    {
        foreach (var entry in _circuitsByIp)
        {
            if (entry.Value.IsEmpty)
            {
                _circuitsByIp.TryRemove(entry.Key, out _);
            }
        }
    }
}
