using AutoMapper;
using Taller.Datos;
using Taller.Entidades;
using Taller.Entidades.DTOs;

namespace Taller.Negocio;

public sealed class ClienteServicio(
    IClienteRepositorio clientes, IVehiculoRepositorio vehiculos, IMapper mapper) : IClienteServicio
{
    public async Task<List<ClienteDto>> ListarAsync(string? nombre, CancellationToken cancellationToken) =>
        mapper.Map<List<ClienteDto>>(await clientes.ListarAsync(nombre, cancellationToken));

    public async Task<ClienteDto?> ObtenerAsync(int id, CancellationToken cancellationToken)
    {
        var cliente = await clientes.ObtenerAsync(id, cancellationToken);
        return cliente is null ? null : mapper.Map<ClienteDto>(cliente);
    }

    public async Task<ClienteDto> CrearAsync(ClienteCrearDto dto, CancellationToken cancellationToken)
    {
        if (await clientes.ExisteDniAsync(dto.DNI, 0, cancellationToken))
            throw new ReglaNegocioException("El DNI ya está registrado.");
        var cliente = mapper.Map<Cliente>(dto);
        Normalizar(cliente);
        try { await clientes.CrearAsync(cliente, cancellationToken); }
        catch (ConflictoPersistenciaException error) { throw new ReglaNegocioException(error.Message, error); }
        return mapper.Map<ClienteDto>(cliente);
    }

    public async Task<bool> ActualizarAsync(int id, ClienteActualizarDto dto, CancellationToken cancellationToken)
    {
        var cliente = await clientes.ObtenerAsync(id, cancellationToken);
        if (cliente is null) return false;
        if (await clientes.ExisteDniAsync(dto.DNI, id, cancellationToken))
            throw new ReglaNegocioException("El DNI ya está registrado.");
        mapper.Map(dto, cliente);
        Normalizar(cliente);
        try { await clientes.ActualizarAsync(cliente, cancellationToken); }
        catch (ConflictoPersistenciaException error) { throw new ReglaNegocioException(error.Message, error); }
        return true;
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken)
    {
        var cliente = await clientes.ObtenerAsync(id, cancellationToken);
        if (cliente is null) return false;
        if (await clientes.TieneVehiculosAsync(id, cancellationToken))
            throw new ReglaNegocioException("No se puede eliminar un cliente con vehículos registrados.");
        try { await clientes.EliminarAsync(cliente, cancellationToken); }
        catch (ConflictoPersistenciaException error) { throw new ReglaNegocioException(error.Message, error); }
        return true;
    }

    public async Task<List<VehiculoDto>?> VehiculosAsync(int id, CancellationToken cancellationToken)
    {
        if (await clientes.ObtenerAsync(id, cancellationToken) is null) return null;
        return mapper.Map<List<VehiculoDto>>(await vehiculos.ListarAsync(null, null, id, cancellationToken));
    }

    private static void Normalizar(Cliente cliente)
    {
        cliente.Nombre = cliente.Nombre.Trim();
        cliente.Email = cliente.Email.Trim();
        cliente.Telefono = cliente.Telefono.Trim();
    }
}
