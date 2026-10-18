using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TaskManagementSystem.Infrastructure.Performance
{
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMiddleware> _logger;

        public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            PerformanceTracker.StartRequest();

            context.Response.OnStarting(() =>
            {
                var queryCount = PerformanceTracker.QueryCount;
                var elapsedMs = PerformanceTracker.ElapsedMilliseconds;
                var isCacheHit = PerformanceTracker.IsCacheHit;

                context.Response.Headers["X-Query-Count"] = queryCount.ToString();
                context.Response.Headers["X-Response-Time-Ms"] = $"{elapsedMs} ms";
                if (isCacheHit)
                {
                    context.Response.Headers["X-Cache"] = "HIT";
                }

                return Task.CompletedTask;
            });

            await _next(context);

            var path = context.Request.Path.Value;
            var method = context.Request.Method;
            var count = PerformanceTracker.QueryCount;
            var time = PerformanceTracker.ElapsedMilliseconds;
            var cacheStatus = PerformanceTracker.IsCacheHit ? "[CACHE HIT] " : string.Empty;

            _logger.LogInformation(
                ">>> [PERFORMANCE] {Method} {Path} -> {CacheStatus}{QueryCount} queries in {ElapsedMs} ms",
                method, path, cacheStatus, count, time);
        }
    }
}
