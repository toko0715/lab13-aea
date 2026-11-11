using Taller.Entidades;

namespace Taller.Datos;

public interface IClienteRepositorio
{
    Task<List<Cliente>> ListarAsync(string? nombre, CancellationToken cancellationToken);
    Task<Cliente?> ObtenerAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExisteDniAsync(string dni, int excluirId, CancellationToken cancellationToken);
    Task<bool> TieneVehiculosAsync(int id, CancellationToken cancellationToken);
    Task CrearAsync(Cliente cliente, CancellationToken cancellationToken);
    Task ActualizarAsync(Cliente cliente, CancellationToken cancellationToken);
    Task EliminarAsync(Cliente cliente, CancellationToken cancellationToken);
}
