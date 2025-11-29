using Domain.INVENTARIO_API;

namespace Business.Services.INVENTARIO_API
{
    public class InventarioApiProductoService
    {
        public GetProductoByIdResponse? GetById (string id)
        {
            return _productos.Find(x => x.ProductoId == id);
        }

        public ICollection<GetProductoByIdResponse> GetProductoListResponse() => _productos;

        private readonly List<GetProductoByIdResponse> _productos = [
            new()
            {
                ProductoId = "001",
                Nombre = "Café de Jarabacoa",
                PrecioUnitario = (decimal)350.00,
                ImpuestoPorcentaje = 18m,
                Stock = 120
            },
            new()
            {
                ProductoId = "002",
                Nombre = "Chocolate Embajador",
                PrecioUnitario = (decimal)95.00,
                ImpuestoPorcentaje = 18m,
                Stock = 250
            },
            new()
            {
                ProductoId = "003",
                Nombre = "Ron Barceló Imperial",
                PrecioUnitario = (decimal)1450.00,
                ImpuestoPorcentaje = 18m,
                Stock = 80
            },
            new()
            {
                ProductoId = "004",
                Nombre = "Habichuelas Rojas Molidas",
                PrecioUnitario = (decimal)85.00,
                ImpuestoPorcentaje = 18m,
                Stock = 300
            },
            new()
            {
                ProductoId = "005",
                Nombre = "Aceite Crisol",
                PrecioUnitario = (decimal)160.00,
                ImpuestoPorcentaje = 18m,
                Stock = 200
            },
            new()
            {
                ProductoId = "006",
                Nombre = "Arroz Selecto Pimco",
                PrecioUnitario = (decimal)45.00,
                ImpuestoPorcentaje = 0m,
                Stock = 500
            },
            new()
            {
                ProductoId = "007",
                Nombre = "Salami Induveca",
                PrecioUnitario = (decimal)220.00,
                ImpuestoPorcentaje = 18m,
                Stock = 150
            },
            new()
            {
                ProductoId = "008",
                Nombre = "Queso Geo Cremoso",
                PrecioUnitario = (decimal)280.00,
                ImpuestoPorcentaje = 18m,
                Stock = 90
            },
            new()
            {
                ProductoId = "009",
                Nombre = "Galletas Noel",
                PrecioUnitario = (decimal)60.00,
                ImpuestoPorcentaje = 18m,
                Stock = 400
            },
            new()
            {
                ProductoId = "010",
                Nombre = "Refresco Merengue Rojo",
                PrecioUnitario = (decimal)30.00,
                ImpuestoPorcentaje = 18m,
                Stock = 600
            },
            new()
            {
                ProductoId = "011",
                Nombre = "Mantequilla Rica",
                PrecioUnitario = (decimal)75.00,
                ImpuestoPorcentaje = 18m,
                Stock = 180
            },
            new()
            {
                ProductoId = "012",
                Nombre = "Leche Rica Entera",
                PrecioUnitario = (decimal)70.00,
                ImpuestoPorcentaje = 18m,
                Stock = 220
            },
            new()
            {
                ProductoId = "013",
                Nombre = "Vino JP Chenet (Dominicano)",
                PrecioUnitario = (decimal)395.00,
                ImpuestoPorcentaje = 18m,
                Stock = 60
            },
            new()
            {
                ProductoId = "014",
                Nombre = "Miel de Abeja de Samaná",
                PrecioUnitario = (decimal)280.00,
                ImpuestoPorcentaje = 0m,
                Stock = 75
            },
            new()
            {
                ProductoId = "015",
                Nombre = "Tabaco Dominicano Premium",
                PrecioUnitario = (decimal)650.00,
                ImpuestoPorcentaje = 18m,
                Stock = 40
            }
        ];
    }
}
