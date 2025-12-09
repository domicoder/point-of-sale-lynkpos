namespace Domain.Environment
{
    /// <summary>
    /// Opciones de configuración para la impresora ESC/POS
    /// </summary>
    public class EscPosPrinterOptions
    {
        /// <summary>
        /// Dirección IP o hostname de la impresora de red
        /// </summary>
        public string Host { get; set; } = "192.168.0.50";

        /// <summary>
        /// Puerto de la impresora (típicamente 9100 para impresoras de red)
        /// </summary>
        public int Port { get; set; } = 9100;

        /// <summary>
        /// Nombre identificador de la impresora
        /// </summary>
        public string PrinterName { get; set; } = "Caja01";

        /// <summary>
        /// Nombre del negocio para mostrar en los recibos
        /// </summary>
        public string BusinessName { get; set; } = "MI NEGOCIO";

        /// <summary>
        /// Dirección del negocio (opcional)
        /// </summary>
        public string? BusinessAddress { get; set; }

        /// <summary>
        /// Teléfono del negocio (opcional)
        /// </summary>
        public string? BusinessPhone { get; set; }

        /// <summary>
        /// RNC del negocio (opcional)
        /// </summary>
        public string? BusinessRnc { get; set; }

        /// <summary>
        /// Si es true, usa FilePrinter (archivo) en lugar de impresora de red.
        /// Útil para desarrollo o cuando no hay impresora física disponible.
        /// Por defecto es true para evitar errores de conexión.
        /// </summary>
        public bool UseFilePrinter { get; set; } = true;
    }
}
