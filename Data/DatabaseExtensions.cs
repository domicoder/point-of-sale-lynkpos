using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    /// <summary>
    /// Extensiones para manejo de base de datos y migraciones
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Aplica las migraciones pendientes automáticamente.
        /// </summary>
        public static void MigrateDatabase(this AppDbContext dbContext, ILogger logger)
        {
            try
            {
                var pendingMigrations = dbContext.Database.GetPendingMigrations();
                
                if (pendingMigrations.Any())
                {
                    logger.LogWarning(
                        "Se encontraron {Count} migraciones pendientes: {Migrations}",
                        pendingMigrations.Count(),
                        string.Join(", ", pendingMigrations));

                    logger.LogInformation("Aplicando migraciones automáticamente...");
                    dbContext.Database.Migrate();
                    
                    logger.LogInformation(
                        "Migraciones aplicadas exitosamente. Total aplicadas: {Count}",
                        pendingMigrations.Count());
                }
                else
                {
                    logger.LogInformation("La base de datos está actualizada. No hay migraciones pendientes.");
                }

                // Verificar conexión
                var canConnect = dbContext.Database.CanConnect();
                if (canConnect)
                {
                    logger.LogInformation("Conexión a la base de datos verificada exitosamente.");
                }
                else
                {
                    logger.LogError("No se pudo conectar a la base de datos.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al aplicar migraciones automáticamente");
                throw;
            }
        }
    }
}
