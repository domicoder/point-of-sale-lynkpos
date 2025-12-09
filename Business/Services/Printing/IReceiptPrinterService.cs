using Domain.Controller.Private.Printing;

namespace Business.Services.Printing
{
    /// <summary>
    /// Interfaz para el servicio de impresión de recibos
    /// </summary>
    public interface IReceiptPrinterService
    {
        /// <summary>
        /// Imprime una factura usando ESC/POS
        /// </summary>
        /// <param name="invoice">Datos de la factura a imprimir</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task PrintInvoiceAsync(PrintInvoiceDto invoice);

        /// <summary>
        /// Genera una vista previa en texto plano del ticket
        /// </summary>
        /// <param name="invoice">Datos de la factura</param>
        /// <returns>String con el formato del ticket legible</returns>
        string BuildInvoicePreview(PrintInvoiceDto invoice);

        /// <summary>
        /// Genera una vista previa HTML estilizada del ticket
        /// </summary>
        /// <param name="invoice">Datos de la factura</param>
        /// <returns>HTML con estilos de ticket térmico</returns>
        string BuildInvoiceHtmlPreview(PrintInvoiceDto invoice);
    }
}
