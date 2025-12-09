using Domain.Controller.Private.Printing;
using Domain.Environment;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace Business.Services.Printing
{
    /// <summary>
    /// Implementación del servicio de impresión usando ESC/POS
    /// </summary>
    public class EscPosReceiptPrinterService : IReceiptPrinterService
    {
        private readonly EscPosPrinterOptions _options;
        private readonly ILogger<EscPosReceiptPrinterService> _logger;
        private readonly IHostEnvironment _environment;
        private readonly EPSON _emitter;

        // Ancho del ticket en caracteres (típico para impresoras de 58mm o 80mm)
        private const int TICKET_WIDTH = 42;

        public EscPosReceiptPrinterService(
            IOptions<EscPosPrinterOptions> options,
            ILogger<EscPosReceiptPrinterService> logger,
            IHostEnvironment environment)
        {
            _options = options.Value;
            _logger = logger;
            _environment = environment;
            _emitter = new EPSON();
        }

        /// <inheritdoc />
        public async Task PrintInvoiceAsync(PrintInvoiceDto invoice)
        {
            try
            {
                var isDevelopment = _environment.IsDevelopment();
                var useFilePrinter = _options.UseFilePrinter || isDevelopment;

                _logger.LogInformation(
                    "Iniciando impresión de factura {InvoiceId} en impresora {PrinterName}. Ambiente: {Environment}, UseFilePrinter: {UseFilePrinter}",
                    invoice.Data.Id, _options.PrinterName, _environment.EnvironmentName, useFilePrinter);

                // Construir el buffer ESC/POS
                var buffer = BuildEscPosBuffer(invoice);

                if (useFilePrinter)
                {
                    await PrintToFileAsync(buffer);
                }
                else
                {
                    await PrintToNetworkAsync(buffer);
                }

                _logger.LogInformation("Factura {InvoiceId} impresa exitosamente", invoice.Data.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al imprimir factura {InvoiceId}", invoice.Data.Id);
                throw;
            }
        }

        /// <inheritdoc />
        public string BuildInvoicePreview(PrintInvoiceDto invoice)
        {
            var sb = new StringBuilder();
            var data = invoice.Data;

            // Encabezado
            sb.AppendLine(CenterText("=".PadRight(TICKET_WIDTH, '=')));
            sb.AppendLine(CenterText(_options.BusinessName));
            
            if (!string.IsNullOrEmpty(_options.BusinessAddress))
                sb.AppendLine(CenterText(_options.BusinessAddress));
            
            if (!string.IsNullOrEmpty(_options.BusinessPhone))
                sb.AppendLine(CenterText($"Tel: {_options.BusinessPhone}"));
            
            if (!string.IsNullOrEmpty(_options.BusinessRnc))
                sb.AppendLine(CenterText($"RNC: {_options.BusinessRnc}"));

            sb.AppendLine(CenterText("*** FACTURA ***"));
            sb.AppendLine(CenterText("=".PadRight(TICKET_WIDTH, '=')));
            sb.AppendLine();

            // Información de la factura
            sb.AppendLine($"Caja: {data.Caja.Codigo} - {data.Caja.Nombre}");
            sb.AppendLine($"Tipo: {data.Tipo.Nombre}");
            sb.AppendLine($"Estado: {data.Estado.Nombre}");
            sb.AppendLine($"Fecha: {data.FechaEmision:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Factura #: {data.Id.ToString()[..8].ToUpper()}");
            sb.AppendLine("-".PadRight(TICKET_WIDTH, '-'));

            // Encabezado de detalles
            sb.AppendLine(FormatDetailHeader());
            sb.AppendLine("-".PadRight(TICKET_WIDTH, '-'));

            // Detalles de productos
            foreach (var detalle in data.FacturaDetalles)
            {
                // Línea 1: Nombre del producto
                var nombreTruncado = TruncateText(detalle.NombreProducto, TICKET_WIDTH);
                sb.AppendLine(nombreTruncado);

                // Línea 2: Cantidad x Precio = Total (con ITBIS)
                var lineaDetalle = $"  {detalle.Cantida} x {detalle.PrecioUnitario:N2} = {detalle.Total:N2} ({detalle.ImpuestoPorcentaje:N0}%)";
                sb.AppendLine(lineaDetalle);
            }

            sb.AppendLine("-".PadRight(TICKET_WIDTH, '-'));

            // Totales
            sb.AppendLine(FormatTotalLine("SUBTOTAL:", data.Subtotal));
            sb.AppendLine(FormatTotalLine("ITBIS:", data.Impuestos));
            sb.AppendLine("=".PadRight(TICKET_WIDTH, '='));
            sb.AppendLine(FormatTotalLine("TOTAL:", data.Total, bold: true));
            sb.AppendLine("=".PadRight(TICKET_WIDTH, '='));
            sb.AppendLine();

            // Mensaje de cierre
            sb.AppendLine(CenterText("¡Gracias por su compra!"));
            sb.AppendLine(CenterText("Vuelva pronto"));
            sb.AppendLine();
            sb.AppendLine(CenterText($"Impreso: {DateTime.Now:dd/MM/yyyy HH:mm:ss}"));

            return sb.ToString();
        }

        /// <summary>
        /// Construye el buffer de comandos ESC/POS para la impresora
        /// </summary>
        private byte[] BuildEscPosBuffer(PrintInvoiceDto invoice)
        {
            var data = invoice.Data;
            var commands = new List<byte[]>();

            // Inicializar impresora
            commands.Add(_emitter.Initialize());
            commands.Add(_emitter.SetStyles(PrintStyle.None));

            // Encabezado centrado
            commands.Add(_emitter.CenterAlign());
            commands.Add(_emitter.SetStyles(PrintStyle.DoubleHeight | PrintStyle.DoubleWidth | PrintStyle.Bold));
            commands.Add(_emitter.PrintLine(_options.BusinessName));
            commands.Add(_emitter.SetStyles(PrintStyle.None));

            if (!string.IsNullOrEmpty(_options.BusinessAddress))
                commands.Add(_emitter.PrintLine(_options.BusinessAddress));

            if (!string.IsNullOrEmpty(_options.BusinessPhone))
                commands.Add(_emitter.PrintLine($"Tel: {_options.BusinessPhone}"));

            if (!string.IsNullOrEmpty(_options.BusinessRnc))
                commands.Add(_emitter.PrintLine($"RNC: {_options.BusinessRnc}"));

            commands.Add(_emitter.PrintLine(""));
            commands.Add(_emitter.SetStyles(PrintStyle.Bold));
            commands.Add(_emitter.PrintLine("*** FACTURA ***"));
            commands.Add(_emitter.SetStyles(PrintStyle.None));
            commands.Add(_emitter.PrintLine("=".PadRight(TICKET_WIDTH, '=')));
            commands.Add(_emitter.PrintLine(""));

            // Información de la factura (alineado izquierda)
            commands.Add(_emitter.LeftAlign());
            commands.Add(_emitter.PrintLine($"Caja: {data.Caja.Codigo} - {data.Caja.Nombre}"));
            commands.Add(_emitter.PrintLine($"Tipo: {data.Tipo.Nombre}"));
            commands.Add(_emitter.PrintLine($"Estado: {data.Estado.Nombre}"));
            commands.Add(_emitter.PrintLine($"Fecha: {data.FechaEmision:dd/MM/yyyy HH:mm:ss}"));
            commands.Add(_emitter.PrintLine($"Factura #: {data.Id.ToString()[..8].ToUpper()}"));
            commands.Add(_emitter.PrintLine("-".PadRight(TICKET_WIDTH, '-')));

            // Encabezado de detalles
            commands.Add(_emitter.SetStyles(PrintStyle.Bold));
            commands.Add(_emitter.PrintLine(FormatDetailHeader()));
            commands.Add(_emitter.SetStyles(PrintStyle.None));
            commands.Add(_emitter.PrintLine("-".PadRight(TICKET_WIDTH, '-')));

            // Detalles de productos
            foreach (var detalle in data.FacturaDetalles)
            {
                var nombreTruncado = TruncateText(detalle.NombreProducto, TICKET_WIDTH);
                commands.Add(_emitter.PrintLine(nombreTruncado));
                commands.Add(_emitter.PrintLine($"  {detalle.Cantida} x {detalle.PrecioUnitario:N2} = {detalle.Total:N2} ({detalle.ImpuestoPorcentaje:N0}%)"));
            }

            commands.Add(_emitter.PrintLine("-".PadRight(TICKET_WIDTH, '-')));

            // Totales
            commands.Add(_emitter.PrintLine(FormatTotalLine("SUBTOTAL:", data.Subtotal)));
            commands.Add(_emitter.PrintLine(FormatTotalLine("ITBIS:", data.Impuestos)));
            commands.Add(_emitter.PrintLine("=".PadRight(TICKET_WIDTH, '=')));

            commands.Add(_emitter.SetStyles(PrintStyle.Bold | PrintStyle.DoubleHeight));
            commands.Add(_emitter.PrintLine(FormatTotalLine("TOTAL:", data.Total)));
            commands.Add(_emitter.SetStyles(PrintStyle.None));
            commands.Add(_emitter.PrintLine("=".PadRight(TICKET_WIDTH, '=')));
            commands.Add(_emitter.PrintLine(""));

            // Mensaje de cierre centrado
            commands.Add(_emitter.CenterAlign());
            commands.Add(_emitter.PrintLine("¡Gracias por su compra!"));
            commands.Add(_emitter.PrintLine("Vuelva pronto"));
            commands.Add(_emitter.PrintLine(""));
            commands.Add(_emitter.PrintLine($"Impreso: {DateTime.Now:dd/MM/yyyy HH:mm:ss}"));
            commands.Add(_emitter.PrintLine(""));
            commands.Add(_emitter.PrintLine(""));

            // Cortar papel
            commands.Add(_emitter.PartialCutAfterFeed(5));

            // Combinar todos los comandos en un solo buffer
            return ByteSplicer.Combine(commands.ToArray());
        }

        /// <summary>
        /// Imprime a un archivo en modo desarrollo
        /// </summary>
        private async Task PrintToFileAsync(byte[] buffer)
        {
            var outputDir = Path.Combine(_environment.ContentRootPath, "PrinterOutput");

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                _logger.LogInformation("Directorio PrinterOutput creado en {Path}", outputDir);
            }

            var filePath = Path.Combine(outputDir, "ticket-ultimo.bin");
            await File.WriteAllBytesAsync(filePath, buffer);

            _logger.LogInformation(
                "Ticket guardado en archivo (modo desarrollo): {FilePath} ({Size} bytes)",
                filePath, buffer.Length);
        }

        /// <summary>
        /// Imprime a una impresora de red en modo producción
        /// </summary>
        private async Task PrintToNetworkAsync(byte[] buffer)
        {
            _logger.LogInformation(
                "Conectando a impresora de red {Host}:{Port}",
                _options.Host, _options.Port);

            var printer = new ImmediateNetworkPrinter(
                new ImmediateNetworkPrinterSettings
                {
                    ConnectionString = $"{_options.Host}:{_options.Port}",
                    PrinterName = _options.PrinterName
                });

            await Task.Run(() => printer.WriteAsync(buffer));

            _logger.LogInformation("Datos enviados a la impresora de red exitosamente");
        }

        /// <inheritdoc />
        public string BuildInvoiceHtmlPreview(PrintInvoiceDto invoice)
        {
            var data = invoice.Data;
            var sb = new StringBuilder();

            sb.AppendLine(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        .ticket {
            width: 300px;
            max-width: 100%;
            margin: 0 auto;
            padding: 15px;
            font-family: 'Courier New', Courier, monospace;
            font-size: 12px;
            background: #fff;
            box-shadow: 0 0 10px rgba(0,0,0,0.15);
            border-radius: 3px;
        }
        .ticket-header {
            text-align: center;
            border-bottom: 2px dashed #000;
            padding-bottom: 10px;
            margin-bottom: 10px;
        }
        .business-name {
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 5px;
        }
        .business-info {
            font-size: 11px;
            color: #333;
        }
        .invoice-title {
            font-size: 16px;
            font-weight: bold;
            margin-top: 10px;
            background: #000;
            color: #fff;
            padding: 5px;
        }
        .invoice-info {
            margin: 10px 0;
            padding: 10px 0;
            border-bottom: 1px dashed #ccc;
        }
        .invoice-info p {
            margin: 3px 0;
        }
        .invoice-info strong {
            display: inline-block;
            min-width: 70px;
        }
        .items-header {
            display: flex;
            justify-content: space-between;
            font-weight: bold;
            border-bottom: 1px solid #000;
            padding: 5px 0;
            margin-bottom: 5px;
        }
        .item {
            border-bottom: 1px dotted #ccc;
            padding: 8px 0;
        }
        .item-name {
            font-weight: bold;
            margin-bottom: 3px;
        }
        .item-details {
            display: flex;
            justify-content: space-between;
            font-size: 11px;
            color: #555;
        }
        .totals {
            margin-top: 10px;
            padding-top: 10px;
            border-top: 2px dashed #000;
        }
        .total-line {
            display: flex;
            justify-content: space-between;
            margin: 5px 0;
        }
        .total-line.grand-total {
            font-size: 16px;
            font-weight: bold;
            border-top: 2px solid #000;
            border-bottom: 2px solid #000;
            padding: 8px 0;
            margin-top: 10px;
        }
        .footer {
            text-align: center;
            margin-top: 15px;
            padding-top: 10px;
            border-top: 2px dashed #000;
        }
        .footer p {
            margin: 5px 0;
        }
        .thank-you {
            font-size: 14px;
            font-weight: bold;
        }
        .timestamp {
            font-size: 10px;
            color: #666;
            margin-top: 10px;
        }
    </style>
</head>
<body>
    <div class='ticket'>");

            // Header
            sb.AppendLine($@"
        <div class='ticket-header'>
            <div class='business-name'>{WebUtility.HtmlEncode(_options.BusinessName)}</div>");

            if (!string.IsNullOrEmpty(_options.BusinessAddress))
                sb.AppendLine($"            <div class='business-info'>{WebUtility.HtmlEncode(_options.BusinessAddress)}</div>");

            if (!string.IsNullOrEmpty(_options.BusinessPhone))
                sb.AppendLine($"            <div class='business-info'>Tel: {WebUtility.HtmlEncode(_options.BusinessPhone)}</div>");

            if (!string.IsNullOrEmpty(_options.BusinessRnc))
                sb.AppendLine($"            <div class='business-info'>RNC: {WebUtility.HtmlEncode(_options.BusinessRnc)}</div>");

            sb.AppendLine($@"
            <div class='invoice-title'>★ FACTURA ★</div>
        </div>");

            // Invoice Info
            sb.AppendLine($@"
        <div class='invoice-info'>
            <p><strong>Caja:</strong> {WebUtility.HtmlEncode(data.Caja.Codigo)} - {WebUtility.HtmlEncode(data.Caja.Nombre)}</p>
            <p><strong>Tipo:</strong> {WebUtility.HtmlEncode(data.Tipo.Nombre)}</p>
            <p><strong>Estado:</strong> {WebUtility.HtmlEncode(data.Estado.Nombre)}</p>
            <p><strong>Fecha:</strong> {data.FechaEmision:dd/MM/yyyy HH:mm}</p>
            <p><strong>Factura #:</strong> {data.Id.ToString()[..8].ToUpper()}</p>
        </div>");

            // Items Header
            sb.AppendLine(@"
        <div class='items-header'>
            <span>DESCRIPCIÓN</span>
            <span>TOTAL</span>
        </div>
        <div class='items'>");

            // Items
            foreach (var detalle in data.FacturaDetalles)
            {
                sb.AppendLine($@"
            <div class='item'>
                <div class='item-name'>{WebUtility.HtmlEncode(detalle.NombreProducto)}</div>
                <div class='item-details'>
                    <span>{detalle.Cantida} x RD$ {detalle.PrecioUnitario:N2}</span>
                    <span>RD$ {detalle.Total:N2}</span>
                </div>
                <div class='item-details'>
                    <span>ITBIS ({detalle.ImpuestoPorcentaje:N0}%): RD$ {detalle.Impuestos:N2}</span>
                </div>
            </div>");
            }

            sb.AppendLine("        </div>");

            // Totals
            sb.AppendLine($@"
        <div class='totals'>
            <div class='total-line'>
                <span>SUBTOTAL:</span>
                <span>RD$ {data.Subtotal:N2}</span>
            </div>
            <div class='total-line'>
                <span>ITBIS:</span>
                <span>RD$ {data.Impuestos:N2}</span>
            </div>
            <div class='total-line grand-total'>
                <span>TOTAL:</span>
                <span>RD$ {data.Total:N2}</span>
            </div>
        </div>");

            // Footer
            sb.AppendLine($@"
        <div class='footer'>
            <p class='thank-you'>¡Gracias por su compra!</p>
            <p>Vuelva pronto</p>
            <p class='timestamp'>Impreso: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
        </div>
    </div>
</body>
</html>");

            return sb.ToString();
        }

        #region Helper Methods

        /// <summary>
        /// Centra el texto en el ancho del ticket
        /// </summary>
        private static string CenterText(string text)
        {
            if (text.Length >= TICKET_WIDTH)
                return text[..TICKET_WIDTH];

            var padding = (TICKET_WIDTH - text.Length) / 2;
            return text.PadLeft(text.Length + padding).PadRight(TICKET_WIDTH);
        }

        /// <summary>
        /// Trunca el texto si excede el ancho máximo
        /// </summary>
        private static string TruncateText(string text, int maxLength)
        {
            if (text.Length <= maxLength)
                return text;

            return text[..(maxLength - 3)] + "...";
        }

        /// <summary>
        /// Formatea el encabezado de la sección de detalles
        /// </summary>
        private static string FormatDetailHeader()
        {
            return "DESCRIPCION";
        }

        /// <summary>
        /// Formatea una línea de total alineada a la derecha
        /// </summary>
        private static string FormatTotalLine(string label, decimal amount, bool bold = false)
        {
            var amountStr = $"RD$ {amount:N2}";
            var spaces = TICKET_WIDTH - label.Length - amountStr.Length;
            return label + new string(' ', Math.Max(1, spaces)) + amountStr;
        }

        #endregion
    }
}
