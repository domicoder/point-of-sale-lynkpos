
namespace Domain.Models
{
    public class CajaBitacora : BaseEntity<Guid>
    {
        public required Guid UsuarioId { get; set; }
        public required Guid CajaId { get; set; }
        public required DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }

        public Usuario Usuario { get; set; } = default!;
        public Caja Caja { get; set; } = default!;
    }
}
