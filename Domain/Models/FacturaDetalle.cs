
namespace Domain.Models
{
    public class FacturaDetalle: BaseEntity<Guid>
    {
        public required Guid FacturaId { get; set; }
        public required string ProductoId { get; set; }
        public required string NombreProducto { get; set; }
        public required int Cantidad { get; set; }
        public required decimal ImpuestoPorcentaje { get; set; }
        public required decimal PrecioUnitario { get; set; }
        public required decimal Total { get; set; }
        public required decimal Subtotal { get; set; }
        public required decimal Impuestos { get; set; }

        public required Factura Factura { get; set; }
    }
}
