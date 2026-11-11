using AutoMapper;
using Taller.Datos;
using Taller.Entidades;
using Taller.Entidades.DTOs;

namespace Taller.Negocio;

public sealed class VehiculoServicio(
    IVehiculoRepositorio vehiculos, IClienteRepositorio clientes, IMapper mapper) : IVehiculoServicio
{
    public async Task<List<VehiculoDto>> ListarAsync(string? placa, string? marca, int? clienteId, CancellationToken cancellationToken) =>
        mapper.Map<List<VehiculoDto>>(await vehiculos.ListarAsync(placa, marca, clienteId, cancellationToken));

    public async Task<VehiculoDto?> ObtenerAsync(int id, CancellationToken cancellationToken)
    {
        var vehiculo = await vehiculos.ObtenerAsync(id, cancellationToken);
        return vehiculo is null ? null : mapper.Map<VehiculoDto>(vehiculo);
    }

    public async Task<VehiculoDto> CrearAsync(VehiculoCrearDto dto, CancellationToken cancellationToken)
    {
        var propietario = await ValidarAsync(dto, 0, cancellationToken);
        var vehiculo = mapper.Map<Vehiculo>(dto);
        vehiculo.Placa = vehiculo.Placa.ToUpperInvariant();
        vehiculo.Marca = vehiculo.Marca.Trim();
        vehiculo.Modelo = vehiculo.Modelo.Trim();
        vehiculo.Cliente = propietario;
        try { await vehiculos.CrearAsync(vehiculo, cancellationToken); }
        catch (ConflictoPersistenciaException error) { throw new ReglaNegocioException(error.Message, error); }
        return mapper.Map<VehiculoDto>(vehiculo);
    }

    public async Task<bool> ActualizarAsync(int id, VehiculoActualizarDto dto, CancellationToken cancellationToken)
    {
        var vehiculo = await vehiculos.ObtenerAsync(id, cancellationToken);
        if (vehiculo is null) return false;
        var propietario = await ValidarAsync(dto, id, cancellationToken);
        mapper.Map(dto, vehiculo);
        vehiculo.Placa = vehiculo.Placa.ToUpperInvariant();
        vehiculo.Marca = vehiculo.Marca.Trim();
        vehiculo.Modelo = vehiculo.Modelo.Trim();
        vehiculo.Cliente = propietario;
        try { await vehiculos.ActualizarAsync(vehiculo, cancellationToken); }
        catch (ConflictoPersistenciaException error) { throw new ReglaNegocioException(error.Message, error); }
        return true;
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken)
    {
        var vehiculo = await vehiculos.ObtenerAsync(id, cancellationToken);
        if (vehiculo is null) return false;
        try { await vehiculos.EliminarAsync(vehiculo, cancellationToken); }
        catch (ConflictoPersistenciaException error) { throw new ReglaNegocioException(error.Message, error); }
        return true;
    }

    private async Task<Cliente> ValidarAsync(VehiculoEntradaDto dto, int excluirId, CancellationToken cancellationToken)
    {
        if (await vehiculos.ExistePlacaAsync(dto.Placa.ToUpperInvariant(), excluirId, cancellationToken))
            throw new ReglaNegocioException("La placa ya está registrada.");
        return await clientes.ObtenerAsync(dto.ClienteId, cancellationToken)
            ?? throw new ReglaNegocioException("El cliente indicado no existe.");
    }
}
