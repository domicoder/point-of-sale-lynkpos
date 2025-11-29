using Domain.Controller.Private.Facturacion;
using Domain.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class FacturaRepository(AppDbContext context) : GenericRepository<Guid, Factura>(context) {

        public async Task<List<FacturacionControllerGetListResponse.Factura>> GetPagination(
            int? page, byte? pageSize,
            short? tipoId, short? estadoId,
            Guid? cajaId, Guid? usuarioId,
            DateTime? fechaEmisionInicio, DateTime? fechaEmisionFinal
        )
        {
            var sql = "EXEC sp_factura_paginacion @page, @page_size, @tipo_id, @estado_id, @caja_id, @usuario_id, @fecha_emision_inicio, @fecha_emision_final;";

            var data = await _context.Database.SqlQueryRaw<FacturacionControllerGetListResponse.Factura>(
                sql,
                new SqlParameter("@page", page ?? 1),
                new SqlParameter("@page_size", pageSize ?? 20),
                new SqlParameter("@tipo_id", (object?)tipoId ?? DBNull.Value),
                new SqlParameter("@estado_id", (object?)estadoId ?? DBNull.Value),
                new SqlParameter("@caja_id", (object?)cajaId ?? DBNull.Value),
                new SqlParameter("@usuario_id", (object?)usuarioId ?? DBNull.Value),
                new SqlParameter("@fecha_emision_inicio", (object?)fechaEmisionInicio ?? DBNull.Value),
                new SqlParameter("@fecha_emision_final", (object?)fechaEmisionFinal ?? DBNull.Value)
            ).ToListAsync();

            return data;
        }

    }
}
