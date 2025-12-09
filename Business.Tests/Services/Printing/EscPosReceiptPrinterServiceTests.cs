using Business.Services.Printing;
using Domain.Controller.Private.Printing;
using Domain.Environment;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Business.Tests.Services.Printing
{
    public class EscPosReceiptPrinterServiceTests
    {
        private readonly Mock<IOptions<EscPosPrinterOptions>> _optionsMock;
        private readonly Mock<ILogger<EscPosReceiptPrinterService>> _loggerMock;
        private readonly Mock<IHostEnvironment> _environmentMock;
        private readonly EscPosPrinterOptions _printerOptions;

        public EscPosReceiptPrinterServiceTests()
        {
            _printerOptions = new EscPosPrinterOptions
            {
                Host = "192.168.0.50",
                Port = 9100,
                PrinterName = "TestPrinter",
                BusinessName = "TEST BUSINESS",
                BusinessAddress = "Test Address 123",
                BusinessPhone = "809-555-1234",
                BusinessRnc = "123456789"
            };

            _optionsMock = new Mock<IOptions<EscPosPrinterOptions>>();
            _optionsMock.Setup(x => x.Value).Returns(_printerOptions);

            _loggerMock = new Mock<ILogger<EscPosReceiptPrinterService>>();
            _environmentMock = new Mock<IHostEnvironment>();
        }

        private PrintInvoiceDto CreateSampleInvoice()
        {
            return new PrintInvoiceDto
            {
                Ok = true,
                Data = new PrintInvoiceDataDto
                {
                    Id = Guid.Parse("6f3d4742-0b6b-4860-bd74-6084bb45782a"),
                    TipoID = 1,
                    EstadoId = 1,
                    CajaId = Guid.Parse("0768bd87-b1f6-4843-8ef1-08de3454633a"),
                    UsuarioId = Guid.Parse("0ff341fc-835d-4d6a-685f-08de31f4f351"),
                    FechaEmision = new DateTime(2025, 12, 5, 20, 37, 27),
                    Total = 95m,
                    Subtotal = 80.51m,
                    Impuestos = 14.49m,
                    Tipo = new PrintInvoiceTipoDto { Id = 1, Nombre = "DEBITO" },
                    Estado = new PrintInvoiceEstadoDto { Id = 1, Nombre = "EMITIDA" },
                    Caja = new PrintInvoiceCajaDto
                    {
                        Id = Guid.Parse("0768bd87-b1f6-4843-8ef1-08de3454633a"),
                        Codigo = "001",
                        Nombre = "001-Diciembre"
                    },
                    FacturaDetalles = new List<PrintInvoiceDetalleDto>
                    {
                        new PrintInvoiceDetalleDto
                        {
                            ProductoId = "002",
                            NombreProducto = "Chocolate Embajador",
                            Cantida = 1,
                            ImpuestoPorcentaje = 18,
                            PrecioUnitario = 95m,
                            Total = 95m,
                            Subtotal = 80.51m,
                            Impuestos = 14.49m
                        }
                    }
                }
            };
        }

        [Fact]
        public void BuildInvoicePreview_ShouldContainBusinessName()
        {
            // Arrange
            _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
            var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
            var invoice = CreateSampleInvoice();

            // Act
            var preview = service.BuildInvoicePreview(invoice);

            // Assert
            Assert.Contains(_printerOptions.BusinessName, preview);
        }

        [Fact]
        public void BuildInvoicePreview_ShouldContainTotal()
        {
            // Arrange
            _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
            var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
            var invoice = CreateSampleInvoice();

            // Act
            var preview = service.BuildInvoicePreview(invoice);

            // Assert
            Assert.Contains("TOTAL", preview);
            Assert.Contains("95.00", preview);
        }

        [Fact]
        public void BuildInvoicePreview_ShouldContainProductName()
        {
            // Arrange
            _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
            var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
            var invoice = CreateSampleInvoice();

            // Act
            var preview = service.BuildInvoicePreview(invoice);

            // Assert
            Assert.Contains("Chocolate Embajador", preview);
        }

        [Fact]
        public void BuildInvoicePreview_ShouldContainSubtotal()
        {
            // Arrange
            _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
            var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
            var invoice = CreateSampleInvoice();

            // Act
            var preview = service.BuildInvoicePreview(invoice);

            // Assert
            Assert.Contains("SUBTOTAL", preview);
            Assert.Contains("80.51", preview);
        }

        [Fact]
        public void BuildInvoicePreview_ShouldContainITBIS()
        {
            // Arrange
            _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
            var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
            var invoice = CreateSampleInvoice();

            // Act
            var preview = service.BuildInvoicePreview(invoice);

            // Assert
            Assert.Contains("ITBIS", preview);
            Assert.Contains("14.49", preview);
        }

        [Fact]
        public void BuildInvoicePreview_ShouldContainThankYouMessage()
        {
            // Arrange
            _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
            var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
            var invoice = CreateSampleInvoice();

            // Act
            var preview = service.BuildInvoicePreview(invoice);

            // Assert
            Assert.Contains("Gracias por su compra", preview);
        }

        [Fact]
        public void BuildInvoicePreview_ShouldContainCajaInfo()
        {
            // Arrange
            _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
            var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
            var invoice = CreateSampleInvoice();

            // Act
            var preview = service.BuildInvoicePreview(invoice);

            // Assert
            Assert.Contains("001", preview);
            Assert.Contains("001-Diciembre", preview);
        }

        [Fact]
        public void BuildInvoicePreview_ShouldContainInvoiceType()
        {
            // Arrange
            _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
            var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
            var invoice = CreateSampleInvoice();

            // Act
            var preview = service.BuildInvoicePreview(invoice);

            // Assert
            Assert.Contains("DEBITO", preview);
        }

        [Fact]
        public async Task PrintInvoiceAsync_InDevelopment_ShouldCreatePrinterOutputDirectory()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"PrinterTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempPath);
            
            try
            {
                _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
                _environmentMock.Setup(x => x.ContentRootPath).Returns(tempPath);
                
                var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
                var invoice = CreateSampleInvoice();

                // Act
                await service.PrintInvoiceAsync(invoice);

                // Assert
                var outputDir = Path.Combine(tempPath, "PrinterOutput");
                Assert.True(Directory.Exists(outputDir));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }

        [Fact]
        public async Task PrintInvoiceAsync_InDevelopment_ShouldCreateTicketFile()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"PrinterTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempPath);
            
            try
            {
                _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
                _environmentMock.Setup(x => x.ContentRootPath).Returns(tempPath);
                
                var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
                var invoice = CreateSampleInvoice();

                // Act
                await service.PrintInvoiceAsync(invoice);

                // Assert
                var ticketPath = Path.Combine(tempPath, "PrinterOutput", "ticket-ultimo.bin");
                Assert.True(File.Exists(ticketPath));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }

        [Fact]
        public async Task PrintInvoiceAsync_InDevelopment_ShouldCreateTicketWithContent()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"PrinterTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempPath);
            
            try
            {
                _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
                _environmentMock.Setup(x => x.ContentRootPath).Returns(tempPath);
                
                var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
                var invoice = CreateSampleInvoice();

                // Act
                await service.PrintInvoiceAsync(invoice);

                // Assert
                var ticketPath = Path.Combine(tempPath, "PrinterOutput", "ticket-ultimo.bin");
                var fileInfo = new FileInfo(ticketPath);
                Assert.True(fileInfo.Length > 0, "El archivo del ticket debe tener contenido");
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }

        [Fact]
        public void BuildInvoicePreview_WithMultipleProducts_ShouldListAllProducts()
        {
            // Arrange
            _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
            var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
            
            var invoice = CreateSampleInvoice();
            ((List<PrintInvoiceDetalleDto>)invoice.Data.FacturaDetalles).Add(new PrintInvoiceDetalleDto
            {
                ProductoId = "003",
                NombreProducto = "Coca Cola 2L",
                Cantida = 2,
                ImpuestoPorcentaje = 18,
                PrecioUnitario = 75m,
                Total = 150m,
                Subtotal = 127.12m,
                Impuestos = 22.88m
            });

            // Act
            var preview = service.BuildInvoicePreview(invoice);

            // Assert
            Assert.Contains("Chocolate Embajador", preview);
            Assert.Contains("Coca Cola 2L", preview);
        }

        [Fact]
        public void BuildInvoicePreview_ShouldContainBusinessAddress()
        {
            // Arrange
            _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
            var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
            var invoice = CreateSampleInvoice();

            // Act
            var preview = service.BuildInvoicePreview(invoice);

            // Assert
            Assert.Contains("Test Address 123", preview);
        }

        [Fact]
        public void BuildInvoicePreview_ShouldContainBusinessRnc()
        {
            // Arrange
            _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
            var service = new EscPosReceiptPrinterService(_optionsMock.Object, _loggerMock.Object, _environmentMock.Object);
            var invoice = CreateSampleInvoice();

            // Act
            var preview = service.BuildInvoicePreview(invoice);

            // Assert
            Assert.Contains("RNC: 123456789", preview);
        }
    }
}
