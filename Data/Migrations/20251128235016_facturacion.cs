using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class facturacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estados_facturas",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    actualizado_en = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estados_facturas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_facturas",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    actualizado_en = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipos_facturas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "facturas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tipo_id = table.Column<short>(type: "smallint", nullable: false),
                    estado_id = table.Column<short>(type: "smallint", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    caja_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fecha_emision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    impuestos = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    actualizado_en = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_facturas", x => x.id);
                    table.ForeignKey(
                        name: "facturas_fk_caja_id",
                        column: x => x.caja_id,
                        principalTable: "cajas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "facturas_fk_estado_id",
                        column: x => x.estado_id,
                        principalTable: "estados_facturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "facturas_fk_tipo_id",
                        column: x => x.tipo_id,
                        principalTable: "tipos_facturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "facturas_fk_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "facturas_detalles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    factura_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    producto_id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    nombre_producto = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    impuesto_porcentaje = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    precio_unitario = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    impuestos = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    actualizado_en = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_facturas_detalles", x => x.id);
                    table.ForeignKey(
                        name: "facturas_detalles_fk_factura_id",
                        column: x => x.factura_id,
                        principalTable: "facturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "estados_facturas",
                columns: new[] { "id", "actualizado_en", "nombre" },
                values: new object[,]
                {
                    { (short)1, null, "EMITIDA" },
                    { (short)2, null, "CANCELADA" }
                });

            migrationBuilder.InsertData(
                table: "tipos_facturas",
                columns: new[] { "id", "actualizado_en", "nombre" },
                values: new object[,]
                {
                    { (short)1, null, "DEBITO" },
                    { (short)2, null, "CREDITO" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_facturas_caja_id",
                table: "facturas",
                column: "caja_id");

            migrationBuilder.CreateIndex(
                name: "ix_facturas_estado_id",
                table: "facturas",
                column: "estado_id");

            migrationBuilder.CreateIndex(
                name: "ix_facturas_tipo_id",
                table: "facturas",
                column: "tipo_id");

            migrationBuilder.CreateIndex(
                name: "ix_facturas_usuario_id",
                table: "facturas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_facturas_detalles_factura_id",
                table: "facturas_detalles",
                column: "factura_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "facturas_detalles");

            migrationBuilder.DropTable(
                name: "facturas");

            migrationBuilder.DropTable(
                name: "estados_facturas");

            migrationBuilder.DropTable(
                name: "tipos_facturas");
        }
    }
}
