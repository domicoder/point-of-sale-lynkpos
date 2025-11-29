
using Domain.Models;

namespace Data.Repositories
{
    public class CajaBitacoraRepository(AppDbContext context) : GenericRepository<Guid, CajaBitacora>(context) { }
}
