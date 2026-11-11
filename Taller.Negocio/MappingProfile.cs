using AutoMapper;
using Taller.Entidades;
using Taller.Entidades.DTOs;

namespace Taller.Negocio;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Cliente, ClienteDto>();
        CreateMap<ClienteCrearDto, Cliente>()
            .ForMember(d => d.ClienteId, o => o.Ignore())
            .ForMember(d => d.Vehiculos, o => o.Ignore());
        CreateMap<ClienteActualizarDto, Cliente>()
            .ForMember(d => d.ClienteId, o => o.Ignore())
            .ForMember(d => d.Vehiculos, o => o.Ignore());

        CreateMap<Vehiculo, VehiculoDto>()
            .ForMember(d => d.ClienteNombre, o => o.MapFrom(s => s.Cliente.Nombre));
        CreateMap<VehiculoCrearDto, Vehiculo>()
            .ForMember(d => d.VehiculoId, o => o.Ignore())
            .ForMember(d => d.Cliente, o => o.Ignore());
        CreateMap<VehiculoActualizarDto, Vehiculo>()
            .ForMember(d => d.VehiculoId, o => o.Ignore())
            .ForMember(d => d.Cliente, o => o.Ignore());
    }
}
