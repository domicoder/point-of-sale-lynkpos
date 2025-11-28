
namespace Domain.Models
{
    public class Usuario: BaseEntity<Guid>
    {
        public required short RolId { get; set; }
        public required string Nombre { get; set; }
        public required string UsuarioNombre { get; set; }
        public required string Password { get; set; }
        public bool Activo { get; set; } = false;
        public bool Eliminado { get; set; } = false;

        public Rol Rol { get; set; } = default!;
        public ICollection<CajaVitacora> CajaVitacoras { get; set; } = [];
        public ICollection<Factura> Facturas { get; set; } = [];
    }
}
