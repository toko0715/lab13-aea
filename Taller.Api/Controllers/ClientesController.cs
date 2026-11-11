using Microsoft.AspNetCore.Mvc;
using Taller.Entidades.DTOs;
using Taller.Negocio;

namespace Taller.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ClientesController(IClienteServicio servicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<ClienteDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ClienteDto>>> Listar([FromQuery] string? nombre, CancellationToken cancellationToken) =>
        Ok(await servicio.ListarAsync(nombre, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> Obtener(int id, CancellationToken cancellationToken)
    {
        var cliente = await servicio.ObtenerAsync(id, cancellationToken);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteDto>> Crear(ClienteCrearDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await servicio.CrearAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(Obtener), new { id = cliente.ClienteId }, cliente);
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
    public async Task<IActionResult> Actualizar(int id, ClienteActualizarDto dto, CancellationToken cancellationToken)
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

    [HttpGet("{id:int}/vehiculos")]
    [ProducesResponseType<List<VehiculoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<VehiculoDto>>> Vehiculos(int id, CancellationToken cancellationToken)
    {
        var vehiculos = await servicio.VehiculosAsync(id, cancellationToken);
        return vehiculos is null ? NotFound() : Ok(vehiculos);
    }
}
