
namespace Domain.Models
{
    public class Caja : BaseEntity<Guid>
    {
        public required short EstadoId { get; set; }
        public required string Codigo { get; set; }
        public required string Nombre { get; set; }
        public bool Activo { get; set; } = false;
        public bool Eliminado { get; set; } = false;

        public CajaEstadoModel CajaEstado { get; set; } = default!;
        public ICollection<CajaBitacora> CajaBitacoras { get; set; } = [];
        public ICollection<Factura> Facturas { get; set; } = [];
    }
}
