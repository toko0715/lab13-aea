using AutoMapper;
using Taller.Negocio;

var builder = WebApplication.CreateBuilder(args);
var cadena = builder.Configuration.GetConnectionString("TallerDB")
    ?? throw new InvalidOperationException("Configura ConnectionStrings__TallerDB o inicia con Iniciar-Local.ps1.");

builder.Services.AddPersistenciaTaller(cadena);
builder.Services.AddScoped<IClienteServicio, ClienteServicio>();
builder.Services.AddScoped<IVehiculoServicio, VehiculoServicio>();
builder.Services.AddAutoMapper(_ => { }, typeof(MappingProfile));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.Services.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
