using Domain.API;

namespace Domain.Controller.Private.Facturacion
{
    public class FacturacionControllerAddInvoiceResponse : BaseObjectResponse<Data> {}

    public class Data
    {
        public Guid Id { get; set; }
    }
}
