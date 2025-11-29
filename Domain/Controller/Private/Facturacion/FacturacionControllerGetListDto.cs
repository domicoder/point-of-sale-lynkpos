using Domain.API;

namespace Domain.Controller.Private.Facturacion
{
    public class FacturacionControllerGetListDto : PaginationQueryParams
    {
        public short? TipoId { get; set; }
        public short? EstadoId { get; set; }
        public Guid? CajaId { get; set; }
        public Guid? UsuarioId { get; set; }
        public DateTime? FechaEmisionInicio { get; set; }
        public DateTime? FechaEmisionFinal { get; set; }
    }
}
