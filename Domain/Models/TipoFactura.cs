
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

        public static string GetName(this TipoFactura tipoFactura)
        {
            return tipoFactura.ToString().ToUpperInvariant();
        }

        public static TipoFactura? FromValue(short value)
        {
            return Enum.IsDefined(typeof(TipoFactura), value)
                ? (TipoFactura)value
                : null;
        }

        public static List<EnumModel<short>> GetList()
        {
            return [.. Enum.GetValues(typeof(TipoFactura))
                .Cast<TipoFactura>()
                .Select(e => new EnumModel<short>
                {
                    Id = (short)e,
                    Nombre = e.GetName()
                })];
        }
    }
}
