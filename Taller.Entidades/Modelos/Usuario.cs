namespace Taller.Entidades;

public sealed class Usuario
{
    public int UsuarioId { get; set; }
    public string Correo { get; set; } = "";
    public string ClaveHash { get; set; } = "";
    public string Rol { get; set; } = Roles.Recepcionista;
}

public static class Roles
{
    public const string Recepcionista = "Recepcionista";
    public const string JefeTaller = "JefeTaller";
}
