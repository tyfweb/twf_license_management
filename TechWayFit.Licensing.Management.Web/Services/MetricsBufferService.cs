using System.Collections.Concurrent;
using TechWayFit.Licensing.Management.Core.Contracts.Services.OperationsDashboard;
using TechWayFit.Licensing.Management.Core.Matrices;

namespace TechWayFit.Licensing.Management.Web.Services
{
    /// <summary>
    /// Buffered metrics collection service that accumulates metrics in memory 
    /// and flushes them to database in batches for better performance
    /// </summary>
    public class MetricsBufferService : BackgroundService, IDisposable
    {
        private readonly ILogger<MetricsBufferService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        
        // Configuration
        private readonly int _bufferSize;
        private readonly TimeSpan _flushInterval;
        private readonly bool _enableBuffering;
        
        // Thread-safe collections for buffering metrics
        private readonly ConcurrentQueue<PerformanceMetricData> _performanceMetrics = new();
        private readonly ConcurrentQueue<ErrorMetricData> _errorMetrics = new();
        private readonly ConcurrentQueue<QueryMetricData> _queryMetrics = new();
        private readonly ConcurrentQueue<SystemMetricData> _systemMetrics = new();
        private readonly ConcurrentQueue<HealthSnapshotData> _healthSnapshots = new();
        
        // Counters for monitoring buffer sizes
        private volatile int _performanceCount = 0;
        private volatile int _errorCount = 0;
        private volatile int _queryCount = 0;
        private volatile int _systemCount = 0;
        private volatile int _healthCount = 0;

        public MetricsBufferService(
            ILogger<MetricsBufferService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            
            // Load configuration with defaults
            _bufferSize = configuration.GetValue<int>("OperationsDashboard:MetricsCollection:BufferSize", 1000);
            var flushIntervalSeconds = configuration.GetValue<int>("OperationsDashboard:MetricsCollection:FlushIntervalSeconds", 30);
            _flushInterval = TimeSpan.FromSeconds(flushIntervalSeconds);
            _enableBuffering = configuration.GetValue<bool>("OperationsDashboard:MetricsCollection:EnableBuffering", true);
            
            _logger.LogInformation("MetricsBufferService initialized - BufferSize: {BufferSize}, FlushInterval: {FlushInterval}s, Buffering: {Enabled}",
                _bufferSize, flushIntervalSeconds, _enableBuffering);
        }

        #region Public Methods for Adding Metrics

        public async Task AddPerformanceMetricAsync(object metricData)
        {
            if (!_enableBuffering)
            {
                // Direct flush mode - write immediately
                await FlushSinglePerformanceMetric(metricData);
                return;
            }

            var metric = new PerformanceMetricData
            {
                Data = metricData,
                Timestamp = DateTime.UtcNow
            };

            _performanceMetrics.Enqueue(metric);
            Interlocked.Increment(ref _performanceCount);

            // Force flush if buffer is full
            if (_performanceCount >= _bufferSize)
            {
                _ = Task.Run(async () => await FlushPerformanceMetrics());
            }
        }

        public async Task AddErrorMetricAsync(object metricData)
        {
            if (!_enableBuffering)
            {
                await FlushSingleErrorMetric(metricData);
                return;
            }

            var metric = new ErrorMetricData
            {
                Data = metricData,
                Timestamp = DateTime.UtcNow
            };

            _errorMetrics.Enqueue(metric);
            Interlocked.Increment(ref _errorCount);

            if (_errorCount >= _bufferSize)
            {
                _ = Task.Run(async () => await FlushErrorMetrics());
            }
        }

        public async Task AddQueryMetricAsync(SqlMetric metricData)
        {
            if (!_enableBuffering)
            {
                await FlushSingleQueryMetric(metricData);
                return;
            }

            var metric = new QueryMetricData
            {
                Data = metricData,
                Timestamp = DateTime.UtcNow
            };

            _queryMetrics.Enqueue(metric);
            Interlocked.Increment(ref _queryCount);

            if (_queryCount >= _bufferSize)
            {
                _ = Task.Run(async () => await FlushQueryMetrics());
            }
        }

        public async Task AddSystemMetricAsync(object metricData)
        {
            if (!_enableBuffering)
            {
                await FlushSingleSystemMetric(metricData);
                return;
            }

            var metric = new SystemMetricData
            {
                Data = metricData,
                Timestamp = DateTime.UtcNow
            };

            _systemMetrics.Enqueue(metric);
            Interlocked.Increment(ref _systemCount);

            if (_systemCount >= _bufferSize)
            {
                _ = Task.Run(async () => await FlushSystemMetrics());
            }
        }

        public async Task AddHealthSnapshotAsync(object metricData)
        {
            if (!_enableBuffering)
            {
                await FlushSingleHealthSnapshot(metricData);
                return;
            }

            var metric = new HealthSnapshotData
            {
                Data = metricData,
                Timestamp = DateTime.UtcNow
            };

            _healthSnapshots.Enqueue(metric);
            Interlocked.Increment(ref _healthCount);

            if (_healthCount >= _bufferSize)
            {
                _ = Task.Run(async () => await FlushHealthSnapshots());
            }
        }

        #endregion

        #region Background Service Implementation

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_enableBuffering)
            {
                _logger.LogInformation("MetricsBufferService running in direct-flush mode (buffering disabled)");
                // Still run the service but just for monitoring/cleanup
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                return;
            }

            _logger.LogInformation("MetricsBufferService started with {FlushInterval} flush interval", _flushInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_flushInterval, stoppingToken);
                    await FlushAllMetrics();
                }
                catch (OperationCanceledException)
                {
                    // Expected during shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during metrics buffer flush cycle");
                    // Continue running even if flush fails
                }
            }

            // Final flush on shutdown
            try
            {
                _logger.LogInformation("MetricsBufferService shutting down - performing final flush");
                await FlushAllMetrics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during final metrics flush");
            }

            _logger.LogInformation("MetricsBufferService stopped");
        }

        #endregion

        #region Private Flush Methods

        private async Task FlushAllMetrics()
        {
            var tasks = new List<Task>();

            if (_performanceCount > 0) tasks.Add(FlushPerformanceMetrics());
            if (_errorCount > 0) tasks.Add(FlushErrorMetrics());
            if (_queryCount > 0) tasks.Add(FlushQueryMetrics());
            if (_systemCount > 0) tasks.Add(FlushSystemMetrics());
            if (_healthCount > 0) tasks.Add(FlushHealthSnapshots());

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
                _logger.LogDebug("Flushed all metrics buffers - P:{Performance}, E:{Errors}, Q:{Queries}, S:{System}, H:{Health}",
                    _performanceCount, _errorCount, _queryCount, _systemCount, _healthCount);
            }
        }

        private async Task FlushPerformanceMetrics()
        {
            var metricsToFlush = new List<PerformanceMetricData>();
            
            // Dequeue all current metrics
            while (_performanceMetrics.TryDequeue(out var metric))
            {
                metricsToFlush.Add(metric);
                Interlocked.Decrement(ref _performanceCount);
            }

            if (metricsToFlush.Count == 0) return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<IOperationsDashboardService>();

                // Bulk insert - pass all metrics at once
                var bulkData = metricsToFlush.Select(m => m.Data).ToList();
                await dashboardService.RecordPagePerformanceBulkAsync(bulkData);

                _logger.LogDebug("Bulk flushed {Count} performance metrics to database", metricsToFlush.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk flush {Count} performance metrics", metricsToFlush.Count);
                // Could implement retry logic or dead letter queue here
            }
        }

        private async Task FlushErrorMetrics()
        {
            var metricsToFlush = new List<ErrorMetricData>();
            
            while (_errorMetrics.TryDequeue(out var metric))
            {
                metricsToFlush.Add(metric);
                Interlocked.Decrement(ref _errorCount);
            }

            if (metricsToFlush.Count == 0) return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<IOperationsDashboardService>();

                // Bulk insert - pass all metrics at once
                var bulkData = metricsToFlush.Select(m => m.Data).ToList();
                await dashboardService.RecordErrorsBulkAsync(bulkData);

                _logger.LogDebug("Bulk flushed {Count} error metrics to database", metricsToFlush.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk flush {Count} error metrics", metricsToFlush.Count);
            }
        }

        private async Task FlushQueryMetrics()
        {
            var metricsToFlush = new List<QueryMetricData>();
            
            while (_queryMetrics.TryDequeue(out var metric))
            {
                metricsToFlush.Add(metric);
                Interlocked.Decrement(ref _queryCount);
            }

            if (metricsToFlush.Count == 0) return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<IOperationsDashboardService>();

                // Bulk insert - pass all metrics at once
                var bulkData = metricsToFlush.Select(m => m.Data).ToList();
                await dashboardService.RecordQueryPerformanceBulkAsync(bulkData);

                _logger.LogDebug("Bulk flushed {Count} query metrics to database", metricsToFlush.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk flush {Count} query metrics", metricsToFlush.Count);
            }
        }

        private async Task FlushSystemMetrics()
        {
            var metricsToFlush = new List<SystemMetricData>();
            
            while (_systemMetrics.TryDequeue(out var metric))
            {
                metricsToFlush.Add(metric);
                Interlocked.Decrement(ref _systemCount);
            }

            if (metricsToFlush.Count == 0) return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<IOperationsDashboardService>();

                foreach (var metric in metricsToFlush)
                {
                    await dashboardService.RecordSystemMetricAsync(metric.Data);
                }

                _logger.LogDebug("Flushed {Count} system metrics to database", metricsToFlush.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to flush {Count} system metrics", metricsToFlush.Count);
            }
        }

        private async Task FlushHealthSnapshots()
        {
            var metricsToFlush = new List<HealthSnapshotData>();
            
            while (_healthSnapshots.TryDequeue(out var metric))
            {
                metricsToFlush.Add(metric);
                Interlocked.Decrement(ref _healthCount);
            }

            if (metricsToFlush.Count == 0) return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<IOperationsDashboardService>();

                // Bulk insert - pass all metrics at once
                var bulkData = metricsToFlush.Select(m => m.Data).ToList();
                await dashboardService.RecordSystemHealthSnapshotsBulkAsync(bulkData);

                _logger.LogDebug("Bulk flushed {Count} health snapshots to database", metricsToFlush.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk flush {Count} health snapshots", metricsToFlush.Count);
            }
        }

        #endregion

        #region Direct Flush Methods (when buffering disabled)

        private async Task FlushSinglePerformanceMetric(object metricData)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<IOperationsDashboardService>();
                await dashboardService.RecordPagePerformanceAsync(metricData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record performance metric directly");
            }
        }

        private async Task FlushSingleErrorMetric(object metricData)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<IOperationsDashboardService>();
                await dashboardService.RecordErrorAsync(metricData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record error metric directly");
            }
        }

        private async Task FlushSingleQueryMetric(SqlMetric metricData)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<IOperationsDashboardService>();
                await dashboardService.RecordQueryPerformanceAsync(metricData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record query metric directly");
            }
        }

        private async Task FlushSingleSystemMetric(object metricData)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<IOperationsDashboardService>();
                await dashboardService.RecordSystemMetricAsync(metricData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record system metric directly");
            }
        }

        private async Task FlushSingleHealthSnapshot(object metricData)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dashboardService = scope.ServiceProvider.GetRequiredService<IOperationsDashboardService>();
                await dashboardService.RecordSystemHealthSnapshotAsync(metricData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record health snapshot directly");
            }
        }

        #endregion

        #region Buffer Status Methods

        public BufferStatus GetBufferStatus()
        {
            return new BufferStatus
            {
                PerformanceMetricsCount = _performanceCount,
                ErrorMetricsCount = _errorCount,
                QueryMetricsCount = _queryCount,
                SystemMetricsCount = _systemCount,
                HealthSnapshotsCount = _healthCount,
                TotalBufferedItems = _performanceCount + _errorCount + _queryCount + _systemCount + _healthCount,
                BufferSizeLimit = _bufferSize,
                FlushInterval = _flushInterval,
                BufferingEnabled = _enableBuffering
            };
        }

        #endregion
    }

    #region Data Classes

    public class PerformanceMetricData
    {
        public object Data { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }

    public class ErrorMetricData
    {
        public object Data { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }

    public class QueryMetricData
    {
        public SqlMetric Data { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }

    public class SystemMetricData
    {
        public object Data { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }

    public class HealthSnapshotData
    {
        public object Data { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }

    public class BufferStatus
    {
        public int PerformanceMetricsCount { get; set; }
        public int ErrorMetricsCount { get; set; }
        public int QueryMetricsCount { get; set; }
        public int SystemMetricsCount { get; set; }
        public int HealthSnapshotsCount { get; set; }
        public int TotalBufferedItems { get; set; }
        public int BufferSizeLimit { get; set; }
        public TimeSpan FlushInterval { get; set; }
        public bool BufferingEnabled { get; set; }
    }

    #endregion
}
