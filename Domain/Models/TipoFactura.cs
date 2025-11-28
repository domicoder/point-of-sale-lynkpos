
namespace Domain.Models
{
    public class TipoFacturaModel: BaseEntity<short>
    {
        public required string Nombre { get; set; }
        public ICollection<Factura> Facturas { get; set; } = [];
    }

    public enum TipoFactura : short
    {
        Debito = 1,
        Credito = 2
    }

    public static class TipoFacturaExtensions
    {
        public static short GetValue(this TipoFactura tipoFactura)
        {
            return (short)tipoFactura;
        }
    }
}
