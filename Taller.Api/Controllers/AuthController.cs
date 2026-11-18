using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taller.Entidades.DTOs;
using Taller.Negocio;

namespace Taller.Api.Controllers;

[ApiController, Route("api/auth"), Authorize]
public sealed class AuthController(IAuthServicio servicio) : ControllerBase
{
    [HttpPost("registro"), AllowAnonymous]
    public async Task<IActionResult> Registro(RegistroDto dto, CancellationToken ct)
    {
        try { return StatusCode(201, await servicio.RegistrarAsync(dto, ct)); }
        catch (ReglaNegocioException error)
        { return Conflict(new ProblemDetails { Status = 409, Title = "Regla de negocio", Detail = error.Message }); }
    }

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken ct)
    {
        var token = await servicio.LoginAsync(dto, ct);
        return token is null ? Unauthorized(new { mensaje = "Credenciales inválidas." }) : Ok(token);
    }

    [HttpGet("perfil")]
    public ActionResult<PerfilDto> Perfil() =>
        Ok(new PerfilDto(User.FindFirst("email")!.Value, User.FindFirst("rol")!.Value));
}
