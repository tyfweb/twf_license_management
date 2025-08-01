// Quick test to insert sample operational data
// Add this as a controller action to test your dashboard

[HttpPost("test/insert-sample-data")]
public async Task<IActionResult> InsertSampleOperationalData()
{
    try
    {
        // Insert sample system health
        await _operationsDashboardService.RecordSystemHealthSnapshotAsync(new {
            CpuUsagePercent = 45.2,
            MemoryUsageMb = 1024,
            DiskUsagePercent = 60.0,
            ActiveDbConnections = 12,
            OverallHealthStatus = "Healthy",
            TotalRequestsLastHour = 1250,
            TotalErrorsLastHour = 3,
            AvgResponseTimeMs = 245.5
        });

        // Insert sample page performance
        await _operationsDashboardService.RecordPagePerformanceAsync(new {
            Controller = "License",
            Action = "Index",
            AvgResponseTimeMs = 120.5,
            MinResponseTimeMs = 45.2,
            MaxResponseTimeMs = 320.1,
            RequestCount = 156,
            ErrorRate = 0.5
        });

        // Insert sample error
        await _operationsDashboardService.RecordErrorAsync(new {
            errorMessage = "Database connection timeout occurred",
            errorLevel = "Warning",
            stackTrace = "Sample stack trace here"
        });

        // Insert sample query performance
        await _operationsDashboardService.RecordQueryPerformanceAsync(new {
            SqlCommand = "SELECT * FROM ProductLicenses WHERE Status = @status",
            ExecutionTimeMs = 85.2,
            TableNames = "ProductLicenses",
            QueryType = "SELECT",
            RecordCount = 245
        });

        return Ok("Sample data inserted successfully!");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error inserting sample data: {ex.Message}");
    }
}
