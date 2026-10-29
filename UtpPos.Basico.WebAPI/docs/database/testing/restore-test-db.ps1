param(
    [string]$ServerInstance = "localhost",
    [string]$BackupPath = "",
    [string]$DatabaseName = "VENTAS_POS_TEST",
    [switch]$UseSqlAuth,
    [string]$UserId = "",
    [string]$Password = ""
)

$ErrorActionPreference = "Stop"

function Test-IsTestDatabaseName {
    param([string]$Name)

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return $false
    }

    return $Name.Trim().ToUpperInvariant().Contains("TEST")
}

function Test-IsForbiddenDatabaseName {
    param([string]$Name)

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return $true
    }

    $normalized = $Name.Trim().ToUpperInvariant()
    return $normalized -eq "VENTAS-POS" -or $normalized -eq "VENTAS_POS"
}

function Escape-SqlLiteral {
    param([string]$Value)
    return $Value.Replace("'", "''")
}

function Escape-SqlIdentifier {
    param([string]$Value)
    return $Value.Replace("]", "]] ").Replace("]] ", "]]")
}

function New-MasterConnectionString {
    param(
        [string]$Server,
        [switch]$SqlAuth,
        [string]$SqlUser,
        [string]$SqlPassword
    )

    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder["Data Source"] = $Server
    $builder["Initial Catalog"] = "master"
    $builder["TrustServerCertificate"] = $true

    if ($SqlAuth) {
        if ([string]::IsNullOrWhiteSpace($SqlUser) -or [string]::IsNullOrWhiteSpace($SqlPassword)) {
            throw "UseSqlAuth requiere UserId y Password."
        }

        $builder["User ID"] = $SqlUser
        $builder["Password"] = $SqlPassword
    }
    else {
        $builder["Integrated Security"] = $true
    }

    return $builder.ConnectionString
}

function Invoke-SqlNonQuery {
    param(
        [System.Data.SqlClient.SqlConnection]$Connection,
        [string]$CommandText,
        [int]$CommandTimeout = 0
    )

    $command = $Connection.CreateCommand()
    $command.CommandText = $CommandText
    $command.CommandTimeout = $CommandTimeout
    [void]$command.ExecuteNonQuery()
}

function Invoke-SqlScalar {
    param(
        [System.Data.SqlClient.SqlConnection]$Connection,
        [string]$CommandText
    )

    $command = $Connection.CreateCommand()
    $command.CommandText = $CommandText
    return $command.ExecuteScalar()
}

function Invoke-SqlDataTable {
    param(
        [System.Data.SqlClient.SqlConnection]$Connection,
        [string]$CommandText,
        [int]$CommandTimeout = 0
    )

    $command = $Connection.CreateCommand()
    $command.CommandText = $CommandText
    $command.CommandTimeout = $CommandTimeout
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
    $table = New-Object System.Data.DataTable
    [void]$adapter.Fill($table)
    return $table
}

if ([string]::IsNullOrWhiteSpace($BackupPath)) {
    $BackupPath = Join-Path $PSScriptRoot "..\bak\VENTAS-POS.bak"
}

$BackupPath = [System.IO.Path]::GetFullPath($BackupPath)

if (!(Test-Path -LiteralPath $BackupPath)) {
    throw "BackupPath no existe: $BackupPath"
}

if (Test-IsForbiddenDatabaseName -Name $DatabaseName) {
    throw "DatabaseName '$DatabaseName' esta bloqueado. Nunca restaure pruebas sobre VENTAS-POS/VENTAS_POS."
}

if (!(Test-IsTestDatabaseName -Name $DatabaseName)) {
    throw "DatabaseName '$DatabaseName' no parece ser de pruebas. Debe contener TEST."
}

$connectionString = New-MasterConnectionString -Server $ServerInstance -SqlAuth:$UseSqlAuth -SqlUser $UserId -SqlPassword $Password
$connection = New-Object System.Data.SqlClient.SqlConnection $connectionString

try {
    $connection.Open()

    $escapedBackupPath = Escape-SqlLiteral $BackupPath
    $fileList = Invoke-SqlDataTable -Connection $connection -CommandText "RESTORE FILELISTONLY FROM DISK = N'$escapedBackupPath';" -CommandTimeout 0

    $dataFiles = @($fileList | Where-Object { $_.Type -eq 'D' })
    $logFiles = @($fileList | Where-Object { $_.Type -eq 'L' })

    if ($dataFiles.Count -lt 1 -or $logFiles.Count -lt 1) {
        throw "El backup no contiene archivos data/log reconocibles."
    }

    $dataLogicalName = [string]$dataFiles[0]["LogicalName"]
    $logLogicalName = [string]$logFiles[0]["LogicalName"]

    $defaultDataPath = [string](Invoke-SqlScalar -Connection $connection -CommandText "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(4000));")
    $defaultLogPath = [string](Invoke-SqlScalar -Connection $connection -CommandText "SELECT CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS nvarchar(4000));")

    if ([string]::IsNullOrWhiteSpace($defaultDataPath)) {
        $defaultDataPath = [System.IO.Path]::GetDirectoryName([string](Invoke-SqlScalar -Connection $connection -CommandText "SELECT TOP (1) physical_name FROM sys.master_files WHERE database_id = DB_ID('master') AND type = 0;"))
    }

    if ([string]::IsNullOrWhiteSpace($defaultLogPath)) {
        $defaultLogPath = [System.IO.Path]::GetDirectoryName([string](Invoke-SqlScalar -Connection $connection -CommandText "SELECT TOP (1) physical_name FROM sys.master_files WHERE database_id = DB_ID('master') AND type = 1;"))
    }

    if ([string]::IsNullOrWhiteSpace($defaultDataPath) -or [string]::IsNullOrWhiteSpace($defaultLogPath)) {
        throw "No fue posible detectar rutas default de data/log de SQL Server."
    }

    $dataFilePath = Join-Path $defaultDataPath "$DatabaseName.mdf"
    $logFilePath = Join-Path $defaultLogPath "$($DatabaseName)_log.ldf"

    $escapedDatabaseName = Escape-SqlIdentifier $DatabaseName
    $databaseExists = [int](Invoke-SqlScalar -Connection $connection -CommandText "SELECT COUNT(1) FROM sys.databases WHERE name = N'$(Escape-SqlLiteral $DatabaseName)';")

    if ($databaseExists -gt 0) {
        if (!(Test-IsTestDatabaseName -Name $DatabaseName) -or (Test-IsForbiddenDatabaseName -Name $DatabaseName)) {
            throw "La BD existente '$DatabaseName' no es segura para REPLACE."
        }

        Write-Host "La base de datos test '$DatabaseName' ya existe. Se reemplazara con el backup indicado."
        Invoke-SqlNonQuery -Connection $connection -CommandText "ALTER DATABASE [$escapedDatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" -CommandTimeout 0
    }

    $restoreSql = @"
RESTORE DATABASE [$escapedDatabaseName]
FROM DISK = N'$(Escape-SqlLiteral $BackupPath)'
WITH
    MOVE N'$(Escape-SqlLiteral $dataLogicalName)' TO N'$(Escape-SqlLiteral $dataFilePath)',
    MOVE N'$(Escape-SqlLiteral $logLogicalName)' TO N'$(Escape-SqlLiteral $logFilePath)',
    REPLACE,
    RECOVERY,
    STATS = 5;
"@

    Invoke-SqlNonQuery -Connection $connection -CommandText $restoreSql -CommandTimeout 0
    Invoke-SqlNonQuery -Connection $connection -CommandText "ALTER DATABASE [$escapedDatabaseName] SET MULTI_USER;" -CommandTimeout 0

    Write-Host "Backup restaurado correctamente en $DatabaseName."

    if ($UseSqlAuth) {
        Write-Host "Configure:"
        Write-Host "UTPPOS_TEST_SQL_CONNECTION_STRING=Server=$ServerInstance;Database=$DatabaseName;User Id=$UserId;Password=***;TrustServerCertificate=True;"
    }
    else {
        Write-Host "Configure:"
        Write-Host "UTPPOS_TEST_SQL_CONNECTION_STRING=Server=$ServerInstance;Database=$DatabaseName;Trusted_Connection=True;TrustServerCertificate=True;"
    }
}
finally {
    if ($connection.State -ne [System.Data.ConnectionState]::Closed) {
        $connection.Close()
    }

    $connection.Dispose()
}
