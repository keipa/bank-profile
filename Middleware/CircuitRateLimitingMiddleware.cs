using System.Collections.Concurrent;

namespace BankProfiles.Web.Middleware;

public class CircuitRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CircuitRateLimitingMiddleware> _logger;
    private readonly IConfiguration _configuration;
    private static readonly ConcurrentDictionary<string, HashSet<string>> _circuitsByIp = new();
    private static readonly object _lock = new();

    public CircuitRateLimitingMiddleware(
        RequestDelegate next,
        ILogger<CircuitRateLimitingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var circuitId = context.TraceIdentifier;
        
        // Check if this is a SignalR connection (Blazor circuit)
        var isBlazorRequest = context.Request.Path.StartsWithSegments("/_blazor");
        
        if (isBlazorRequest)
        {
            var maxCircuits = _configuration.GetValue<int>("SignalRSettings:MaxCircuitsPerIP");
            bool shouldReject = false;
            
            lock (_lock)
            {
                if (!_circuitsByIp.ContainsKey(ipAddress))
                {
                    _circuitsByIp[ipAddress] = new HashSet<string>();
                }
                
                var circuits = _circuitsByIp[ipAddress];
                
                // Add current circuit
                if (!circuits.Contains(circuitId))
                {
                    if (circuits.Count >= maxCircuits)
                    {
                        shouldReject = true;
                        _logger.LogWarning(
                            "Circuit rate limit exceeded for IP: {IP}. Current circuits: {Count}, Max allowed: {Max}",
                            ipAddress, circuits.Count, maxCircuits);
                    }
                    else
                    {
                        circuits.Add(circuitId);
                        _logger.LogInformation("Circuit {CircuitId} added for IP: {IP}. Total circuits: {Count}/{Max}",
                            circuitId, ipAddress, circuits.Count, maxCircuits);
                    }
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
                lock (_lock)
                {
                    if (_circuitsByIp.TryGetValue(ipAddress, out var circuits))
                    {
                        circuits.Remove(circuitId);
                        _logger.LogInformation("Circuit {CircuitId} removed for IP: {IP}. Remaining circuits: {Count}",
                            circuitId, ipAddress, circuits.Count);
                        
                        // Clean up empty entries
                        if (circuits.Count == 0)
                        {
                            _circuitsByIp.TryRemove(ipAddress, out _);
                        }
                    }
                }
                return Task.CompletedTask;
            });
        }
        
        await _next(context);
    }
    
    // Cleanup method for periodic maintenance
    public static void CleanupStaleEntries()
    {
        lock (_lock)
        {
            var emptyIps = _circuitsByIp.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key).ToList();
            foreach (var ip in emptyIps)
            {
                _circuitsByIp.TryRemove(ip, out _);
            }
        }
    }
}
