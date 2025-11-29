
using Domain.API;

namespace Domain.Controller.Private.Facturacion
{
    public class FacturacionControllerGetListResponse: OkResponse
    {
        public ICollection<Factura> Data { get; set; } = [];

        public class Factura
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
            public required string TipoNombre { get; set; }
            public required string EstadoNombre { get; set; }
            public required string CajaCodigo { get; set; }
            public required string CajaNombre { get; set; }
            public required string Usuario { get; set; }
            public required string UsuarioNombre { get; set; }
        }
    }
}
