namespace Domain.Controller.Private.Printing
{
    /// <summary>
    /// DTO principal para la solicitud de impresión de factura
    /// </summary>
    public class PrintInvoiceDto
    {
        /// <summary>
        /// Indica si la operación fue exitosa
        /// </summary>
        public bool Ok { get; set; }

        /// <summary>
        /// Datos de la factura a imprimir
        /// </summary>
        public required PrintInvoiceDataDto Data { get; set; }
    }

    /// <summary>
    /// Datos completos de la factura
    /// </summary>
    public class PrintInvoiceDataDto
    {
        /// <summary>
        /// ID único de la factura
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID del tipo de factura (1 = DEBITO, 2 = CREDITO, etc.)
        /// </summary>
        public short TipoID { get; set; }

        /// <summary>
        /// ID del estado de la factura
        /// </summary>
        public short EstadoId { get; set; }

        /// <summary>
        /// ID de la caja donde se generó la factura
        /// </summary>
        public Guid CajaId { get; set; }

        /// <summary>
        /// ID del usuario que generó la factura
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Fecha y hora de emisión de la factura
        /// </summary>
        public DateTime FechaEmision { get; set; }

        /// <summary>
        /// Total de la factura (subtotal + impuestos)
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Subtotal antes de impuestos
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Total de impuestos
        /// </summary>
        public decimal Impuestos { get; set; }

        /// <summary>
        /// Información del tipo de factura
        /// </summary>
        public required PrintInvoiceTipoDto Tipo { get; set; }

        /// <summary>
        /// Información del estado de la factura
        /// </summary>
        public required PrintInvoiceEstadoDto Estado { get; set; }

        /// <summary>
        /// Información de la caja
        /// </summary>
        public required PrintInvoiceCajaDto Caja { get; set; }

        /// <summary>
        /// Lista de productos/detalles de la factura
        /// </summary>
        public required ICollection<PrintInvoiceDetalleDto> FacturaDetalles { get; set; }
    }

    /// <summary>
    /// Información del tipo de factura
    /// </summary>
    public class PrintInvoiceTipoDto
    {
        public short Id { get; set; }
        public required string Nombre { get; set; }
    }

    /// <summary>
    /// Información del estado de la factura
    /// </summary>
    public class PrintInvoiceEstadoDto
    {
        public short Id { get; set; }
        public required string Nombre { get; set; }
    }

    /// <summary>
    /// Información de la caja
    /// </summary>
    public class PrintInvoiceCajaDto
    {
        public Guid Id { get; set; }
        public required string Codigo { get; set; }
        public required string Nombre { get; set; }
    }

    /// <summary>
    /// Detalle de un producto en la factura
    /// </summary>
    public class PrintInvoiceDetalleDto
    {
        /// <summary>
        /// ID del producto
        /// </summary>
        public required string ProductoId { get; set; }

        /// <summary>
        /// Nombre del producto
        /// </summary>
        public required string NombreProducto { get; set; }

        /// <summary>
        /// Cantidad de unidades
        /// </summary>
        public int Cantida { get; set; }

        /// <summary>
        /// Porcentaje de impuesto aplicado
        /// </summary>
        public decimal ImpuestoPorcentaje { get; set; }

        /// <summary>
        /// Precio unitario del producto
        /// </summary>
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Total de la línea (subtotal + impuestos)
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Subtotal de la línea antes de impuestos
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Impuestos de la línea
        /// </summary>
        public decimal Impuestos { get; set; }
    }
}
