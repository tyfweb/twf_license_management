using System.Diagnostics;
using System.Runtime.InteropServices;
using TechWayFit.Licensing.Management.Core.Contracts.Services.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Implementations;
using TechWayFit.Licensing.Management.Infrastructure.Data.Context;

namespace TechWayFit.Licensing.Management.Web.Services
{
    /// <summary>
    /// Background service that periodically collects system health metrics
    /// </summary>
    public class SystemHealthCollectionService : BackgroundService
    {
        private readonly ILogger<SystemHealthCollectionService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _collectionInterval;

        public SystemHealthCollectionService(
            ILogger<SystemHealthCollectionService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            
            // Default to collecting every 30 seconds, configurable
            var intervalSeconds = configuration.GetValue<int>("OperationsDashboard:SystemHealthCollection:IntervalSeconds", 30);
            _collectionInterval = TimeSpan.FromSeconds(intervalSeconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("System Health Collection Service started with interval: {Interval}", _collectionInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CollectSystemHealthMetrics();
                    await Task.Delay(_collectionInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error collecting system health metrics");
                    // Wait a bit before retrying to avoid tight error loops
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            _logger.LogInformation("System Health Collection Service stopped");
        }

        private async Task CollectSystemHealthMetrics()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var metricsBuffer = scope.ServiceProvider
                    .GetService<MetricsBufferService>();

                if (metricsBuffer != null)
                {
                    var healthMetrics = await GatherSystemHealthData();

                    // Add health snapshot to buffer
                    await metricsBuffer.AddHealthSnapshotAsync(new
                    {
                        cpuUsagePercent = healthMetrics.CpuUsagePercent,
                        memoryUsagePercent = healthMetrics.MemoryUsagePercent,
                        memoryUsageMb = healthMetrics.MemoryUsageMb,
                        availableMemoryMb = healthMetrics.AvailableMemoryMb,
                        diskUsagePercent = healthMetrics.DiskUsagePercent,
                        diskAvailableGb = healthMetrics.DiskAvailableGb,
                        activeConnections = healthMetrics.ActiveConnections,
                        threadsCount = healthMetrics.ThreadsCount,
                        gcGen0Collections = healthMetrics.GcGen0Collections,
                        gcGen1Collections = healthMetrics.GcGen1Collections,
                        gcGen2Collections = healthMetrics.GcGen2Collections,
                        workingSetMb = healthMetrics.WorkingSetMb,
                        timestamp = DateTime.UtcNow
                    });

                    // Also record overall system metrics
                    await metricsBuffer.AddSystemMetricAsync(new
                    {
                        metricName = "SystemHealth",
                        metricValue = CalculateOverallHealthScore(healthMetrics),
                        metricUnit = "Score",
                        category = "System",
                        timestamp = DateTime.UtcNow
                    });

                    _logger.LogDebug("System health metrics buffered: CPU: {Cpu}%, Memory: {Memory}%, Disk: {Disk}%", 
                        healthMetrics.CpuUsagePercent, healthMetrics.MemoryUsagePercent, healthMetrics.DiskUsagePercent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to buffer system health metrics");
            }
        }

        private async Task<SystemHealthData> GatherSystemHealthData()
        {
            var healthData = new SystemHealthData();

            try
            {
                // Get current process
                using var currentProcess = Process.GetCurrentProcess();

                // Memory metrics
                var workingSet = currentProcess.WorkingSet64;
                healthData.WorkingSetMb = (int)(workingSet / 1024 / 1024);
                healthData.MemoryUsageMb = (int)(GC.GetTotalMemory(false) / 1024 / 1024);

                // GC metrics
                healthData.GcGen0Collections = GC.CollectionCount(0);
                healthData.GcGen1Collections = GC.CollectionCount(1);
                healthData.GcGen2Collections = GC.CollectionCount(2);

                // Thread count
                healthData.ThreadsCount = currentProcess.Threads.Count;

                // CPU usage (approximate)
                healthData.CpuUsagePercent = await GetCpuUsageAsync(currentProcess);

                // System memory info
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var memoryInfo = GetWindowsMemoryInfo();
                    healthData.MemoryUsagePercent = memoryInfo.UsagePercent;
                    healthData.AvailableMemoryMb = memoryInfo.AvailableMb;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    var memoryInfo = GetUnixMemoryInfo();
                    healthData.MemoryUsagePercent = memoryInfo.UsagePercent;
                    healthData.AvailableMemoryMb = memoryInfo.AvailableMb;
                }

                // Disk space info
                var diskInfo = GetDiskSpaceInfo();
                healthData.DiskUsagePercent = diskInfo.UsagePercent;
                healthData.DiskAvailableGb = diskInfo.AvailableGb;

                // Database connections (if available)
                healthData.ActiveConnections = await GetActiveDatabaseConnections();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Some system health metrics could not be collected");
            }

            return healthData;
        }

        private async Task<double> GetCpuUsageAsync(Process process)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;
                
                await Task.Delay(500); // Wait 500ms for measurement
                
                var endTime = DateTime.UtcNow;
                var endCpuUsage = process.TotalProcessorTime;
                
                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                
                return Math.Round(cpuUsageTotal * 100, 2);
            }
            catch
            {
                return 0; // Fallback if CPU measurement fails
            }
        }

        private (int UsagePercent, int AvailableMb) GetWindowsMemoryInfo()
        {
            try
            {
                // This is a simplified version - in production you might want to use WMI or native APIs
                var totalMemory = GC.GetTotalMemory(false);
                var workingSet = Environment.WorkingSet;
                
                // Rough estimation - in production use actual Windows APIs
                var usagePercent = Math.Min(90, (int)((workingSet / (double)(totalMemory + workingSet)) * 100));
                var availableMb = Math.Max(100, (int)((totalMemory / 1024 / 1024) * 0.7)); // Rough estimate
                
                return (usagePercent, availableMb);
            }
            catch
            {
                return (50, 1024); // Fallback values
            }
        }

        private (int UsagePercent, int AvailableMb) GetUnixMemoryInfo()
        {
            try
            {
                // Simple estimation for Unix systems
                var workingSet = Environment.WorkingSet;
                var totalMemory = GC.GetTotalMemory(false);
                
                var usagePercent = Math.Min(90, (int)((workingSet / (double)(totalMemory + workingSet)) * 100));
                var availableMb = Math.Max(100, (int)((totalMemory / 1024 / 1024) * 0.8));
                
                return (usagePercent, availableMb);
            }
            catch
            {
                return (50, 1024); // Fallback values
            }
        }

        private (int UsagePercent, long AvailableGb) GetDiskSpaceInfo()
        {
            try
            {
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                    .FirstOrDefault();

                if (drives != null)
                {
                    var totalSize = drives.TotalSize;
                    var availableSpace = drives.AvailableFreeSpace;
                    var usedSpace = totalSize - availableSpace;
                    
                    var usagePercent = (int)((usedSpace / (double)totalSize) * 100);
                    var availableGb = availableSpace / 1024 / 1024 / 1024;
                    
                    return (usagePercent, availableGb);
                }
                
                return (50, 10); // Fallback
            }
            catch
            {
                return (50, 10); // Fallback values
            }
        }

        private async Task<int> GetActiveDatabaseConnections()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<LicensingDbContext>();
                
                if (dbContext != null)
                {
                    // This is a rough estimate - actual implementation would query connection pools
                    return await Task.FromResult(1); // At least 1 if context is available
                }
                
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private double CalculateOverallHealthScore(SystemHealthData healthData)
        {
            // Simple health scoring algorithm
            var cpuScore = Math.Max(0, 100 - healthData.CpuUsagePercent);
            var memoryScore = Math.Max(0, 100 - healthData.MemoryUsagePercent);
            var diskScore = Math.Max(0, 100 - healthData.DiskUsagePercent);
            
            return Math.Round((cpuScore + memoryScore + diskScore) / 3.0, 2);
        }
    }

    /// <summary>
    /// Data structure for system health metrics
    /// </summary>
    public class SystemHealthData
    {
        public double CpuUsagePercent { get; set; }
        public int MemoryUsagePercent { get; set; }
        public int MemoryUsageMb { get; set; }
        public int AvailableMemoryMb { get; set; }
        public int DiskUsagePercent { get; set; }
        public long DiskAvailableGb { get; set; }
        public int ActiveConnections { get; set; }
        public int ThreadsCount { get; set; }
        public int GcGen0Collections { get; set; }
        public int GcGen1Collections { get; set; }
        public int GcGen2Collections { get; set; }
        public int WorkingSetMb { get; set; }
    }
}
