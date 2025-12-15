using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    /// <summary>
    /// Controlador para verificar el estado de la base de datos y migraciones
    /// Útil para debugging y monitoreo en producción
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(AppDbContext dbContext, ILogger<DatabaseController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene información sobre el estado de las migraciones de la base de datos
        /// </summary>
        [HttpGet("migrations/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Estado de migraciones",
            Description = "Devuelve información sobre las migraciones aplicadas y pendientes. Requiere autenticación."
        )]
        public async Task<IActionResult> GetMigrationsStatus()
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                    {
                        CanConnect = false,
                        Message = "No se puede conectar a la base de datos",
                        AppliedMigrations = Array.Empty<string>(),
                        PendingMigrations = Array.Empty<string>()
                    });
                }

                var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync();
                var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
                var allMigrations = _dbContext.Database.GetMigrations();

                return Ok(new
                {
                    CanConnect = true,
                    AppliedMigrations = appliedMigrations.ToArray(),
                    PendingMigrations = pendingMigrations.ToArray(),
                    AllMigrations = allMigrations.ToArray(),
                    IsUpToDate = !pendingMigrations.Any(),
                    AppliedCount = appliedMigrations.Count(),
                    PendingCount = pendingMigrations.Count(),
                    TotalCount = allMigrations.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado de migraciones");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Error = "Error al obtener información de migraciones",
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene información sobre el estado de las migraciones (público, sin autenticación)
        /// Útil para debugging en producción
        /// </summary>
        [AllowAnonymous]
        [HttpGet("migrations/status/public")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Estado de migraciones (público)",
            Description = "Devuelve información sobre las migraciones aplicadas y pendientes. No requiere autenticación."
        )]
        public async Task<IActionResult> GetMigrationsStatusPublic()
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                    {
                        CanConnect = false,
                        Message = "No se puede conectar a la base de datos",
                        AppliedMigrations = Array.Empty<string>(),
                        PendingMigrations = Array.Empty<string>()
                    });
                }

                var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync();
                var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
                var allMigrations = _dbContext.Database.GetMigrations();

                return Ok(new
                {
                    CanConnect = true,
                    AppliedMigrations = appliedMigrations.ToArray(),
                    PendingMigrations = pendingMigrations.ToArray(),
                    AllMigrations = allMigrations.ToArray(),
                    IsUpToDate = !pendingMigrations.Any(),
                    AppliedCount = appliedMigrations.Count(),
                    PendingCount = pendingMigrations.Count(),
                    TotalCount = allMigrations.Count(),
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado de migraciones");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Error = "Error al obtener información de migraciones",
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Verifica la conexión a la base de datos
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [SwaggerOperation(
            Summary = "Health check de base de datos",
            Description = "Verifica si la aplicación puede conectarse a la base de datos."
        )]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    return Ok(new
                    {
                        Status = "Healthy",
                        Database = "Connected",
                        Timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                    {
                        Status = "Unhealthy",
                        Database = "Disconnected",
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en health check de base de datos");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Status = "Unhealthy",
                    Database = "Error",
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
