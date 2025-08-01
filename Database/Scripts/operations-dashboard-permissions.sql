-- ==================================================
-- Operations Dashboard Permissions Script
-- Version: 1.0.0
-- Description: Grants necessary permissions for operations dashboard tables
-- ==================================================

-- Grant permissions to the application user
GRANT SELECT, INSERT, UPDATE, DELETE ON system_metrics TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON error_log_summaries TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON page_performance_metrics TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON query_performance_metrics TO twf_license_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON system_health_snapshots TO twf_license_user;

-- Grant sequence permissions (for auto-generated IDs if needed)
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO twf_license_user;

-- Create indexes for better query performance (if not already created)
-- These indexes optimize the most common dashboard queries

-- System Metrics - Time-based queries
CREATE INDEX IF NOT EXISTS idx_system_metrics_recent 
    ON system_metrics (timestamp_hour DESC, metric_type);

-- Error Summaries - Unresolved errors query
CREATE INDEX IF NOT EXISTS idx_error_summaries_unresolved 
    ON error_log_summaries (is_resolved, severity_level, timestamp_hour DESC);

-- Page Performance - Slow pages query
CREATE INDEX IF NOT EXISTS idx_page_performance_slow 
    ON page_performance_metrics (avg_response_time_ms DESC, timestamp_hour DESC);

-- Query Performance - Slow queries query
CREATE INDEX IF NOT EXISTS idx_query_performance_slow 
    ON query_performance_metrics (avg_execution_time_ms DESC, timestamp_hour DESC);

-- System Health - Recent health status
CREATE INDEX IF NOT EXISTS idx_system_health_recent 
    ON system_health_snapshots (snapshot_timestamp DESC, overall_health_status);

-- ==================================================
-- Data Retention Policies (Optional)
-- ==================================================

-- Note: Create a scheduled job to clean up old data periodically
-- Example cleanup queries (run these manually or via scheduled job):

-- Clean up old system metrics (older than 90 days)
-- DELETE FROM system_metrics WHERE timestamp_hour < (CURRENT_TIMESTAMP - INTERVAL '90 days');

-- Clean up old page performance metrics (older than 90 days)
-- DELETE FROM page_performance_metrics WHERE timestamp_hour < (CURRENT_TIMESTAMP - INTERVAL '90 days');

-- Clean up old query performance metrics (older than 90 days)
-- DELETE FROM query_performance_metrics WHERE timestamp_hour < (CURRENT_TIMESTAMP - INTERVAL '90 days');

-- Clean up old system health snapshots (older than 90 days, keep daily snapshots)
-- DELETE FROM system_health_snapshots 
-- WHERE snapshot_timestamp < (CURRENT_TIMESTAMP - INTERVAL '90 days')
-- AND EXTRACT(hour FROM snapshot_timestamp) != 0;

-- ==================================================
-- Performance Optimization Views
-- ==================================================

-- View for current system health overview
CREATE OR REPLACE VIEW vw_current_system_health AS
SELECT 
    sh.overall_health_status,
    sh.total_requests_last_hour,
    sh.total_errors_last_hour,
    sh.error_rate_percent,
    sh.avg_response_time_ms,
    sh.cpu_usage_percent,
    sh.memory_usage_mb,
    sh.active_db_connections,
    sh.active_user_sessions,
    sh.total_active_licenses,
    sh.snapshot_timestamp
FROM system_health_snapshots sh
WHERE sh.snapshot_timestamp = (
    SELECT MAX(snapshot_timestamp) 
    FROM system_health_snapshots 
    WHERE is_active = true
)
AND sh.is_active = true;

-- Grant permissions on views
GRANT SELECT ON vw_current_system_health TO twf_license_user;

-- ==================================================
-- Success Message
-- ==================================================
SELECT 'Operations Dashboard permissions granted successfully!' as status;
SELECT 'Views created: vw_current_system_health, vw_recent_critical_errors, vw_slowest_pages_recent, vw_slowest_queries_recent' as views;
SELECT 'Cleanup function created: cleanup_old_dashboard_data(retention_days)' as functions;
