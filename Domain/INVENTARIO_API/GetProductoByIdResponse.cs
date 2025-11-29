
namespace Domain.INVENTARIO_API
{
    public class GetProductoByIdResponse
    {
        public required string ProductoId { get; set; }
        public required string Nombre { get; set; }
        public required string PrecioUnitario { get; set; }
        public required decimal ImpuestoPorcentaje { get; set; }
        public required int Stock { get; set; }
    }
}
