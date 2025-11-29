
namespace Domain.Controller.Private.Facturacion
{
    public class FacturacionControllerGetByIdResponse
    {
        public required Guid Id { get; set; }
        public required short TipoID { get; set; }
        public required short EstadoId { get; set; }
        public required Guid CajaId { get; set; }
        public required Guid UsuarioId { get; set; }
        public required DateTime FechaEmision { get; set; }
        public required decimal Total { get; set; }
        public required decimal Subtotal { get; set; }
        public required decimal Impuestos { get; set; }
        public required FacturaTipo Tipo { get; set; }
        public required FacturaEstado Estado { get; set; }
        public required FacturaCaja Caja { get; set; }
        public required ICollection<FacturaDetalle> FacturaDetalles { get; set; }

        public class FacturaTipo
        {
            public required short Id { get; set; }
            public required string Nombre { get; set; }
        }

        public class FacturaEstado
        {
            public required short Id { get; set; }
            public required string Nombre { get; set; }
        }

        public class FacturaCaja
        {
            public required Guid Id { get; set; }
            public required string Codigo { get; set; }
            public required string Nombre { get; set; }
        }

        public class FacturaDetalle
        {
            public required string ProductoId { get; set; }
            public required string NombreProducto { get; set; }
            public required int Cantida { get; set; }
            public required decimal ImpuestoPorcentaje { get; set; }
            public required decimal PrecioUnitario { get; set; }
            public required decimal Total { get; set; }
            public required decimal Subtotal { get; set; }
            public required decimal Impuestos { get; set; }
        }
    }
}
