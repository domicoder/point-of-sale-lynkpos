
namespace Domain.Controller.Private.Facturacion
{
    public class FacturacionControllerAddInvoiceDto
    {
        public required short TipoId { get; set; }
        public required Guid CajaId { get; set; }
        public required ICollection<Producto> Productos { get; set; }

        public class Producto
        {
            public required string ProductoId { get; set; }
            public required int Cantidad { get; set; }
        }
    }
}
