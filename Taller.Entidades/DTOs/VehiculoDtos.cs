using System.ComponentModel.DataAnnotations;

namespace Taller.Entidades.DTOs;

public sealed class VehiculoDto
{
    public int VehiculoId { get; set; }
    public string Placa { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public int Anio { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = "";
}

public abstract class VehiculoEntradaDto
{
    [Required, StringLength(7, MinimumLength = 7), RegularExpression(@"^[A-Za-z0-9]{3}-[0-9]{3}$")]
    public string Placa { get; set; } = "";

    [Required, StringLength(40, MinimumLength = 2)]
    public string Marca { get; set; } = "";

    [Required, StringLength(60, MinimumLength = 1)]
    public string Modelo { get; set; } = "";

    [Range(1980, 2100)]
    public int Anio { get; set; }

    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }
}

public sealed class VehiculoCrearDto : VehiculoEntradaDto;
public sealed class VehiculoActualizarDto : VehiculoEntradaDto;
