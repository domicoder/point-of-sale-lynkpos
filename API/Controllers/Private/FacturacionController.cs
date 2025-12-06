using Business.Controllers;
using Business.Services.INVENTARIO_API;
using Data.Repositories;
using Domain.API;
using Domain.Controller.Private.Facturacion;
using Domain.Controller.Private.Usuario;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FacturacionController(
        AuthService _authService,
        InventarioApiProductoService _inventarioApiProductoService,
        CajaRepository _cajaRepository,
        CajaBitacoraRepository _cajaBitacoraRepository,
        FacturaRepository _facturaRepository,
        FacturaDetalleRepository _facturaDetalleRepository
    ) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<EnumModelResponse<short>>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
              Summary = "Obtener lista de tipos",
              Description = "Devuelve una lista de los tipos de una factura."
          )]
        public IActionResult GetTypeList()
        {
            var data = TipoFacturaExtensions.GetList();

            return Ok(new EnumModelResponse<short>()
            {
                Data = data,
            });
        }

        [HttpGet]
        [ProducesResponseType<EnumModelResponse<short>>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
              Summary = "Obtener lista de estados",
              Description = "Devuelve una lista de los estados de una factura."
          )]
        public IActionResult GetStatusList()
        {
            var data = EstadoFacturaExtensions.GetList();

            return Ok(new EnumModelResponse<short>()
            {
                Data = data,
            });
        }

        [HttpGet]
        [ProducesResponseType<BaseObjectResponse<FacturacionControllerGetByIdResponse>>(StatusCodes.Status200OK)]
        [SwaggerOperation(
              Summary = "Obtener una factura por su ID.",
              Description = "Devuelve una factura."
          )]
        public async Task<IActionResult> GetById(Guid id)
        {
            var dbFactura = await _facturaRepository.GetById(id, "FacturaDetalle");

            if (dbFactura == null) {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La factura no se ha encontrado."
                });
            }

            var dbCaja = await _cajaRepository.GetById(dbFactura.CajaId);

            if (dbCaja == null) {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha econtrado la caja vinculada a la factura."
                });
            }

            var tipo = TipoFacturaExtensions.FromValue(dbFactura.TipoId);
            var estado = EstadoFacturaExtensions.FromValue(dbFactura.EstadoId);

            if (tipo == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha econtrado el tipo de factura vinculado a la factura."
                });
            }

            if (estado == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha econtrado el estado de factura vinculado a la factura."
                });
            }

            List<FacturacionControllerGetByIdResponse.FacturaDetalle> facturaDetalles = [];

            foreach (var item in dbFactura.FacturaDetalle)
            {
                facturaDetalles.Add(new() {
                    ProductoId = item.ProductoId,
                    NombreProducto = item.NombreProducto,
                    Cantida = item.Cantidad,
                    ImpuestoPorcentaje = item.ImpuestoPorcentaje,
                    PrecioUnitario = item.PrecioUnitario,
                    Total = item.Total,
                    Subtotal = item.Subtotal,
                    Impuestos = item.Impuestos
                });
            }

            return Ok(new BaseObjectResponse<FacturacionControllerGetByIdResponse>()
            {
                Data = new()
                {
                    Id = dbFactura.Id,
                    TipoID = dbFactura.TipoId,
                    EstadoId = dbFactura.EstadoId,
                    CajaId = dbFactura.CajaId,
                    UsuarioId = dbFactura.UsuarioId,
                    FechaEmision = dbFactura.FechaEmision,
                    Total = dbFactura.Total,
                    Subtotal = dbFactura.Subtotal,
                    Impuestos = dbFactura.Impuestos,
                    Tipo = new()
                    {
                        Id = tipo.Value.GetValue(),
                        Nombre = tipo.Value.GetName(),
                    },
                    Estado = new()
                    {
                        Id = estado.Value.GetValue(),
                        Nombre = estado.Value.GetName(),
                    },
                    Caja = new()
                    {
                        Id = dbCaja.Id,
                        Codigo = dbCaja.Codigo,
                        Nombre = dbCaja.Nombre
                    },
                    FacturaDetalles = facturaDetalles,
                }
            });
        }

        [HttpGet]
        [ProducesResponseType<FacturacionControllerGetListResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
              Summary = "Obtener lista de facturas",
              Description = "Devuelve una lista de todas las facturas con filtros y paginación."
          )]
        public async Task<IActionResult> GetInvoiceList([FromQuery] FacturacionControllerGetListDto param)
        {
            var data = await _facturaRepository.GetPagination(
                page: param.Page,
                pageSize: param.PageSize,
                tipoId: param.TipoId,
                estadoId: param.EstadoId,
                cajaId: param.CajaId,
                usuarioId: param.UsuarioId,
                fechaEmisionInicio: param.FechaEmisionInicio,
                fechaEmisionFinal: param.FechaEmisionFinal
            );

            return Ok(new FacturacionControllerGetListResponse()
            {
                Data = data
            });
        }

        [HttpPost]
        [ProducesResponseType<FacturacionControllerAddInvoiceResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Facturar",
            Description = "Agrega una nueva factura al sistema."
        )]
        public async Task<IActionResult> AddInvoice([FromBody] FacturacionControllerAddInvoiceDto body)
        {
            var authUser = await _authService.GetUserInfoByHeader(Request.Headers.Authorization);

            if (authUser == null)
            {
                return Unauthorized();
            }

            var tipo = TipoFacturaExtensions.FromValue(body.TipoId);

            if (tipo == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "El tipo de factura enviado no existe."
                });
            }

            var dbCaja = await _cajaRepository.GetById(body.CajaId);

            if (dbCaja == null || dbCaja.Activo == false || dbCaja.Eliminado == true)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La caja no se ha encontrado o se encuentra inactiva."
                });
            }

            if (dbCaja.EstadoId != CajaEstado.Abierto.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La caja no se encuentra abierta."
                });
            }

            var dbCajaBitacora = await _cajaBitacoraRepository.GetOneByFilter(x => x.CajaId.Equals(x.CajaId) && x.UsuarioId.Equals(authUser.Id) && x.FechaCierre == null);

            if (dbCajaBitacora == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No existe una bitácora abierta para esta caja o usuario."
                });
            }

            Guid facturaId = Guid.NewGuid();
            List<FacturaDetalle> facturaDetalles = [];

            foreach (var item in body.Productos)
            {
                var producto = _inventarioApiProductoService.GetById(item.ProductoId);

                if (producto == null)
                {
                    return BadRequest(new BadRequestResponse
                    {
                        BadMessage = $"El producto con ID {item.ProductoId} no fue encontrado."
                    });
                }

                var precioBase = producto.PrecioUnitario / (1 + (producto.ImpuestoPorcentaje / 100));
                var impuestoBase = producto.PrecioUnitario - precioBase;
                var impuestos = item.Cantidad * impuestoBase;
                var subTotal = item.Cantidad * precioBase;
                var total = impuestos + subTotal;

                facturaDetalles.Add(new()
                {
                    Id = Guid.NewGuid(),
                    FacturaId = facturaId,
                    ProductoId = producto.ProductoId,
                    NombreProducto = producto.Nombre,
                    Cantidad = item.Cantidad,
                    ImpuestoPorcentaje = producto.ImpuestoPorcentaje,
                    PrecioUnitario = producto.PrecioUnitario,
                    Total = total,
                    Subtotal = subTotal,
                    Impuestos = impuestos
                });
            }

            var totalImpuestos = facturaDetalles.Sum(x => x.Impuestos);
            var totalSubtotal = facturaDetalles.Sum(x => x.Subtotal);
            var totalTotal = facturaDetalles.Sum(x => x.Total);

            Factura factura = new()
            {
                Id = facturaId,
                TipoId = body.TipoId,
                EstadoId = EstadoFactura.Emitida.GetValue(),
                CajaId = body.CajaId,
                UsuarioId = authUser.Id,
                FechaEmision = DateTime.Now,
                Total = totalTotal,
                Subtotal = totalSubtotal,
                Impuestos = totalImpuestos
            };

            await _facturaRepository.Create(factura);
            await _facturaDetalleRepository.CreateMultiple(facturaDetalles);

            return Ok(new
            {
                Id = facturaId
            });
        }
    }
}