# Laboratorio 13 Taller mecánico

API REST con controladores ASP.NET Core en .NET 10, EF Core 10 y AutoMapper 16. El taller registra clientes y sus vehículos. Un cliente puede tener varios vehículos y cada vehículo pertenece a un cliente.

## Ejecutar

Requiere .NET SDK 10 o superior, SQL Server LocalDB, sqlcmd y PowerShell.

1. Ejecuta TallerDB.sql en SQL Server. El script y los datos de ejemplo están en la raíz.
2. Desde esta carpeta ejecuta los comandos siguientes.

```powershell
sqlcmd -S '(localdb)\MSSQLLocalDB' -E -C -b -f 65001 -i TallerDB.sql
dotnet dev-certs https --trust
pwsh -File Iniciar-Local.ps1
```

Swagger está disponible en https://localhost:7143/swagger en el entorno Development.
Iniciar-Local.ps1 construye la configuración local mediante autenticación integrada y la pasa por una variable de entorno. Para usar otra instancia, indica los parámetros Servidor y BaseDatos. El repositorio conserva la configuración de la aplicación sin credenciales ni cadenas de conexión.

## Base de datos primero

Las entidades de Taller.Entidades/Modelos y TallerContext se generaron mediante Scaffold de EF Core a partir de TallerDB.sql. La generación usa un directorio de salida en Entidades y un namespace independiente para el contexto de Datos.

Para regenerar instala la herramienta y ejecuta el script desde esta carpeta.

```powershell
dotnet tool install --global dotnet-ef --version 10.0.13
pwsh -File Scaffold.ps1
```

## Capas y reglas

Las referencias directas forman la cadena Api → Negocio → Datos → Entidades.

- Entidades contiene las entidades generadas y los DTOs de lectura, creación y actualización. Los DTOs de entrada incluyen Data Annotations.
- Datos contiene TallerContext y un repositorio con su interfaz por entidad. Usa consultas LINQ asíncronas.
- Negocio contiene los servicios con sus interfaces y un único MappingProfile.
- Api recibe DTOs y llama a las interfaces de los servicios. Program.cs registra los servicios, AutoMapper y la persistencia mediante AddPersistenciaTaller, que encapsula el registro del contexto y los repositorios.

ClienteServicio rechaza DNI duplicados y la eliminación de clientes con vehículos. VehiculoServicio rechaza placas duplicadas y propietarios inexistentes. Normaliza las placas a mayúsculas. Las reglas lanzan ReglaNegocioException y los controladores devuelven 409. Los índices únicos y la clave foránea también impiden inconsistencias entre solicitudes concurrentes.

## Endpoints

| Método | Ruta | Resultado correcto |
| --- | --- | --- |
| GET | /api/clientes?nombre=Ana | 200 |
| GET | /api/clientes/{id} | 200 |
| POST | /api/clientes | 201 con Location |
| PUT | /api/clientes/{id} | 204 |
| DELETE | /api/clientes/{id} | 204 |
| GET | /api/clientes/{id}/vehiculos | 200 |
| GET | /api/vehiculos?placa=ABC&marca=Toyota&clienteId=1 | 200 |
| GET | /api/vehiculos/{id} | 200 |
| POST | /api/vehiculos | 201 con Location |
| PUT | /api/vehiculos/{id} | 204 |
| DELETE | /api/vehiculos/{id} | 204 |

Los DTOs inválidos devuelven 400, los recursos inexistentes devuelven 404 y las reglas incumplidas devuelven 409.

## Pruebas

Con la API encendida ejecuta lo siguiente. Python usa únicamente su biblioteca estándar.

```powershell
python Probar-Api.py https://localhost:7143 entrega
```

La prueba realiza 39 solicitudes HTTP y 15 comprobaciones adicionales. Comprueba los once endpoints, los códigos 200, 201, 204, 400, 404 y 409, los mapeos, la persistencia, los filtros y solicitudes concurrentes con el mismo DNI. Elimina los registros de prueba que creó. El resultado JSON se guarda en entrega, carpeta excluida de Git.

AutoMapper valida todos los mapeos al arrancar. La versión 16 muestra un aviso de licencia para desarrollo y pruebas.
