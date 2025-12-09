using Business.Services.Printing;
using Domain.API;
using Domain.Controller.Private.Printing;
using Domain.Environment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    /// <summary>
    /// Controlador para impresión de recibos ESC/POS
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PrintingController : ControllerBase
    {
        private readonly IReceiptPrinterService _printerService;
        private readonly ILogger<PrintingController> _logger;
        private readonly EscPosPrinterOptions _printerOptions;

        public PrintingController(
            IReceiptPrinterService printerService,
            ILogger<PrintingController> logger,
            IOptions<EscPosPrinterOptions> printerOptions)
        {
            _printerService = printerService;
            _logger = logger;
            _printerOptions = printerOptions.Value;
        }

        /// <summary>
        /// Imprime una factura usando la impresora ESC/POS configurada
        /// </summary>
        /// <param name="invoice">Datos de la factura a imprimir</param>
        /// <returns>NoContent si la impresión fue exitosa</returns>
        [HttpPost("invoice")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Imprimir factura",
            Description = "Envía la factura a la impresora ESC/POS. En desarrollo guarda a archivo, en producción envía a impresora de red."
        )]
        public async Task<IActionResult> PrintInvoice([FromBody] PrintInvoiceDto invoice)
        {
            if (invoice?.Data == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "Los datos de la factura son requeridos."
                });
            }

            if (invoice.Data.FacturaDetalles == null || !invoice.Data.FacturaDetalles.Any())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La factura debe tener al menos un detalle."
                });
            }

            try
            {
                await _printerService.PrintInvoiceAsync(invoice);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al imprimir factura {InvoiceId}", invoice.Data.Id);
                return StatusCode(StatusCodes.Status500InternalServerError, new BadRequestResponse
                {
                    BadMessage = $"Error al imprimir: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Genera una vista previa en texto del ticket de impresión
        /// </summary>
        /// <param name="invoice">Datos de la factura</param>
        /// <returns>Vista previa del ticket en texto plano</returns>
        [HttpPost("invoice/preview")]
        [ProducesResponseType<PrintInvoicePreviewResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Vista previa del ticket",
            Description = "Genera una vista previa en texto plano del ticket de factura. Útil para debugging y pruebas."
        )]
        public IActionResult PrintInvoicePreview([FromBody] PrintInvoiceDto invoice)
        {
            if (invoice?.Data == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "Los datos de la factura son requeridos."
                });
            }

            if (invoice.Data.FacturaDetalles == null || !invoice.Data.FacturaDetalles.Any())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La factura debe tener al menos un detalle."
                });
            }

            try
            {
                var preview = _printerService.BuildInvoicePreview(invoice);
                return Ok(new PrintInvoicePreviewResponse
                {
                    Preview = preview
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar vista previa de factura {InvoiceId}", invoice.Data.Id);
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = $"Error al generar vista previa: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Genera una vista previa HTML estilizada del ticket
        /// </summary>
        /// <param name="invoice">Datos de la factura</param>
        /// <returns>HTML del ticket como string en JSON</returns>
        [HttpPost("invoice/preview/html")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Vista previa HTML del ticket",
            Description = "Genera una vista previa HTML estilizada del ticket. El HTML puede ser mostrado en un iframe o modal."
        )]
        public IActionResult PrintInvoiceHtmlPreview([FromBody] PrintInvoiceDto invoice)
        {
            if (invoice?.Data == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "Los datos de la factura son requeridos."
                });
            }

            if (invoice.Data.FacturaDetalles == null || !invoice.Data.FacturaDetalles.Any())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La factura debe tener al menos un detalle."
                });
            }

            try
            {
                var html = _printerService.BuildInvoiceHtmlPreview(invoice);
                return Ok(new { html });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar vista HTML de factura {InvoiceId}", invoice.Data.Id);
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = $"Error al generar vista HTML: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Renderiza el ticket directamente como HTML (para visualizar en navegador)
        /// </summary>
        /// <param name="invoice">Datos de la factura</param>
        /// <returns>Página HTML con el ticket</returns>
        [HttpPost("invoice/render")]
        [Produces("text/html")]
        [SwaggerOperation(
            Summary = "Renderizar ticket como HTML",
            Description = "Renderiza el ticket directamente como una página HTML. Útil para abrir en una nueva pestaña o imprimir desde el navegador."
        )]
        public IActionResult RenderInvoiceHtml([FromBody] PrintInvoiceDto invoice)
        {
            if (invoice?.Data == null)
            {
                return BadRequest("Los datos de la factura son requeridos.");
            }

            if (invoice.Data.FacturaDetalles == null || !invoice.Data.FacturaDetalles.Any())
            {
                return BadRequest("La factura debe tener al menos un detalle.");
            }

            try
            {
                var html = _printerService.BuildInvoiceHtmlPreview(invoice);
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al renderizar factura {InvoiceId}", invoice.Data.Id);
                return BadRequest($"Error al renderizar: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene información sobre el estado de la configuración de impresión
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Estado de la impresora",
            Description = "Devuelve información sobre la configuración actual de la impresora."
        )]
        public IActionResult GetPrinterStatus([FromServices] IWebHostEnvironment env)
        {
            var isDevelopment = env.IsDevelopment();
            var useFilePrinter = _printerOptions.UseFilePrinter || isDevelopment;

            return Ok(new
            {
                Environment = env.EnvironmentName,
                IsDevelopment = isDevelopment,
                UseFilePrinter = useFilePrinter,
                Mode = useFilePrinter ? "File (archivo local)" : "Network (impresora de red)",
                PrinterName = _printerOptions.PrinterName,
                NetworkHost = useFilePrinter ? "(no aplica)" : $"{_printerOptions.Host}:{_printerOptions.Port}",
                BusinessName = _printerOptions.BusinessName,
                Message = useFilePrinter
                    ? "Los tickets se guardan en PrinterOutput/ticket-ultimo.bin"
                    : $"Los tickets se envían a {_printerOptions.Host}:{_printerOptions.Port}"
            });
        }
    }
}
