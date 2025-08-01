using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using TechWayFit.Licensing.Management.Core.Contracts.Services.OperationsDashboard;
using TechWayFit.Licensing.Management.Web.Services;

namespace TechWayFit.Licensing.Management.Web.Extensions
{
    /// <summary>
    /// Enhanced SQL interceptor that logs queries and captures metrics for operations dashboard
    /// </summary>
    public class EnhancedSqlInterceptor : DbCommandInterceptor
    {
        private readonly ILogger<EnhancedSqlInterceptor> _logger;
        private readonly IServiceProvider _serviceProvider;

        public EnhancedSqlInterceptor(
            ILogger<EnhancedSqlInterceptor> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            LogCommandResult(command, eventData, "EXECUTED");
            _ = RecordQueryMetricAsync(command, eventData, "SELECT");
            return base.ReaderExecuted(command, eventData, result);
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            LogCommandResult(command, eventData, "EXECUTED ASYNC");
            await RecordQueryMetricAsync(command, eventData, "SELECT");
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override int NonQueryExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result)
        {
            LogCommandResult(command, eventData, "NON-QUERY EXECUTED", result);
            _ = RecordQueryMetricAsync(command, eventData, GetQueryType(command.CommandText));
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override async ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            LogCommandResult(command, eventData, "NON-QUERY EXECUTED ASYNC", result);
            await RecordQueryMetricAsync(command, eventData, GetQueryType(command.CommandText));
            return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override object? ScalarExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result)
        {
            LogCommandResult(command, eventData, "SCALAR EXECUTED", result);
            _ = RecordQueryMetricAsync(command, eventData, "SCALAR");
            return base.ScalarExecuted(command, eventData, result);
        }

        public override async ValueTask<object?> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result,
            CancellationToken cancellationToken = default)
        {
            LogCommandResult(command, eventData, "SCALAR EXECUTED ASYNC", result);
            await RecordQueryMetricAsync(command, eventData, "SCALAR");
            return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        {
            _logger.LogError(eventData.Exception, 
                "SQL COMMAND FAILED: {CommandText} | Duration: {ElapsedMs}ms | Exception: {ExceptionMessage}",
                command.CommandText,
                eventData.Duration.TotalMilliseconds,
                eventData.Exception?.Message);
            
            _ = RecordQueryErrorAsync(command, eventData);
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
            
            await RecordQueryErrorAsync(command, eventData);
            await base.CommandFailedAsync(command, eventData, cancellationToken);
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

        private async Task RecordQueryMetricAsync(DbCommand command, CommandExecutedEventData eventData, string queryType)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var metricsBuffer = scope.ServiceProvider
                    .GetService<MetricsBufferService>();

                if (metricsBuffer != null)
                {
                    var duration = eventData.Duration.TotalMilliseconds;
                    var isSlowQuery = duration > 1000;
                    var table = ExtractTableName(command.CommandText);

                    // Add to in-memory buffer instead of immediate DB write
                    await metricsBuffer.AddQueryMetricAsync(new
                    {
                        queryType = queryType,
                        executionTimeMs = (int)duration,
                        tableName = table,
                        isSlowQuery = isSlowQuery,
                        parameterCount = command.Parameters.Count,
                        commandText = TruncateCommandText(command.CommandText),
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buffering query performance metric");
            }
        }

        private async Task RecordQueryErrorAsync(DbCommand command, CommandErrorEventData eventData)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var metricsBuffer = scope.ServiceProvider
                    .GetService<MetricsBufferService>();

                if (metricsBuffer != null)
                {
                    // Add to in-memory buffer instead of immediate DB write
                    await metricsBuffer.AddErrorMetricAsync(new
                    {
                        errorMessage = $"SQL Query Failed: {eventData.Exception?.Message}",
                        errorLevel = "Error",
                        queryType = GetQueryType(command.CommandText),
                        executionTimeMs = (int)eventData.Duration.TotalMilliseconds,
                        tableName = ExtractTableName(command.CommandText),
                        commandText = TruncateCommandText(command.CommandText),
                        exceptionType = eventData.Exception?.GetType().Name,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buffering query error metric");
            }
        }

        private static string GetQueryType(string commandText)
        {
            if (string.IsNullOrEmpty(commandText))
                return "UNKNOWN";

            var trimmed = commandText.Trim().ToUpperInvariant();
            
            if (trimmed.StartsWith("SELECT"))
                return "SELECT";
            if (trimmed.StartsWith("INSERT"))
                return "INSERT";
            if (trimmed.StartsWith("UPDATE"))
                return "UPDATE";
            if (trimmed.StartsWith("DELETE"))
                return "DELETE";
            if (trimmed.StartsWith("CREATE"))
                return "CREATE";
            if (trimmed.StartsWith("ALTER"))
                return "ALTER";
            if (trimmed.StartsWith("DROP"))
                return "DROP";
            
            return "OTHER";
        }

        private static string ExtractTableName(string commandText)
        {
            if (string.IsNullOrEmpty(commandText))
                return "Unknown";

            try
            {
                var trimmed = commandText.Trim().ToUpperInvariant();
                
                // Extract table name for different query types
                if (trimmed.StartsWith("SELECT"))
                {
                    var fromIndex = trimmed.IndexOf(" FROM ");
                    if (fromIndex > 0)
                    {
                        var afterFrom = commandText.Substring(fromIndex + 6).Trim();
                        var tableName = afterFrom.Split(' ', '\n', '\r', '\t')[0].Trim('`', '"', '[', ']');
                        return tableName.Length > 50 ? "Complex Query" : tableName;
                    }
                }
                else if (trimmed.StartsWith("INSERT INTO"))
                {
                    var intoIndex = trimmed.IndexOf("INSERT INTO");
                    if (intoIndex >= 0)
                    {
                        var afterInto = commandText.Substring(intoIndex + 11).Trim();
                        var tableName = afterInto.Split(' ', '(')[0].Trim('`', '"', '[', ']');
                        return tableName.Length > 50 ? "Complex Query" : tableName;
                    }
                }
                else if (trimmed.StartsWith("UPDATE"))
                {
                    var afterUpdate = commandText.Substring(6).Trim();
                    var tableName = afterUpdate.Split(' ')[0].Trim('`', '"', '[', ']');
                    return tableName.Length > 50 ? "Complex Query" : tableName;
                }
                else if (trimmed.StartsWith("DELETE FROM"))
                {
                    var fromIndex = trimmed.IndexOf("DELETE FROM");
                    if (fromIndex >= 0)
                    {
                        var afterFrom = commandText.Substring(fromIndex + 11).Trim();
                        var tableName = afterFrom.Split(' ')[0].Trim('`', '"', '[', ']');
                        return tableName.Length > 50 ? "Complex Query" : tableName;
                    }
                }
                
                return "Unknown";
            }
            catch
            {
                return "Parse Error";
            }
        }

        private static string TruncateCommandText(string commandText)
        {
            if (string.IsNullOrEmpty(commandText))
                return "";
            
            return commandText.Length > 500 ? 
                commandText.Substring(0, 500) + "..." : 
                commandText;
        }
    }
}
