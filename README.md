# Taller mecánico

Para ejecutar los tres servicios, consultar la tabla de rutas y preparar el despliegue, abre [Laboratorio 15](LAB15.md). El contenido de semana 14 se conserva aquí como antecedente.

## Laboratorio 14 JWT en el taller mecánico

Extensión del proyecto S13, en las mismas cuatro capas y con SQL Server LocalDB. .NET 10, EF Core 10, BCrypt.Net-Next 4.0.3, JWT 8.19.2, JwtBearer 10.0.13 y AutoMapper 16.3.0.

## Ejecutar localmente

Requiere SDK .NET 10 o superior, SQL Server LocalDB y sqlcmd. Desde esta carpeta:

```powershell
sqlcmd -S '(localdb)\MSSQLLocalDB' -E -C -b -f 65001 -i TallerDB.sql
sqlcmd -S '(localdb)\MSSQLLocalDB' -E -C -b -f 65001 -i Usuarios.sql
powershell -NoProfile -ExecutionPolicy Bypass -File Iniciar-Local.ps1
```

Swagger: https://localhost:7143/swagger

El script construye la conexión con autenticación integrada y la pasa por ConnectionStrings__TallerDB. Genera una clave aleatoria de 48 bytes si Jwt__Clave no está definida y usa Jwt__Minutos=30 si no se configuró. La clave y la duración viven en variables de entorno. Para mantener la misma clave entre reinicios, configúrala fuera del repositorio con un gestor de secretos. appsettings.json conserva únicamente emisor y audiencia. No subas cadenas de conexión ni claves JWT.

## Cuentas de demostración

Usuarios.sql incluye hashes BCrypt reales obtenidos después de registrar las cuentas por la API. Son cuentas locales de laboratorio:

| Correo | Rol | Clave de prueba |
|---|---|---|
| recepcion.lab14@example.com | Recepcionista | Taller2026!Lab14 |
| jefe.lab14@example.com | JefeTaller | Taller2026!Lab14 |

El servidor asigna Recepcionista al registrarse. Para crear un JefeTaller nuevo, registra la cuenta por POST /api/auth/registro y cambia Rol mediante un UPDATE autorizado en SQL. Vuelve a iniciar sesión para emitir un token con el nuevo rol. No existe una operación pública para elevar permisos.

## Autenticación

- POST /api/auth/registro es público, recibe correo y clave, devuelve 201; un correo duplicado devuelve 409. Campos adicionales como rol devuelven 400.
- POST /api/auth/login es público y devuelve 200 con token y expiracion; correo inexistente y clave incorrecta devuelven el mismo 401.
- GET /api/auth/perfil requiere token y lee email y rol de User.Claims.
- Todos los endpoints de clientes y vehículos requieren autenticación. Solo JefeTaller puede ejecutar DELETE.
- La matriz completa está en Matriz-Permisos.md.

En Swagger pulsa Authorize, pega solo el token y confirma Authorize. Las solicitudes protegidas enviarán Authorization: Bearer. Se validan firma HS256, emisor, audiencia y vencimiento, con ClockSkew cero. Los claims incluyen sub, email, rol, jti, nbf, exp, iss y aud. Un cambio de rol en SQL no revoca tokens ya emitidos; conservan sus permisos hasta expirar.

## Verificación

```powershell
dotnet build Taller.slnx
python Probar-Jwt.py https://localhost:7143 entrega/reprueba-http.json
```

Batería final: 66 solicitudes correctas, todos los endpoints y ambos roles; códigos 200, 201, 204, 400, 401, 403, 404 y 409. El certificado autofirmado se acepta únicamente en el cliente de pruebas local. Las cuentas registradas en las pruebas permanecen para comprobar correo único; los clientes y vehículos creados por una ejecución correcta se eliminan.

El script Probar-Api.py se conserva como antecedente S13. Para S14 usa Probar-Jwt.py.

## Entregables

La carpeta entrega contiene el informe HTML con imágenes incluidas, la sustentación en Markdown, diez capturas PNG y los resultados HTTP. Las capturas del visor muestran las respuestas reales guardadas; las de Swagger muestran la interfaz original. El informe se puede abrir en un navegador e imprimir.

## Arquitectura conservada

Api → Negocio → Datos → Entidades. Se mantiene MappingProfile y las reglas sobre DNI, placas y relaciones entre clientes y vehículos. TallerContext.Usuarios.cs amplía el contexto mediante una clase parcial; Usuario se agregó manualmente. Si regeneras el scaffold con Force, conserva esas extensiones y los ajustes de las entidades.

ReglaNegocioException produce 409. El índice único de Usuarios protege también contra registros concurrentes con el mismo correo.
