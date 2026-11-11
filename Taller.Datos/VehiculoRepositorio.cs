using Microsoft.EntityFrameworkCore;
using Taller.Entidades;

namespace Taller.Datos;

public sealed class VehiculoRepositorio(TallerContext context) : IVehiculoRepositorio
{
    public async Task<List<Vehiculo>> ListarAsync(string? placa, string? marca, int? clienteId, CancellationToken cancellationToken)
    {
        var consulta = context.Vehiculos.AsNoTracking().Include(v => v.Cliente).AsQueryable();
        if (!string.IsNullOrWhiteSpace(placa))
            consulta = consulta.Where(v => v.Placa.Contains(placa.Trim()));
        if (!string.IsNullOrWhiteSpace(marca))
            consulta = consulta.Where(v => v.Marca.Contains(marca.Trim()));
        if (clienteId.HasValue)
            consulta = consulta.Where(v => v.ClienteId == clienteId.Value);
        return await consulta.OrderBy(v => v.VehiculoId).ToListAsync(cancellationToken);
    }

    public async Task<Vehiculo?> ObtenerAsync(int id, CancellationToken cancellationToken) =>
        await context.Vehiculos.Include(v => v.Cliente).SingleOrDefaultAsync(v => v.VehiculoId == id, cancellationToken);

    public async Task<bool> ExistePlacaAsync(string placa, int excluirId, CancellationToken cancellationToken) =>
        await context.Vehiculos.AnyAsync(v => v.Placa == placa && v.VehiculoId != excluirId, cancellationToken);

    public async Task CrearAsync(Vehiculo vehiculo, CancellationToken cancellationToken)
    {
        await context.Vehiculos.AddAsync(vehiculo, cancellationToken);
        await Persistencia.GuardarAsync(context, cancellationToken);
    }

    public async Task ActualizarAsync(Vehiculo vehiculo, CancellationToken cancellationToken)
    {
        context.Vehiculos.Update(vehiculo);
        await Persistencia.GuardarAsync(context, cancellationToken);
    }

    public async Task EliminarAsync(Vehiculo vehiculo, CancellationToken cancellationToken)
    {
        context.Vehiculos.Remove(vehiculo);
        await Persistencia.GuardarAsync(context, cancellationToken);
    }
}
