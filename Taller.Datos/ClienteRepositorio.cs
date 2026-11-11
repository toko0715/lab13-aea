using Microsoft.EntityFrameworkCore;
using Taller.Entidades;

namespace Taller.Datos;

public sealed class ClienteRepositorio(TallerContext context) : IClienteRepositorio
{
    public async Task<List<Cliente>> ListarAsync(string? nombre, CancellationToken cancellationToken)
    {
        var consulta = context.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(nombre))
            consulta = consulta.Where(c => c.Nombre.Contains(nombre.Trim()));
        return await consulta.OrderBy(c => c.ClienteId).ToListAsync(cancellationToken);
    }

    public async Task<Cliente?> ObtenerAsync(int id, CancellationToken cancellationToken) =>
        await context.Clientes.SingleOrDefaultAsync(c => c.ClienteId == id, cancellationToken);

    public async Task<bool> ExisteDniAsync(string dni, int excluirId, CancellationToken cancellationToken) =>
        await context.Clientes.AnyAsync(c => c.Dni == dni && c.ClienteId != excluirId, cancellationToken);

    public async Task<bool> TieneVehiculosAsync(int id, CancellationToken cancellationToken) =>
        await context.Vehiculos.AnyAsync(v => v.ClienteId == id, cancellationToken);

    public async Task CrearAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        await context.Clientes.AddAsync(cliente, cancellationToken);
        await Persistencia.GuardarAsync(context, cancellationToken);
    }

    public async Task ActualizarAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        context.Clientes.Update(cliente);
        await Persistencia.GuardarAsync(context, cancellationToken);
    }

    public async Task EliminarAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        context.Clientes.Remove(cliente);
        await Persistencia.GuardarAsync(context, cancellationToken);
    }
}
