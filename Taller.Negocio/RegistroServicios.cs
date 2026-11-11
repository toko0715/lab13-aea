using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Taller.Datos;

namespace Taller.Negocio;

public static class RegistroServicios
{
    public static IServiceCollection AddPersistenciaTaller(this IServiceCollection servicios, string cadena)
    {
        servicios.AddDbContext<TallerContext>(opciones => opciones.UseSqlServer(cadena));
        servicios.AddScoped<IClienteRepositorio, ClienteRepositorio>();
        servicios.AddScoped<IVehiculoRepositorio, VehiculoRepositorio>();
        return servicios;
    }
}
