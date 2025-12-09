namespace Domain.Controller.Private.Printing
{
    /// <summary>
    /// Respuesta de la vista previa de impresión
    /// </summary>
    public class PrintInvoicePreviewResponse
    {
        /// <summary>
        /// Vista previa del ticket en texto plano
        /// </summary>
        public required string Preview { get; set; }
    }
}
