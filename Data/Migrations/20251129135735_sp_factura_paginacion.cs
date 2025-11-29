using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class sp_factura_paginacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE PROCEDURE sp_factura_paginacion (
    @page INTEGER = 1,
    @page_size SMALLINT = 20,
    @tipo_id SMALLINT = NULL,
    @estado_id SMALLINT = NULL,
    @caja_id UNIQUEIDENTIFIER = NULL,
    @usuario_id UNIQUEIDENTIFIER = NULL,
    @fecha_emision_inicio DATETIME = NULL,
    @fecha_emision_final DATETIME = NULL
) AS
BEGIN
	SET NOCOUNT ON;

    SET @page = CASE WHEN @page < 1 THEN 1 ELSE @page END;

    SET @page_size = CASE WHEN @page_size < 1 THEN 20
                          WHEN @page_size > 100 THEN 100 ELSE @page_size END;

    DECLARE @offset INT = (@page - 1) * @page_size;

	WITH cte AS (
        SELECT id
        FROM facturas
        WHERE (@tipo_id IS NULL OR tipo_id LIKE @tipo_id)
              AND (@estado_id IS NULL OR estado_id LIKE @estado_id)
              AND (@caja_id IS NULL OR caja_id LIKE @caja_id)
              AND (@usuario_id IS NULL OR usuario_id LIKE @usuario_id)
              AND (@fecha_emision_inicio IS NULL OR fecha_emision >= @fecha_emision_inicio)
              AND (@fecha_emision_final IS NULL OR fecha_emision <= @fecha_emision_final)
        ORDER BY fecha_emision DESC
        OFFSET @offset ROWS FETCH NEXT @page_size ROWS ONLY
    )
    SELECT b.id, b.tipo_id,
        b.estado_id, b.caja_id,
        b.usuario_id, b.fecha_emision,
        b.total, b.subtotal, b.impuestos,
        c.nombre as tipo_nombre,
        d.nombre as estado_nombre,
        e.codigo as caja_codigo, e.nombre as caja_nombre,
        f.nombre as usuario, f.usuario_nombre
    FROM cte as a INNER JOIN
         facturas as b ON b.id = a.id INNER JOIN
         tipos_facturas as c ON c.id = b.tipo_id INNER JOIN
         estados_facturas as d ON d.id = b.estado_id INNER JOIN
         cajas as e ON e.id = b.caja_id INNER JOIN
         usuarios as f ON f.id = b.usuario_id
    ORDER BY b.fecha_emision ASC
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_factura_paginacion;");
        }
    }
}
