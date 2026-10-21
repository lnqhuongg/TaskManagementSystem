using System.Diagnostics;

namespace TaskManagementSystem.Infrastructure.Performance
{
    public class RequestMetrics
    {
        public int QueryCount;
        public bool IsCacheHit;
        public Stopwatch Stopwatch = Stopwatch.StartNew();
    }

    public static class PerformanceTracker
    {
        private static readonly AsyncLocal<RequestMetrics?> _current = new();

        public static RequestMetrics Current => _current.Value ??= new RequestMetrics();

        public static void StartRequest()
        {
            _current.Value = new RequestMetrics();
        }

        public static int QueryCount => Current.QueryCount;

        public static void IncrementQueryCount()
        {
            Interlocked.Increment(ref Current.QueryCount);
        }

        public static void MarkCacheHit()
        {
            Current.IsCacheHit = true;
        }

        public static bool IsCacheHit => Current.IsCacheHit;

        public static double ElapsedMilliseconds =>
            Math.Round(Current.Stopwatch.Elapsed.TotalMilliseconds, 2);
    }
}
