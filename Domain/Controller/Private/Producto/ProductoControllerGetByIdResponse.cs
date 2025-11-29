
namespace Domain.Controller.Private.Producto
{
    public class ProductoControllerGetByIdResponse
    {
        public required string ProductoId { get; set; }
        public required string Nombre { get; set; }
        public required decimal PrecioUnitario { get; set; }
        public required decimal ImpuestoPorcentaje { get; set; }
        public required int Stock { get; set; }
    }
}
