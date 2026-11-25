using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: false);
var rutasNube = builder.Configuration["Ocelot:Archivo"];
if (!string.IsNullOrWhiteSpace(rutasNube))
{
    builder.Configuration.AddJsonFile(rutasNube, optional: false, reloadOnChange: false);
}
builder.Configuration.AddEnvironmentVariables();
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
builder.Services.AddOcelot(builder.Configuration);
var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var location = context.Response.Headers.Location.ToString();
        var path = location.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)
            ? location
            : Uri.TryCreate(location, UriKind.Absolute, out var uri) ? uri.PathAndQuery : "";
        if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Headers.Location = "/gateway/" + path[5..];
        }
        return Task.CompletedTask;
    });
    await next(context);
});

await app.UseOcelot();
app.Run();
