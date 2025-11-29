using Domain.Models;

namespace Data.Repositories
{
    public class FacturaDetalleRepository(AppDbContext context) : GenericRepository<Guid, FacturaDetalle>(context) { }
}
