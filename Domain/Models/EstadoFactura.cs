
namespace Domain.Models
{
    public class EstadoFacturaModel: BaseEntity<short>
    {
        public required string Nombre { get; set; }
        public ICollection<Factura> Facturas { get; set; } = [];
    }

    public enum EstadoFactura : short
    {
        Emitida = 1,
        Cancelada = 2
    }

    public static class EstadoFacturaExtensions
    {
        public static short GetValue(this EstadoFactura estadoFactura)
        {
            return (short)estadoFactura;
        }
    }
}
