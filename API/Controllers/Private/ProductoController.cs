using Business.Services.INVENTARIO_API;
using Domain.API;
using Domain.Controller.Private.Producto;
using Domain.INVENTARIO_API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductoController(InventarioApiProductoService _inventarioApiProductoService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<BaseObjectResponse<ProductoControllerGetByIdResponse>>(StatusCodes.Status200OK)]
        [SwaggerOperation(
              Summary = "Obtener una producto por su ID.",
              Description = "Devuelve una producto desde inventario."
          )]
        public IActionResult GetProductById(string id)
        {
            var data = _inventarioApiProductoService.GetById(id);

            if (data == null)
            {
                return Ok(new BaseObjectResponse<object>
                {
                    Ok = true,
                    Data = null
                });
            }

            return Ok(new BaseObjectResponse<ProductoControllerGetByIdResponse>
            {
                Ok = true,
                Data = new()
                {
                    ProductoId = data.ProductoId,
                    Nombre = data.Nombre,
                    ImpuestoPorcentaje = data.ImpuestoPorcentaje,
                    PrecioUnitario = data.PrecioUnitario,
                    Stock = data.Stock
                }
            });
        }

        [HttpGet]
        [ProducesResponseType<BaseObjectResponse<ICollection<GetProductoByIdResponse>>>(StatusCodes.Status200OK)]
        [SwaggerOperation(
              Summary = "Obtener una lista de productos.",
              Description = "Devuelve una lista de los producto desde inventario."
          )]
        public IActionResult GetProductList()
        {
            var data = _inventarioApiProductoService.GetProductoListResponse();

            return Ok(new BaseObjectResponse<ICollection<GetProductoByIdResponse>>
            {
                Ok = true,
                Data = data
            });
        }
    }
}