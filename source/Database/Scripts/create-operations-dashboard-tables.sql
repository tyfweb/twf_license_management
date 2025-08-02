-- ==================================================
-- Operations Dashboard Tables Creation Script
-- Version: 1.1.0
-- Description: Creates tables for operations dashboard metrics collection with correct EF column mappings
-- 
-- Run this script to create/recreate the operations dashboard tables
-- Connection: postgresql://twf_license_user:M@n@s0000@localhost:5433/twf_license_management
-- ==================================================

-- Drop tables if they exist (for development/testing)
DROP TABLE IF EXISTS system_health_snapshots CASCADE;
DROP TABLE IF EXISTS query_performance_metrics CASCADE;
DROP TABLE IF EXISTS page_performance_metrics CASCADE;
DROP TABLE IF EXISTS error_log_summaries CASCADE;
DROP TABLE IF EXISTS system_metrics CASCADE;

-- ==================================================
-- 1. System Metrics Table (Real-time aggregated metrics)
-- ==================================================
CREATE TABLE system_metrics (
    metric_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    timestamp_hour TIMESTAMP WITH TIME ZONE NOT NULL,
    metric_type VARCHAR(50) NOT NULL, -- 'request', 'error', 'performance', 'api'
    controller VARCHAR(100),
    action VARCHAR(100),
    http_method VARCHAR(10),
    status_code INTEGER,
    
    -- Aggregated performance data
    request_count INTEGER NOT NULL DEFAULT 0,
    total_response_time_ms BIGINT NOT NULL DEFAULT 0,
    avg_response_time_ms INTEGER NOT NULL DEFAULT 0,
    min_response_time_ms INTEGER NOT NULL DEFAULT 0,
    max_response_time_ms INTEGER NOT NULL DEFAULT 0,
    
    -- Error tracking
    error_count INTEGER NOT NULL DEFAULT 0,
    timeout_count INTEGER NOT NULL DEFAULT 0,
    
    -- User tracking
    unique_users INTEGER NOT NULL DEFAULT 0,
    
    -- System info
    server_name VARCHAR(100),
    environment VARCHAR(50) NOT NULL DEFAULT 'Development',
    
    -- Audit fields
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL DEFAULT 'System',
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE
);

-- ==================================================
-- 2. Error Log Summaries Table
-- ==================================================
CREATE TABLE error_log_summaries (
    error_summary_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    timestamp_hour TIMESTAMP WITH TIME ZONE NOT NULL,
    
    -- Error classification
    error_type VARCHAR(100) NOT NULL, -- 'Exception', 'Validation', 'Authorization', etc.
    error_source VARCHAR(200), -- Controller.Action or service name
    error_message_hash VARCHAR(64) NOT NULL, -- Hash of error message for grouping
    error_message_sample TEXT, -- Sample error message (first occurrence)
    
    -- Occurrence tracking
    occurrence_count INTEGER NOT NULL DEFAULT 1,
    first_occurrence TIMESTAMP WITH TIME ZONE NOT NULL,
    last_occurrence TIMESTAMP WITH TIME ZONE NOT NULL,
    
    -- Impact assessment
    affected_users INTEGER NOT NULL DEFAULT 0,
    severity_level VARCHAR(20) NOT NULL DEFAULT 'Medium', -- 'Low', 'Medium', 'High', 'Critical'
    
    -- Resolution tracking
    is_resolved BOOLEAN NOT NULL DEFAULT false,
    resolved_by VARCHAR(100),
    resolved_on TIMESTAMP WITH TIME ZONE,
    resolution_notes TEXT,
    
    -- Context information
    user_agent_sample VARCHAR(500),
    ip_address_sample VARCHAR(45),
    correlation_id_sample UUID,
    stack_trace_hash VARCHAR(64),
    
    -- Audit fields
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL DEFAULT 'System',
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE
);

-- ==================================================
-- 3. Page Performance Metrics Table
-- ==================================================
CREATE TABLE page_performance_metrics (
    performance_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    timestamp_hour TIMESTAMP WITH TIME ZONE NOT NULL,
    
    -- Page identification
    controller VARCHAR(100) NOT NULL,
    action VARCHAR(100) NOT NULL,
    route_template VARCHAR(200),
    
    -- Performance metrics
    request_count INTEGER NOT NULL DEFAULT 0,
    avg_response_time_ms INTEGER NOT NULL DEFAULT 0,
    min_response_time_ms INTEGER NOT NULL DEFAULT 0,
    max_response_time_ms INTEGER NOT NULL DEFAULT 0,
    p95response_time_ms INTEGER NOT NULL DEFAULT 0,
    p99response_time_ms INTEGER NOT NULL DEFAULT 0,
    
    -- Slow request tracking
    slow_request_count INTEGER NOT NULL DEFAULT 0, -- >2000ms
    very_slow_request_count INTEGER NOT NULL DEFAULT 0, -- >5000ms
    
    -- Success/failure rates
    success_count INTEGER NOT NULL DEFAULT 0,
    client_error_count INTEGER NOT NULL DEFAULT 0, -- 4xx
    server_error_count INTEGER NOT NULL DEFAULT 0, -- 5xx
    
    -- Resource usage
    avg_memory_usage_mb DECIMAL(10,2),
    avg_cpu_usage_percent DECIMAL(5,2),
    
    -- User experience
    bounce_rate_percent DECIMAL(5,2),
    conversion_rate_percent DECIMAL(5,2),
    
    -- Audit fields
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL DEFAULT 'System',
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE
);

-- ==================================================
-- 4. Query Performance Metrics Table
-- ==================================================
CREATE TABLE query_performance_metrics (
    query_metric_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    timestamp_hour TIMESTAMP WITH TIME ZONE NOT NULL,
    
    -- Query identification
    query_hash VARCHAR(64) NOT NULL, -- Hash of normalized query
    query_type VARCHAR(50) NOT NULL, -- 'SELECT', 'INSERT', 'UPDATE', 'DELETE'
    table_names VARCHAR(500), -- Comma-separated list of tables
    operation_context VARCHAR(200), -- Controller.Action that triggered query
    
    -- Performance metrics
    execution_count INTEGER NOT NULL DEFAULT 0,
    total_execution_time_ms BIGINT NOT NULL DEFAULT 0,
    avg_execution_time_ms INTEGER NOT NULL DEFAULT 0,
    min_execution_time_ms INTEGER NOT NULL DEFAULT 0,
    max_execution_time_ms INTEGER NOT NULL DEFAULT 0,
    
    -- Query complexity
    rows_affected_avg INTEGER NOT NULL DEFAULT 0,
    rows_affected_max INTEGER NOT NULL DEFAULT 0,
    
    -- Slow query tracking
    slow_query_count INTEGER NOT NULL DEFAULT 0, -- >1000ms
    very_slow_query_count INTEGER NOT NULL DEFAULT 0, -- >5000ms
    timeout_count INTEGER NOT NULL DEFAULT 0,
    
    -- Sample information
    query_sample TEXT, -- Sample query (first occurrence)
    parameters_sample_json JSONB, -- Sample parameters
    
    -- Optimization suggestions
    needs_optimization BOOLEAN NOT NULL DEFAULT false,
    optimization_notes TEXT,
    
    -- Audit fields
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL DEFAULT 'System',
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE
);

-- ==================================================
-- 5. System Health Snapshots Table
-- ==================================================
CREATE TABLE system_health_snapshots (
    snapshot_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    snapshot_timestamp TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Application metrics
    total_requests_last_hour INTEGER NOT NULL DEFAULT 0,
    total_errors_last_hour INTEGER NOT NULL DEFAULT 0,
    error_rate_percent DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    avg_response_time_ms INTEGER NOT NULL DEFAULT 0,
    
    -- System resources
    cpu_usage_percent DECIMAL(5,2),
    memory_usage_mb DECIMAL(10,2),
    disk_usage_percent DECIMAL(5,2),
    
    -- Database metrics
    active_db_connections INTEGER,
    db_pool_usage_percent DECIMAL(5,2),
    avg_db_query_time_ms INTEGER,
    
    -- Application health
    uptime_minutes INTEGER,
    active_user_sessions INTEGER,
    cache_hit_rate_percent DECIMAL(5,2),
    
    -- License management specific metrics
    total_active_licenses INTEGER,
    licenses_validated_last_hour INTEGER,
    license_validation_error_rate DECIMAL(5,2),
    
    -- Health status
    overall_health_status VARCHAR(20) NOT NULL DEFAULT 'Healthy', -- 'Healthy', 'Warning', 'Critical'
    health_issues_json JSONB, -- Array of current health issues
    
    -- Audit fields
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_by VARCHAR(100) NOT NULL DEFAULT 'System',
    updated_by VARCHAR(100),
    created_on TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_on TIMESTAMP WITH TIME ZONE
);

-- ==================================================
-- INDEXES FOR PERFORMANCE
-- ==================================================

-- System Metrics Indexes
CREATE INDEX idx_system_metrics_timestamp_type ON system_metrics (timestamp_hour, metric_type);
CREATE INDEX idx_system_metrics_controller_action ON system_metrics (controller, action);
CREATE INDEX idx_system_metrics_status_code ON system_metrics (status_code);
CREATE INDEX idx_system_metrics_created_on ON system_metrics (created_on);

-- Error Log Summaries Indexes
CREATE INDEX idx_error_summaries_timestamp ON error_log_summaries (timestamp_hour);
CREATE INDEX idx_error_summaries_type_source ON error_log_summaries (error_type, error_source);
CREATE INDEX idx_error_summaries_hash ON error_log_summaries (error_message_hash);
CREATE INDEX idx_error_summaries_severity ON error_log_summaries (severity_level);
CREATE INDEX idx_error_summaries_resolved ON error_log_summaries (is_resolved);

-- Page Performance Indexes
CREATE INDEX idx_page_performance_timestamp ON page_performance_metrics (timestamp_hour);
CREATE INDEX idx_page_performance_controller_action ON page_performance_metrics (controller, action);
CREATE INDEX idx_page_performance_slow_requests ON page_performance_metrics (slow_request_count);
CREATE INDEX idx_page_performance_avg_time ON page_performance_metrics (avg_response_time_ms);

-- Query Performance Indexes
CREATE INDEX idx_query_performance_timestamp ON query_performance_metrics (timestamp_hour);
CREATE INDEX idx_query_performance_hash ON query_performance_metrics (query_hash);
CREATE INDEX idx_query_performance_type ON query_performance_metrics (query_type);
CREATE INDEX idx_query_performance_slow ON query_performance_metrics (slow_query_count);
CREATE INDEX idx_query_performance_context ON query_performance_metrics (operation_context);

-- System Health Indexes
CREATE INDEX idx_system_health_timestamp ON system_health_snapshots (snapshot_timestamp);
CREATE INDEX idx_system_health_status ON system_health_snapshots (overall_health_status);
CREATE INDEX idx_system_health_created_on ON system_health_snapshots (created_on);

-- ==================================================
-- PARTITIONING FOR LARGE DATASETS (Optional - for high-volume environments)
-- ==================================================

-- Uncomment these if you expect high volume (>1M records per month)
-- 
-- -- Partition system_metrics by month
-- ALTER TABLE system_metrics PARTITION BY RANGE (timestamp_hour);
-- CREATE TABLE system_metrics_2025_08 PARTITION OF system_metrics
--     FOR VALUES FROM ('2025-08-01') TO ('2025-09-01');
-- 
-- -- Partition error_log_summaries by month
-- ALTER TABLE error_log_summaries PARTITION BY RANGE (timestamp_hour);
-- CREATE TABLE error_log_summaries_2025_08 PARTITION OF error_log_summaries
--     FOR VALUES FROM ('2025-08-01') TO ('2025-09-01');

-- ==================================================
-- CONSTRAINTS AND CHECKS
-- ==================================================

-- Add check constraints for data validation
ALTER TABLE system_metrics 
    ADD CONSTRAINT chk_system_metrics_counts CHECK (request_count >= 0 AND error_count >= 0);

ALTER TABLE system_metrics 
    ADD CONSTRAINT chk_system_metrics_times CHECK (avg_response_time_ms >= 0 AND min_response_time_ms >= 0 AND max_response_time_ms >= min_response_time_ms);

ALTER TABLE error_log_summaries 
    ADD CONSTRAINT chk_error_summaries_count CHECK (occurrence_count > 0);

ALTER TABLE error_log_summaries 
    ADD CONSTRAINT chk_error_summaries_users CHECK (affected_users >= 0);

ALTER TABLE error_log_summaries 
    ADD CONSTRAINT chk_error_summaries_severity CHECK (severity_level IN ('Low', 'Medium', 'High', 'Critical'));

ALTER TABLE page_performance_metrics 
    ADD CONSTRAINT chk_page_performance_counts CHECK (request_count >= 0 AND success_count >= 0);

ALTER TABLE page_performance_metrics 
    ADD CONSTRAINT chk_page_performance_times CHECK (avg_response_time_ms >= 0 AND min_response_time_ms >= 0 AND max_response_time_ms >= min_response_time_ms);

ALTER TABLE query_performance_metrics 
    ADD CONSTRAINT chk_query_performance_count CHECK (execution_count > 0);

ALTER TABLE query_performance_metrics 
    ADD CONSTRAINT chk_query_performance_times CHECK (avg_execution_time_ms >= 0 AND min_execution_time_ms >= 0 AND max_execution_time_ms >= min_execution_time_ms);

ALTER TABLE system_health_snapshots 
    ADD CONSTRAINT chk_system_health_rates CHECK (error_rate_percent >= 0 AND error_rate_percent <= 100);

ALTER TABLE system_health_snapshots 
    ADD CONSTRAINT chk_system_health_status CHECK (overall_health_status IN ('Healthy', 'Warning', 'Critical'));

-- ==================================================
-- SUCCESS MESSAGE
-- ==================================================
SELECT 'Operations Dashboard tables created successfully!' as status;
SELECT 'Tables created: system_metrics, error_log_summaries, page_performance_metrics, query_performance_metrics, system_health_snapshots' as tables;
SELECT 'All indexes and constraints applied successfully.' as indexes;
