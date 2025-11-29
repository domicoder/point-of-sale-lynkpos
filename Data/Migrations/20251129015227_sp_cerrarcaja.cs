using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class sp_cerrarcaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE PROCEDURE sp_cerrarcaja
    @usuario_id UNIQUEIDENTIFIER,
    @caja_id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE
        @estado_id SMALLINT,
        @now DATETIME2(7) = SYSUTCDATETIME(),
        @bitacora_id UNIQUEIDENTIFIER = NULL,
        @estadoCerrado SMALLINT = 1,
        @estadoAbierto SMALLINT = 2;

    IF @usuario_id IS NULL OR @caja_id IS NULL
        THROW 50000, N'usuario_id y caja_id son requeridos.', 1;

    IF NOT EXISTS (SELECT 1 FROM usuarios WHERE id=@usuario_id AND activo=1 AND eliminado=0)
        THROW 50002, N'Usuario no encontrado o inactivo/eliminado.', 1;

    SELECT @estado_id = estado_id FROM cajas WHERE id=@caja_id AND activo=1 AND eliminado=0;

    IF @estado_id IS NULL
        THROW 50003, N'Caja no encontrada o inactiva/eliminada.', 1;

    IF @estado_id <> @estadoAbierto
        THROW 50004, N'La caja no está en estado ""Abierto"".', 1;

    SELECT @bitacora_id = id
    FROM caja_bitacora
    WHERE caja_id=@caja_id
          AND usuario_id=@usuario_id
          AND fecha_cierre IS NULL;

    IF (@bitacora_id IS NULL)
        THROW 50005, N'No existe una vitácora abierta para esta caja o usuario.', 1;

    BEGIN TRANSACTION;
        UPDATE cajas 
        SET estado_id = @estadoCerrado, actualizado_en = @now
        WHERE id = @caja_id;

        UPDATE caja_bitacora 
        SET fecha_cierre = @now
        WHERE id = @bitacora_id;
    COMMIT TRANSACTION;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_cerrarcaja;");
        }
    }
}
