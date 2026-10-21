using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TaskManagementSystem.Infrastructure.Performance
{
    public class QueryCounterInterceptor : DbCommandInterceptor
    {
        public override DbDataReader ReaderExecuted(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            DbDataReader result)
        {
            PerformanceTracker.IncrementQueryCount();
            return base.ReaderExecuted(command, eventData, result);
        }

        public override ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            DbDataReader result, 
            CancellationToken cancellationToken = default)
        {
            PerformanceTracker.IncrementQueryCount();
            return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override object? ScalarExecuted(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            object? result)
        {
            PerformanceTracker.IncrementQueryCount();
            return base.ScalarExecuted(command, eventData, result);
        }

        public override ValueTask<object?> ScalarExecutedAsync(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            object? result, 
            CancellationToken cancellationToken = default)
        {
            PerformanceTracker.IncrementQueryCount();
            return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override int NonQueryExecuted(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            int result)
        {
            PerformanceTracker.IncrementQueryCount();
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override ValueTask<int> NonQueryExecutedAsync(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            int result, 
            CancellationToken cancellationToken = default)
        {
            PerformanceTracker.IncrementQueryCount();
            return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }
    }
}
