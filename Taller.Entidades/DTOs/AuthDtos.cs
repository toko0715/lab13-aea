using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Taller.Entidades.DTOs;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class RegistroDto
{
    [Required, EmailAddress, StringLength(120)]
    public string Correo { get; set; } = "";
    [Required, StringLength(64, MinimumLength = 10)]
    public string Clave { get; set; } = "";
}

public sealed class LoginDto
{
    [Required, EmailAddress, StringLength(120)]
    public string Correo { get; set; } = "";
    [Required, StringLength(64)]
    public string Clave { get; set; } = "";
}

public sealed record TokenDto(string Token, DateTime Expiracion);
public sealed record PerfilDto(string Correo, string Rol);
