using Taller.Entidades.DTOs;

namespace Taller.Negocio;

public sealed class ReglaNegocioException(string mensaje, Exception? causa = null) : Exception(mensaje, causa);

public interface IClienteServicio
{
    Task<List<ClienteDto>> ListarAsync(string? nombre, CancellationToken cancellationToken);
    Task<ClienteDto?> ObtenerAsync(int id, CancellationToken cancellationToken);
    Task<ClienteDto> CrearAsync(ClienteCrearDto dto, CancellationToken cancellationToken);
    Task<bool> ActualizarAsync(int id, ClienteActualizarDto dto, CancellationToken cancellationToken);
    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken);
    Task<List<VehiculoDto>?> VehiculosAsync(int id, CancellationToken cancellationToken);
}

public interface IVehiculoServicio
{
    Task<List<VehiculoDto>> ListarAsync(string? placa, string? marca, int? clienteId, CancellationToken cancellationToken);
    Task<VehiculoDto?> ObtenerAsync(int id, CancellationToken cancellationToken);
    Task<VehiculoDto> CrearAsync(VehiculoCrearDto dto, CancellationToken cancellationToken);
    Task<bool> ActualizarAsync(int id, VehiculoActualizarDto dto, CancellationToken cancellationToken);
    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken);
}
