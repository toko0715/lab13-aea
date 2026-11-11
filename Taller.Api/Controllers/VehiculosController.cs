using Microsoft.AspNetCore.Mvc;
using Taller.Entidades.DTOs;
using Taller.Negocio;

namespace Taller.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VehiculosController(IVehiculoServicio servicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<VehiculoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VehiculoDto>>> Listar(
        [FromQuery] string? placa, [FromQuery] string? marca, [FromQuery] int? clienteId, CancellationToken cancellationToken) =>
        Ok(await servicio.ListarAsync(placa, marca, clienteId, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType<VehiculoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehiculoDto>> Obtener(int id, CancellationToken cancellationToken)
    {
        var vehiculo = await servicio.ObtenerAsync(id, cancellationToken);
        return vehiculo is null ? NotFound() : Ok(vehiculo);
    }

    [HttpPost]
    [ProducesResponseType<VehiculoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehiculoDto>> Crear(VehiculoCrearDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var vehiculo = await servicio.CrearAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(Obtener), new { id = vehiculo.VehiculoId }, vehiculo);
        }
        catch (ReglaNegocioException error)
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Regla de negocio", Detail = error.Message });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, VehiculoActualizarDto dto, CancellationToken cancellationToken)
    {
        try { return await servicio.ActualizarAsync(id, dto, cancellationToken) ? NoContent() : NotFound(); }
        catch (ReglaNegocioException error)
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Regla de negocio", Detail = error.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        try { return await servicio.EliminarAsync(id, cancellationToken) ? NoContent() : NotFound(); }
        catch (ReglaNegocioException error)
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Regla de negocio", Detail = error.Message });
        }
    }
}
