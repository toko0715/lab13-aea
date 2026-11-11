param(
    [string]$Servidor = '(localdb)\MSSQLLocalDB',
    [string]$BaseDatos = 'TallerLab13DB'
)
$ErrorActionPreference = 'Stop'
$conexion = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$conexion.DataSource = $Servidor
$conexion.InitialCatalog = $BaseDatos
$conexion.IntegratedSecurity = $true
$conexion.TrustServerCertificate = $true
dotnet ef dbcontext scaffold $conexion.ConnectionString Microsoft.EntityFrameworkCore.SqlServer --project (Join-Path $PSScriptRoot 'Taller.Datos') --startup-project (Join-Path $PSScriptRoot 'Taller.Api') --output-dir ../Taller.Entidades/Modelos --context-dir . --namespace Taller.Entidades --context-namespace Taller.Datos --context TallerContext --no-onconfiguring --force
if ($LASTEXITCODE -ne 0) { throw 'Scaffold no terminó correctamente.' }
