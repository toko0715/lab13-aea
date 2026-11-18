using Microsoft.EntityFrameworkCore;
using Taller.Entidades;

namespace Taller.Datos;

public interface IUsuarioRepositorio
{
    Task<Usuario?> BuscarPorCorreoAsync(string correo, CancellationToken ct);
    Task AgregarAsync(Usuario usuario, CancellationToken ct);
    Task GuardarAsync(CancellationToken ct);
}

public sealed class UsuarioRepositorio(TallerContext context) : IUsuarioRepositorio
{
    public Task<Usuario?> BuscarPorCorreoAsync(string correo, CancellationToken ct) =>
        context.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.Correo == correo, ct);
    public async Task AgregarAsync(Usuario usuario, CancellationToken ct) =>
        await context.Usuarios.AddAsync(usuario, ct);
    public Task GuardarAsync(CancellationToken ct) => Persistencia.GuardarAsync(context, ct);
}
