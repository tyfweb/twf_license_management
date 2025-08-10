using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace TechWayFit.Licensing.Management.Web.Extensions
{
    /// <summary>
    /// Custom interceptor for Entity Framework Core database operations to provide detailed SQL logging
    /// </summary>
    public class SqlLoggingInterceptor : DbCommandInterceptor
    {
        private readonly ILogger<SqlLoggingInterceptor> _logger;

        public SqlLoggingInterceptor(ILogger<SqlLoggingInterceptor> logger)
        {
            _logger = logger;
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            LogCommand(command, "EXECUTING");
            return base.ReaderExecuting(command, eventData, result);
        }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            LogCommand(command, "EXECUTING ASYNC");
            return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            LogCommandResult(command, eventData, "EXECUTED");
            return base.ReaderExecuted(command, eventData, result);
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            LogCommandResult(command, eventData, "EXECUTED ASYNC");
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result)
        {
            LogCommand(command, "NON-QUERY EXECUTING");
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            LogCommand(command, "NON-QUERY EXECUTING ASYNC");
            return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override int NonQueryExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result)
        {
            LogCommandResult(command, eventData, "NON-QUERY EXECUTED", result);
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override async ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            LogCommandResult(command, eventData, "NON-QUERY EXECUTED ASYNC", result);
            return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result)
        {
            LogCommand(command, "SCALAR EXECUTING");
            return base.ScalarExecuting(command, eventData, result);
        }

        public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
        {
            LogCommand(command, "SCALAR EXECUTING ASYNC");
            return await base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override object? ScalarExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result)
        {
            LogCommandResult(command, eventData, "SCALAR EXECUTED", result);
            return base.ScalarExecuted(command, eventData, result);
        }

        public override async ValueTask<object?> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result,
            CancellationToken cancellationToken = default)
        {
            LogCommandResult(command, eventData, "SCALAR EXECUTED ASYNC", result);
            return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        {
            _logger.LogError(eventData.Exception, 
                "SQL COMMAND FAILED: {CommandText} | Duration: {ElapsedMs}ms | Exception: {ExceptionMessage}",
                command.CommandText,
                eventData.Duration.TotalMilliseconds,
                eventData.Exception?.Message);
            
            base.CommandFailed(command, eventData);
        }

        public override async Task CommandFailedAsync(
            DbCommand command,
            CommandErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            _logger.LogError(eventData.Exception, 
                "SQL COMMAND FAILED ASYNC: {CommandText} | Duration: {ElapsedMs}ms | Exception: {ExceptionMessage}",
                command.CommandText,
                eventData.Duration.TotalMilliseconds,
                eventData.Exception?.Message);
            
            await base.CommandFailedAsync(command, eventData, cancellationToken);
        }

        private void LogCommand(DbCommand command, string operation)
        {
            _logger.LogInformation("SQL {Operation}: {CommandText} | Parameters: {@Parameters}",
                operation,
                command.CommandText,
                command.Parameters.Cast<DbParameter>().Select(p => new { 
                    Name = p.ParameterName, 
                    Value = p.Value, 
                    Type = p.DbType.ToString() 
                }).ToArray());
        }

        private void LogCommandResult(DbCommand command, CommandExecutedEventData eventData, string operation, object? result = null)
        {
            var logMessage = "SQL {Operation}: {CommandText} | Duration: {ElapsedMs}ms";
            var logArgs = new object[] { 
                operation, 
                command.CommandText, 
                eventData.Duration.TotalMilliseconds 
            };

            if (result != null)
            {
                logMessage += " | Result: {Result}";
                logArgs = logArgs.Append(result).ToArray();
            }

            if (eventData.Duration.TotalMilliseconds > 1000) // Log slow queries as warnings
            {
                _logger.LogWarning(logMessage + " | ⚠️ SLOW QUERY", logArgs);
            }
            else
            {
                _logger.LogInformation(logMessage, logArgs);
            }
        }
    }

    /// <summary>
    /// Extension methods for configuring Entity Framework Core SQL logging
    /// </summary>
    public static class EntityFrameworkLoggingExtensions
    {
        /// <summary>
        /// Configure SQL logging for Entity Framework Core
        /// </summary>
        public static IServiceCollection ConfigureEfCoreLogging(this IServiceCollection services)
        {
            services.AddScoped<SqlLoggingInterceptor>();
            return services;
        }
    }
}
