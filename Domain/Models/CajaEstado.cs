
namespace Domain.Models
{
    public class CajaEstadoModel : BaseEntity<short>
    {
        public required string Nombre { get; set; }

        public ICollection<Caja> Cajas { get; set; } = [];
    }

    public enum CajaEstado : short
    {
        Cerrado = 1,
        Abierto = 2
    }

    public static class CajaEstadoExtensions
    {
        public static short GetValue(this CajaEstado estado)
        {
            return (short)estado;
        }

        public static string GetName(this CajaEstado estado)
        {
            return estado.ToString().ToUpperInvariant();
        }

        public static CajaEstado? FromValue(short value)
        {
            return Enum.IsDefined(typeof(CajaEstado), value)
                ? (CajaEstado)value
                : null;
        }

        public static List<EnumModel<short>> GetList()
        {
            return [.. Enum.GetValues(typeof(CajaEstado))
                .Cast<CajaEstado>()
                .Select(e => new EnumModel<short>
                {
                    Id = (short)e,
                    Nombre = e.GetName()
                })];
        }
    }
}
