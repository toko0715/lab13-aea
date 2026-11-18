using AutoMapper;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Taller.Negocio;

var builder = WebApplication.CreateBuilder(args);
var cadena = builder.Configuration.GetConnectionString("TallerDB")
    ?? throw new InvalidOperationException("Configura ConnectionStrings__TallerDB o inicia con Iniciar-Local.ps1.");
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtConfiguracion>()
    ?? throw new InvalidOperationException("Configura Jwt.");
if (jwt.Clave.Length < 32 || Encoding.UTF8.GetByteCount(jwt.Clave) < 32 || jwt.Minutos < 1 || jwt.Minutos > 120 ||
    string.IsNullOrWhiteSpace(jwt.Emisor) || string.IsNullOrWhiteSpace(jwt.Audiencia))
    throw new InvalidOperationException("Jwt requiere clave de al menos 32 caracteres, emisor, audiencia y minutos entre 1 y 120.");
builder.Services.AddSingleton(jwt);
builder.Services.AddPersistenciaTaller(cadena);
builder.Services.AddScoped<IAuthServicio, AuthServicio>();
builder.Services.AddScoped<IClienteServicio, ClienteServicio>();
builder.Services.AddScoped<IVehiculoServicio, VehiculoServicio>();
builder.Services.AddAutoMapper(_ => { }, typeof(MappingProfile));
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwt.Emisor,
        ValidateAudience = true, ValidAudience = jwt.Audiencia,
        ValidateLifetime = true, RequireExpirationTime = true,
        ValidateIssuerSigningKey = true, RequireSignedTokens = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        RoleClaimType = "rol", NameClaimType = "email", ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", Description = "Pega solamente el token obtenido en api/auth/login." });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] });
});
builder.Services.AddProblemDetails();
var app = builder.Build();
app.Services.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
