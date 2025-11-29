using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class CajaRepository(AppDbContext context) : GenericRepository<Guid, Caja>(context) {

        public async Task Open(Guid usuarioId, Guid cajaId)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_abrircaja @usuario_id = {0}, @caja_id = {1}",
                usuarioId, cajaId
            );
        }

        public async Task Close(Guid usuarioId, Guid cajaId)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_cerrarcaja @usuario_id = {0}, @caja_id = {1}",
                usuarioId, cajaId
            );
        }

    }
}
