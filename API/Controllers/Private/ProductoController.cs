using Domain.API;
using Domain.Controller.Private.Producto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<BaseObjectResponse<ProductoControllerGetByIdResponse>>(StatusCodes.Status200OK)]
        [SwaggerOperation(
              Summary = "Obtener una producto por su ID.",
              Description = "Devuelve una producto desde inventario."
          )]
        public IActionResult GetProductById(string id)
        {
            var data = _productos.Find(x => x.ProductoId.Equals(id));

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

        private readonly List<Domain.INVENTARIO_API.GetProductoByIdResponse> _productos = [
            new()
            {
                ProductoId = "PD-001",
                Nombre = "Café de Jarabacoa",
                PrecioUnitario = "350.00",
                ImpuestoPorcentaje = 18m,
                Stock = 120
            },
            new()
            {
                ProductoId = "PD-002",
                Nombre = "Chocolate Embajador",
                PrecioUnitario = "95.00",
                ImpuestoPorcentaje = 18m,
                Stock = 250
            },
            new()
            {
                ProductoId = "PD-003",
                Nombre = "Ron Barceló Imperial",
                PrecioUnitario = "1450.00",
                ImpuestoPorcentaje = 18m,
                Stock = 80
            },
            new()
            {
                ProductoId = "PD-004",
                Nombre = "Habichuelas Rojas Molidas",
                PrecioUnitario = "85.00",
                ImpuestoPorcentaje = 18m,
                Stock = 300
            },
            new()
            {
                ProductoId = "PD-005",
                Nombre = "Aceite Crisol",
                PrecioUnitario = "160.00",
                ImpuestoPorcentaje = 18m,
                Stock = 200
            },
            new()
            {
                ProductoId = "PD-006",
                Nombre = "Arroz Selecto Pimco",
                PrecioUnitario = "45.00",
                ImpuestoPorcentaje = 0m,
                Stock = 500
            },
            new()
            {
                ProductoId = "PD-007",
                Nombre = "Salami Induveca",
                PrecioUnitario = "220.00",
                ImpuestoPorcentaje = 18m,
                Stock = 150
            },
            new()
            {
                ProductoId = "PD-008",
                Nombre = "Queso Geo Cremoso",
                PrecioUnitario = "280.00",
                ImpuestoPorcentaje = 18m,
                Stock = 90
            },
            new()
            {
                ProductoId = "PD-009",
                Nombre = "Galletas Noel",
                PrecioUnitario = "60.00",
                ImpuestoPorcentaje = 18m,
                Stock = 400
            },
            new()
            {
                ProductoId = "PD-010",
                Nombre = "Refresco Merengue Rojo",
                PrecioUnitario = "30.00",
                ImpuestoPorcentaje = 18m,
                Stock = 600
            },
            new()
            {
                ProductoId = "PD-011",
                Nombre = "Mantequilla Rica",
                PrecioUnitario = "75.00",
                ImpuestoPorcentaje = 18m,
                Stock = 180
            },
            new()
            {
                ProductoId = "PD-012",
                Nombre = "Leche Rica Entera",
                PrecioUnitario = "70.00",
                ImpuestoPorcentaje = 18m,
                Stock = 220
            },
            new()
            {
                ProductoId = "PD-013",
                Nombre = "Vino JP Chenet (Dominicano)",
                PrecioUnitario = "395.00",
                ImpuestoPorcentaje = 18m,
                Stock = 60
            },
            new()
            {
                ProductoId = "PD-014",
                Nombre = "Miel de Abeja de Samaná",
                PrecioUnitario = "280.00",
                ImpuestoPorcentaje = 0m,
                Stock = 75
            },
            new()
            {
                ProductoId = "PD-015",
                Nombre = "Tabaco Dominicano Premium",
                PrecioUnitario = "650.00",
                ImpuestoPorcentaje = 18m,
                Stock = 40
            }
        ];
    }
}