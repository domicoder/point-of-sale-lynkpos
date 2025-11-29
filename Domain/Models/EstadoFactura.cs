
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

        public static string GetName(this EstadoFactura estadoFactura)
        {
            return estadoFactura.ToString().ToUpperInvariant();
        }

        public static EstadoFactura? FromValue(short value)
        {
            return Enum.IsDefined(typeof(EstadoFactura), value)
                ? (EstadoFactura)value
                : null;
        }

        public static List<EnumModel<short>> GetList()
        {
            return [.. Enum.GetValues(typeof(EstadoFactura))
                .Cast<EstadoFactura>()
                .Select(e => new EnumModel<short>
                {
                    Id = (short)e,
                    Nombre = e.GetName()
                })];
        }
    }
}
