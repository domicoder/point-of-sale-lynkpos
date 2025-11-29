
namespace Domain.Models
{
    public class Factura: BaseEntity<Guid>
    {
        public required short TipoId { get; set; }
        public required short EstadoId { get; set; }
        public required Guid UsuarioId { get; set; }
        public required Guid CajaId { get; set; }
        public required DateTime FechaEmision { get; set; }
        public required decimal Total { get; set; }
        public required decimal Subtotal { get; set; }
        public required decimal Impuestos { get; set; }

        public TipoFacturaModel TipoFactura { get; set; } = default!;
        public EstadoFacturaModel EstadoFactura { get; set; } = default!;
        public Usuario Usuario { get; set; } = default!;
        public Caja Caja { get; set; } = default!;
        public ICollection<FacturaDetalle> FacturaDetalle { get; set; } = [];
    }
}
