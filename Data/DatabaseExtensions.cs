using System;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace Data
{
    /// <summary>
    /// Extensiones para manejo de base de datos y migraciones
    /// </summary>
    public static class DatabaseExtensions
    {
        private const int MaxRetryAttempts = 5;
        private const int BaseDelaySeconds = 5;

        /// <summary>
        /// Aplica las migraciones pendientes automáticamente con reintentos para errores transitorios.
        /// </summary>
        public static void MigrateDatabase(this AppDbContext dbContext, ILogger logger)
        {
            int attempt = 0;
            while (attempt < MaxRetryAttempts)
            {
                try
                {
                    attempt++;
                    logger.LogInformation("Intento {Attempt} de {MaxAttempts} para conectar a la base de datos...", attempt, MaxRetryAttempts);

                    // Intentar obtener migraciones pendientes
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

                    // Si llegamos aquí, todo salió bien
                    return;
                }
                catch (SqlException sqlEx) when (IsTransientError(sqlEx))
                {
                    if (attempt >= MaxRetryAttempts)
                    {
                        logger.LogError(sqlEx, 
                            "Error al conectar a la base de datos después de {Attempts} intentos. Error: {ErrorNumber} - {Message}",
                            MaxRetryAttempts, sqlEx.Number, sqlEx.Message);
                        throw new InvalidOperationException(
                            $"No se pudo conectar a la base de datos después de {MaxRetryAttempts} intentos. " +
                            $"El error indica que la base de datos está en un estado que no permite conexiones. " +
                            $"Verifica en Azure Portal que la base de datos no esté pausada o en proceso de escalado. " +
                            $"Error: {sqlEx.Number} - {sqlEx.Message}", sqlEx);
                    }

                    var delay = BaseDelaySeconds * attempt; // Backoff exponencial: 5s, 10s, 15s, 20s, 25s
                    logger.LogWarning(sqlEx,
                        "Error transitorio al conectar a la base de datos (Error {ErrorNumber}). " +
                        "Reintentando en {Delay} segundos... (Intento {Attempt}/{MaxAttempts})",
                        sqlEx.Number, delay, attempt, MaxRetryAttempts);
                    
                    Thread.Sleep(TimeSpan.FromSeconds(delay));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error no transitorio al aplicar migraciones automáticamente");
                    throw;
                }
            }
        }

        /// <summary>
        /// Determina si un error de SQL Server es transitorio y debe reintentarse.
        /// </summary>
        private static bool IsTransientError(SqlException sqlException)
        {
            // Error 40925: "Can not connect to the database in its current state"
            // Ocurre cuando la base de datos está pausada (serverless) o en proceso de escalado
            if (sqlException.Number == 40925)
            {
                return true;
            }

            // Otros errores transitorios comunes de Azure SQL
            var transientErrorNumbers = new[]
            {
                2,      // Timeout
                53,     // Network error
                121,    // Semaphore timeout
                233,    // Connection initialization error
                10053,  // Transport-level error
                10054,  // Connection reset
                10060,  // Connection timeout
                40197,  // Service error
                40501,  // Service busy
                40613,  // Database unavailable
                49918,  // Cannot process request
                49919,  // Cannot process create/update request
                49920,  // Cannot process request
            };

            return transientErrorNumbers.Contains(sqlException.Number);
        }
    }
}
