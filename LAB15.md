# Laboratorio 15 Microservicios para un taller mecánico

El gateway recibe las solicitudes del cliente y las distribuye entre dos servicios. **Taller.Api** administra clientes, vehículos y usuarios. Emite JWT al iniciar sesión y conserva su estructura en capas y su persistencia en SQL Server. **Catalogo.Api** consulta un catálogo de servicios del taller en memoria por lista o identificador. Cada API cuenta con sus propios modelos.

## Tabla de rutas

| Upstream en el gateway | Método | Downstream local | Token |
|---|---|---|---|
| /gateway/auth/{everything} | POST | localhost:5143/api/auth/{everything} | No |
| /gateway/auth/perfil | GET | localhost:5143/api/auth/perfil | Sí |
| /gateway/clientes/{everything} | GET | localhost:5143/api/clientes/{everything} | No |
| /gateway/clientes/{everything} | POST, PUT, DELETE | localhost:5143/api/clientes/{everything} | Sí |
| /gateway/vehiculos/{everything} | GET | localhost:5143/api/vehiculos/{everything} | No |
| /gateway/vehiculos/{everything} | POST, PUT, DELETE | localhost:5143/api/vehiculos/{everything} | Sí |
| /gateway/servicios/{everything} | GET | localhost:5144/api/servicios/{everything} | Sí |

Gateway local en http://localhost:5145. El comodín permite tanto la lista como las rutas con ID. El gateway devuelve la cabecera Location de una creación con una ruta relativa bajo /gateway, para que el cliente pueda seguirla por el mismo origen. Los GET de clientes y vehículos son públicos por requisito de la guía. Usa solo registros ficticios en el despliegue.

## Ejecución y pruebas locales

Requisitos en Windows: SDK .NET 10 o superior, runtime ASP.NET Core 10, SQL Server LocalDB, sqlcmd y Python 3.

```powershell
dotnet build Taller.slnx
python Verificar-Local.py
```

La verificación crea o reutiliza la base de demostración TallerLab15DB y levanta los tres proyectos con una clave compartida que se genera en memoria. Ejecuta las solicitudes, guarda códigos y URL en evidencias y cierra los procesos. Deja las cuentas de recepción de prueba en la base. Para la prueba de caída detiene únicamente el catálogo, pide su lista con token y vuelve a consultar la lista pública de vehículos. También llama directamente al catálogo sin token para comprobar el 401 del servicio.

La ejecución registrada completó 21 solicitudes con el resultado esperado y cubrió los códigos 200, 201, 204, 401, 403, 404 y 502. Los JSON y los registros de arranque contienen los resultados originales. Las imágenes de evidencias presentan esos resultados y los puertos de los tres procesos.

### Ejecución manual para grabar

Después de inicializar la base con el comando anterior, abre tres terminales desde la carpeta del proyecto. Establece una clave de demostración común en las tres mediante `Jwt__Clave`. Genera un valor aleatorio en PowerShell con `[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))` en PowerShell 7. Establece `Jwt__Minutos=30` para Taller.Api y configura `ConnectionStrings__TallerDB` con la conexión integrada a TallerLab15DB, o inicia Taller.Api con `./Iniciar-Local.ps1 -BaseDatos TallerLab15DB` conservando la misma clave. Ejecuta `dotnet run --project Catalogo.Api` y `dotnet run --project GatewayApi` en las otras terminales.

## Despliegue preparado

La opción propuesta es Render Free para ejecutar tres contenedores ASP.NET Core y Azure SQL Database en su oferta gratuita. Render acepta Docker y entrega HTTPS público. Sus servicios gratuitos pueden dormir tras inactividad y comparten horas gratuitas por espacio de trabajo. Azure SQL permite conservar EF Core con SQL Server y el esquema T-SQL. Selecciona la pausa al alcanzar el límite gratuito. Render Postgres es otra opción, pero requiere cambiar el proveedor de EF Core y adaptar el script SQL.

- [Render Free](https://render.com/docs/free)
- [Render Docker](https://render.com/docs/docker)
- [Oferta gratuita de Azure SQL](https://learn.microsoft.com/azure/azure-sql/database/free-offer)

1. En una base Azure SQL de demostración ejecuta `TallerAzure.sql` conectado directamente a la base. Crea los datos ficticios y el esquema de usuarios. Configura el acceso SQL para los rangos de salida del servicio Taller.Api y usa una conexión TLS con `Encrypt=True;TrustServerCertificate=False`.
2. Crea tres servicios gratuitos con el mismo repositorio y Dockerfile. Usa el valor de `PROJECT` correspondiente en cada servicio: `Taller.Api`, `Catalogo.Api` y `GatewayApi`. La aplicación escucha en el puerto interno 8080.
3. Configura el mismo `Jwt__Clave` en los tres servicios. Usa los valores de emisor y audiencia del repositorio, o establece `Jwt__Emisor` y `Jwt__Audiencia` con valores idénticos en los tres. En Taller.Api define también `Jwt__Minutos` y `ConnectionStrings__TallerDB`.
4. Genera el archivo de rutas publicado con `python Preparar-Nube.py URL_GATEWAY URL_TALLER URL_CATALOGO`. Usa los tres orígenes HTTPS asignados por Render. Sube el archivo generado `GatewayApi/ocelot.Cloud.json` y configura `Ocelot__Archivo=ocelot.Cloud.json` en el gateway. El archivo guarda hosts públicos, puerto 443, esquema HTTPS y la URL base del gateway. Usa ese archivo para la captura de configuración.
5. Registra la cuenta de jefe de prueba por la ruta pública y asigna el rol `JefeTaller` mediante una actualización autorizada de la fila en SQL. TallerAzure.sql solo crea el esquema de usuarios para que el despliegue use credenciales nuevas.
6. Define `LAB15_JEFE_CORREO` y `LAB15_JEFE_CLAVE` en tu terminal. Ejecuta `python Probar-Gateway.py URL_GATEWAY URL_CATALOGO evidencias/nube.json`. Con el catálogo temporalmente fuera de servicio, repite con `evidencias/nube-caida.json --solo-caida`. Registra el estado real si el proveedor devuelve una página de suspensión en lugar de permitir a Ocelot detectar la falla de conexión. Reactiva el catálogo al terminar.

Luego completa la tabla de rutas con los dos orígenes downstream publicados, conserva las capturas de las pruebas y pega en la guía los enlaces del repositorio, los servicios y el video.

## Guion de sustentación de diez minutos

| Tiempo | Demostración |
|---|---|
| 0:00–1:30 | Presenta el taller y muestra los tres servicios. Compara las direcciones internas con la única URL que usa el cliente. |
| 1:30–3:30 | Abre el archivo de Ocelot publicado. Recorre /gateway/servicios/1 hasta /api/servicios/1 y explica el prefijo, el comodín y HTTPS. |
| 3:30–5:30 | Inicia sesión. Explica cómo Taller.Api emite el JWT y cómo el gateway y los servicios validan firma, emisor, audiencia y vencimiento. Explica por qué el servicio valida también una llamada directa y el costo de mantener las configuraciones sincronizadas. |
| 5:30–8:30 | Muestra 200, 201, 204 y 404. Obtén un 401 sin token y un 403 intentando eliminar con una cuenta de recepción. Explica que Taller.Api conserva la verificación del rol de jefe. |
| 8:30–10:30 | Detén el catálogo y muestra su respuesta a través del gateway. Consulta los vehículos y explica cómo el taller permanece disponible. Cierra con la complejidad adicional de coordinar rutas, despliegues y secretos. |

Para la entrega de clase, graba la demostración con cámara y pantalla, completa el enlace en la guía y revisa que el docente pueda reproducirlo. Evita mostrar los valores de las variables de entorno durante la grabación.
