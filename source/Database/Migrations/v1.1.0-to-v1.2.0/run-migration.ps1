# =====================================================
# TechWayFit Licensing Management System
# PowerShell Migration Runner
# 
# Purpose: Run tier-based licensing database migration
# Author: System Migration
# Date: 2025-08-05
# =====================================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Host = "localhost",
    
    [Parameter(Mandatory=$false)]
    [string]$Port = "5432",
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "licensing_management",
    
    [Parameter(Mandatory=$false)]
    [string]$Username = "postgres",
    
    [Parameter(Mandatory=$false)]
    [string]$Password = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$Rollback = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$ValidateOnly = $false
)

# Function to display colored output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    
    $originalColor = $Host.UI.RawUI.ForegroundColor
    $Host.UI.RawUI.ForegroundColor = $Color
    Write-Output $Message
    $Host.UI.RawUI.ForegroundColor = $originalColor
}

# Function to check if psql is available
function Test-PostgreSQLClient {
    try {
        $null = Get-Command psql -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

# Main execution
try {
    Write-ColorOutput "=======================================" "Cyan"
    Write-ColorOutput "TechWayFit Licensing Migration Runner" "Cyan"
    Write-ColorOutput "Tier-Based Licensing Migration v1.2.0" "Cyan"
    Write-ColorOutput "=======================================" "Cyan"
    
    # Check prerequisites
    if (-not (Test-PostgreSQLClient)) {
        Write-ColorOutput "ERROR: PostgreSQL client (psql) not found in PATH" "Red"
        Write-ColorOutput "Please install PostgreSQL client tools" "Yellow"
        exit 1
    }
    
    # Build connection string if not provided
    if ([string]::IsNullOrEmpty($ConnectionString)) {
        if ([string]::IsNullOrEmpty($Password)) {
            $Password = Read-Host "Enter PostgreSQL password" -AsSecureString
            $Password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password))
        }
        $ConnectionString = "postgresql://${Username}:${Password}@${Host}:${Port}/${Database}"
    }
    
    # Get script directory
    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    
    # Determine which script to run
    $ScriptToRun = ""
    $Operation = ""
    
    if ($Rollback) {
        $ScriptToRun = Join-Path $ScriptDir "999-rollback-tier-based-licensing.sql"
        $Operation = "ROLLBACK"
        Write-ColorOutput "WARNING: This will rollback tier-based licensing changes!" "Yellow"
        $confirm = Read-Host "Are you sure you want to continue? (y/N)"
        if ($confirm -ne "y" -and $confirm -ne "Y") {
            Write-ColorOutput "Operation cancelled." "Yellow"
            exit 0
        }
    }
    elseif ($ValidateOnly) {
        $ScriptToRun = Join-Path $ScriptDir "validate-migration.sql"
        $Operation = "VALIDATION"
    }
    else {
        $ScriptToRun = Join-Path $ScriptDir "run-migration.sql"
        $Operation = "MIGRATION"
    }
    
    # Check if script exists
    if (-not (Test-Path $ScriptToRun)) {
        Write-ColorOutput "ERROR: Script not found: $ScriptToRun" "Red"
        exit 1
    }
    
    Write-ColorOutput "Starting $Operation..." "Green"
    Write-ColorOutput "Database: $Database" "Gray"
    Write-ColorOutput "Host: $Host:$Port" "Gray"
    Write-ColorOutput "Script: $(Split-Path -Leaf $ScriptToRun)" "Gray"
    Write-ColorOutput ""
    
    # Create log file
    $LogFile = Join-Path $ScriptDir "migration-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
    
    # Execute the script
    $env:PGPASSWORD = $Password.Split('@')[0].Split(':')[-1]  # Extract password from connection string
    
    $psqlArgs = @(
        "-h", $Host
        "-p", $Port
        "-U", $Username
        "-d", $Database
        "-f", $ScriptToRun
        "-v", "ON_ERROR_STOP=1"
        "--echo-queries"
    )
    
    Write-ColorOutput "Executing PostgreSQL script..." "Yellow"
    
    $process = Start-Process -FilePath "psql" -ArgumentList $psqlArgs -NoNewWindow -Wait -PassThru -RedirectStandardOutput $LogFile -RedirectStandardError "$LogFile.error"
    
    if ($process.ExitCode -eq 0) {
        Write-ColorOutput ""
        Write-ColorOutput "$Operation completed successfully!" "Green"
        Write-ColorOutput "Log file: $LogFile" "Gray"
        
        # Display validation results if available
        if (Test-Path "$LogFile.error") {
            $errorContent = Get-Content "$LogFile.error" -Raw
            if (-not [string]::IsNullOrEmpty($errorContent.Trim())) {
                Write-ColorOutput ""
                Write-ColorOutput "Warnings/Notices:" "Yellow"
                Write-ColorOutput $errorContent "Yellow"
            }
        }
    }
    else {
        Write-ColorOutput ""
        Write-ColorOutput "$Operation FAILED!" "Red"
        Write-ColorOutput "Check log files for details:" "Red"
        Write-ColorOutput "Output: $LogFile" "Red"
        Write-ColorOutput "Errors: $LogFile.error" "Red"
        
        if (Test-Path "$LogFile.error") {
            Write-ColorOutput ""
            Write-ColorOutput "Error details:" "Red"
            Get-Content "$LogFile.error" | Write-Host -ForegroundColor Red
        }
        
        exit $process.ExitCode
    }
}
catch {
    Write-ColorOutput ""
    Write-ColorOutput "EXCEPTION: $($_.Exception.Message)" "Red"
    Write-ColorOutput "Stack Trace: $($_.ScriptStackTrace)" "Red"
    exit 1
}
finally {
    # Clean up environment variable
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

Write-ColorOutput ""
Write-ColorOutput "=======================================" "Cyan"
Write-ColorOutput "Next Steps:" "Cyan"
Write-ColorOutput "1. Update your application code" "White"
Write-ColorOutput "2. Test tier-based license creation" "White"
Write-ColorOutput "3. Verify existing licenses still work" "White"
Write-ColorOutput "=======================================" "Cyan"
