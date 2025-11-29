using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class arreglar_facturacion_relacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_facturas_detalles_factura_id",
                table: "facturas_detalles");

            migrationBuilder.CreateIndex(
                name: "ix_facturas_detalles_factura_id",
                table: "facturas_detalles",
                column: "factura_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_facturas_detalles_factura_id",
                table: "facturas_detalles");

            migrationBuilder.CreateIndex(
                name: "ix_facturas_detalles_factura_id",
                table: "facturas_detalles",
                column: "factura_id",
                unique: true);
        }
    }
}
