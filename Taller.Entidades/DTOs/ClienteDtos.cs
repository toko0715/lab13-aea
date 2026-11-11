using System.ComponentModel.DataAnnotations;

namespace Taller.Entidades.DTOs;

public sealed class ClienteDto
{
    public int ClienteId { get; set; }
    public string DNI { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telefono { get; set; } = "";
}

public abstract class ClienteEntradaDto
{
    [Required, StringLength(8, MinimumLength = 8), RegularExpression(@"^[0-9]{8}$")]
    public string DNI { get; set; } = "";

    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(120), EmailAddress]
    public string Email { get; set; } = "";

    [Required, StringLength(20, MinimumLength = 7), Phone]
    public string Telefono { get; set; } = "";
}

public sealed class ClienteCrearDto : ClienteEntradaDto;
public sealed class ClienteActualizarDto : ClienteEntradaDto;
