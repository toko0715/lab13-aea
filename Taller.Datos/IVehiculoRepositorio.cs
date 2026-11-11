using Taller.Entidades;

namespace Taller.Datos;

public interface IVehiculoRepositorio
{
    Task<List<Vehiculo>> ListarAsync(string? placa, string? marca, int? clienteId, CancellationToken cancellationToken);
    Task<Vehiculo?> ObtenerAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistePlacaAsync(string placa, int excluirId, CancellationToken cancellationToken);
    Task CrearAsync(Vehiculo vehiculo, CancellationToken cancellationToken);
    Task ActualizarAsync(Vehiculo vehiculo, CancellationToken cancellationToken);
    Task EliminarAsync(Vehiculo vehiculo, CancellationToken cancellationToken);
}
