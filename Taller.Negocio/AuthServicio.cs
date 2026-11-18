using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Taller.Datos;
using Taller.Entidades;
using Taller.Entidades.DTOs;

namespace Taller.Negocio;

public sealed class JwtConfiguracion
{
    public string Clave { get; set; } = "";
    public string Emisor { get; set; } = "";
    public string Audiencia { get; set; } = "";
    public int Minutos { get; set; }
}

public interface IAuthServicio
{
    Task<PerfilDto> RegistrarAsync(RegistroDto dto, CancellationToken ct);
    Task<TokenDto?> LoginAsync(LoginDto dto, CancellationToken ct);
}

public sealed class AuthServicio(IUsuarioRepositorio usuarios, JwtConfiguracion jwt) : IAuthServicio
{
    // Verificación BCrypt incluso si el correo no existe para reducir diferencias de tiempo.
    private static readonly string HashFicticio = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), 12);

    public async Task<PerfilDto> RegistrarAsync(RegistroDto dto, CancellationToken ct)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();
        if (Encoding.UTF8.GetByteCount(dto.Clave) > 72)
            throw new ReglaNegocioException("La clave supera los 72 bytes admitidos por BCrypt.");
        if (await usuarios.BuscarPorCorreoAsync(correo, ct) is not null)
            throw new ReglaNegocioException("El correo ya está registrado.");
        var usuario = new Usuario { Correo = correo,
            ClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.Clave, 12), Rol = Roles.Recepcionista };
        await usuarios.AgregarAsync(usuario, ct);
        try { await usuarios.GuardarAsync(ct); }
        catch (ConflictoPersistenciaException error) { throw new ReglaNegocioException("El correo ya está registrado.", error); }
        return new PerfilDto(usuario.Correo, usuario.Rol);
    }

    public async Task<TokenDto?> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        var usuario = await usuarios.BuscarPorCorreoAsync(dto.Correo.Trim().ToLowerInvariant(), ct);
        var valida = BCrypt.Net.BCrypt.Verify(dto.Clave, usuario?.ClaveHash ?? HashFicticio);
        if (usuario is null || !valida) return null;
        var ahora = DateTime.UtcNow;
        var expiracion = ahora.AddMinutes(jwt.Minutos);
        var token = new JwtSecurityToken(jwt.Emisor, jwt.Audiencia,
            [new Claim(JwtRegisteredClaimNames.Sub, usuario.UsuarioId.ToString()),
             new Claim(JwtRegisteredClaimNames.Email, usuario.Correo), new Claim("rol", usuario.Rol),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())],
            ahora, expiracion, new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)), SecurityAlgorithms.HmacSha256));
        return new TokenDto(new JwtSecurityTokenHandler().WriteToken(token), expiracion);
    }
}
