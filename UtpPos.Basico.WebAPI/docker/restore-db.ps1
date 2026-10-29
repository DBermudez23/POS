<#
  Levanta SQL Server (docker compose) y restaura el .bak de desarrollo.

  Uso (desde esta carpeta):
    .\restore-db.ps1                      # restaura VENTAS-POS
    .\restore-db.ps1 -SetDevPassword      # ademas pone una clave conocida a TODOS los usuarios (solo BD local!)
    .\restore-db.ps1 -DatabaseName VENTAS_POS_TEST   # copia para los tests de integracion

  Requisitos: Docker Desktop corriendo, docker\.env creado (copia de .env.example)
  y el .bak en ..\Docs\database\bak\
#>
param(
    [string]$DatabaseName = 'VENTAS-POS',
    [string]$BackupFile   = 'UtpPos-VENTAS-POS.bak',
    [switch]$SetDevPassword
)
# 'Continue': PowerShell 5.1 trata la salida informativa de docker (stderr) como error; validamos $LASTEXITCODE a mano.
$ErrorActionPreference = 'Continue'
Set-Location $PSScriptRoot

if (-not (Test-Path .env)) { throw "Falta docker\.env. Copia .env.example a .env y ajusta la clave." }
$env = @{}
Get-Content .env | Where-Object { $_ -match '^\s*[^#=]+=' } | ForEach-Object { $k, $v = $_ -split '=', 2; $env[$k.Trim()] = $v.Trim() }
$sa = $env['UTPPOS_SA_PASSWORD']

if (-not (Test-Path "..\Docs\database\bak\$BackupFile")) {
    throw "No encuentro ..\Docs\database\bak\$BackupFile. Pide el .bak al equipo y copialo en esa carpeta."
}

Write-Host '1/3 Levantando SQL Server...' -ForegroundColor Cyan
docker compose up -d --wait
if ($LASTEXITCODE -ne 0) { throw 'docker compose fallo' }

function Invoke-Sql([string]$sql, [string]$db = 'master') {
    docker exec utppos-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -b -S localhost -U sa -P $sa -d $db -Q $sql
    if ($LASTEXITCODE -ne 0) { throw "SQL fallo: $sql" }
}

Write-Host "2/3 Restaurando $BackupFile como [$DatabaseName]..." -ForegroundColor Cyan
Invoke-Sql @"
IF DB_ID(N'$DatabaseName') IS NOT NULL ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [$DatabaseName] FROM DISK = N'/import/$BackupFile'
WITH REPLACE,
     MOVE 'POS-VENTAS'     TO '/var/opt/mssql/data/$DatabaseName.mdf',
     MOVE 'POS-VENTAS_log' TO '/var/opt/mssql/data/${DatabaseName}_log.ldf';
ALTER DATABASE [$DatabaseName] SET MULTI_USER;
"@

if ($SetDevPassword) {
    Write-Host '3/3 Asignando clave de desarrollo a los usuarios...' -ForegroundColor Cyan
    # Hash BCrypt (factor 12) de 'UtpPos#Dev2026'. SOLO para BD local, nunca en ambientes compartidos.
    Invoke-Sql "UPDATE TBL_USUARIOS SET USR_PASSWORD = '`$2a`$12`$WflFewBrISoMmsBesYp.le0cV9HtG8wXvbI5ID.dT335mXs1zAvc2'" $DatabaseName
    Write-Host '   Usuarios (clave: UtpPos#Dev2026):' -ForegroundColor Green
    Invoke-Sql 'SET NOCOUNT ON; SELECT USR_CORREO FROM TBL_USUARIOS' $DatabaseName
} else {
    Write-Host '3/3 (omitido: usa -SetDevPassword para tener una clave de login conocida)' -ForegroundColor DarkGray
}
Write-Host "Listo. Conexion: Server=127.0.0.1,$($env['UTPPOS_SQL_PORT']);Database=$DatabaseName;User Id=sa;Password=<tu clave de .env>;TrustServerCertificate=True;" -ForegroundColor Green
