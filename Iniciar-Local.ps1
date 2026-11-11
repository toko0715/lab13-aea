param(
    [string]$Servidor = '(localdb)\MSSQLLocalDB',
    [string]$BaseDatos = 'TallerLab13DB'
)
$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$conexion = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$conexion.DataSource = $Servidor
$conexion.InitialCatalog = $BaseDatos
$conexion.IntegratedSecurity = $true
$conexion.TrustServerCertificate = $true
$env:ConnectionStrings__TallerDB = $conexion.ConnectionString
dotnet run --project (Join-Path $PSScriptRoot 'Taller.Api')
