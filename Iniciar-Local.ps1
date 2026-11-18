param(
    [string]$Servidor = '(localdb)\MSSQLLocalDB',
    [string]$BaseDatos = 'TallerLab13DB'
)
$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$conexion = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$conexion['Data Source'] = $Servidor
$conexion['Initial Catalog'] = $BaseDatos
$conexion['Integrated Security'] = $true
$conexion['TrustServerCertificate'] = $true
$env:ConnectionStrings__TallerDB = $conexion.ConnectionString
if (-not $env:Jwt__Clave) {
    $bytes = New-Object byte[] 48
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($bytes)
    $rng.Dispose()
    $env:Jwt__Clave = [Convert]::ToBase64String($bytes)
}
if (-not $env:Jwt__Minutos) { $env:Jwt__Minutos = '30' }
dotnet run --project (Join-Path $PSScriptRoot 'Taller.Api')
