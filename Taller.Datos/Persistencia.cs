using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Taller.Datos;

public sealed class ConflictoPersistenciaException(string mensaje, Exception causa) : Exception(mensaje, causa);

internal static class Persistencia
{
    internal static async Task GuardarAsync(TallerContext contexto, CancellationToken cancellationToken)
    {
        try { await contexto.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException error)
        {
            throw new ConflictoPersistenciaException("El registro cambió o fue eliminado. Consulta nuevamente.", error);
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 or 547 })
        {
            throw new ConflictoPersistenciaException("La operación entra en conflicto con un registro existente o relacionado.", error);
        }
    }
}
