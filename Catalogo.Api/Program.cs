using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var jwt = builder.Configuration.GetSection("Jwt");
var clave = jwt["Clave"];
var emisor = jwt["Emisor"];
var audiencia = jwt["Audiencia"];
if (clave is null || Encoding.UTF8.GetByteCount(clave) < 32 ||
    string.IsNullOrWhiteSpace(emisor) || string.IsNullOrWhiteSpace(audiencia))
{
    throw new InvalidOperationException("Configura Jwt__Clave, Jwt__Emisor y Jwt__Audiencia.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = emisor,
        ValidateAudience = true,
        ValidAudience = audiencia,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ValidateIssuerSigningKey = true,
        RequireSignedTokens = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        RoleClaimType = "rol",
        NameClaimType = "email",
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

ServicioTaller[] servicios =
[
    new(1, "Cambio de aceite", "Cambio de aceite del motor y filtro.", 45),
    new(2, "Alineación", "Ajuste de la geometría de las ruedas.", 60),
    new(3, "Revisión de frenos", "Inspección de discos, pastillas y líquido.", 40)
];

var catalogo = app.MapGroup("/api/servicios").RequireAuthorization();
catalogo.MapGet("", () => Results.Ok(servicios));
catalogo.MapGet("/{id:int}", (int id) =>
    servicios.FirstOrDefault(servicio => servicio.Id == id) is { } servicio
        ? Results.Ok(servicio)
        : Results.NotFound());

app.Run();

record ServicioTaller(int Id, string Nombre, string Descripcion, int DuracionMinutos);
