
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

        public required TipoFacturaModel TipoFactura { get; set; }
        public required EstadoFacturaModel EstadoFactura { get; set; }
        public required Usuario Usuario { get; set; }
        public required Caja Caja { get; set; }
        public required FacturaDetalle FacturaDetalle { get; set; }
    }
}
