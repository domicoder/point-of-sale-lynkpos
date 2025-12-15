using DotNetEnv;
using API;
using Business;
using Business.Authentication;
using Data;
using Data.Repositories;
using Domain.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var storagePublicPath = builder.Configuration["Storage:PublicPath"];

if (storagePublicPath != null)
{
    var projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.ToString() ?? "";
    storagePublicPath = Path.Combine(projectDirectory, storagePublicPath);

    if (!Directory.Exists(storagePublicPath))
    {
        Directory.CreateDirectory(storagePublicPath);
    }

    builder.Configuration["Storage:PublicPath"] = storagePublicPath;
}

// Add services to the container.
builder.Services.AddDataServices(builder.Configuration);
builder.Services.AddBusinessServices(builder.Configuration);
builder.Services.AddScoped<CajaRepository>();

var allowedOriginsFromConfig = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

// Default list in case env/config is missing. Keep it lean to avoid surprises.
var allowedOrigins = allowedOriginsFromConfig.Length > 0
    ? allowedOriginsFromConfig
    : new[]
    {
        "https://lynkpos.app",
        "https://www.lynkpos.app",
        "https://lynkpos-frontend.vercel.app"
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalHostOrigin", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin))
                return false;
            
            try
            {
                var uri = new Uri(origin);
                var host = uri.Host;
                
                // Allow localhost and 127.0.0.1 on any port
                if (host == "localhost" || host == "127.0.0.1")
                    return true;

                // Allow configured origins (exact match)
                if (allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                    return true;
                
                return false;
            }
            catch
            {
                return false;
            }
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    options.CustomSchemaIds(SwaggerHelper.SafeSchemaId);

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Punto de Venta - API",
        Version = "v1"
    });

    // Add JWT Bearer Auth
    options.AddSecurityDefinition(AuthScheme.User.ToSchemeName(), new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token de usuario."
    });

    options.OperationFilter<SwaggerAuthorizeCheckOperationFilter>();
});

var app = builder.Build();

if (storagePublicPath != null)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(storagePublicPath),
        RequestPath = "/api/media"
    });
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.EnablePersistAuthorization();
        opt.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    });
}

// CORS must be enabled before UseHttpsRedirection and UseAuthorization
app.UseCors("AllowLocalHostOrigin");

// Only redirect in non-Docker environments
if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();

app.MapControllers();

// Aplicar migraciones automáticamente al iniciar (solo en producción o cuando AUTO_MIGRATE=true)
var autoMigrate = builder.Configuration.GetValue<bool>("AUTO_MIGRATE", defaultValue: false);
var isProduction = app.Environment.IsProduction() || app.Environment.IsStaging();
var environment = app.Environment.EnvironmentName;

app.Logger.LogInformation("=== Configuración de Migraciones ===");
app.Logger.LogInformation("Environment: {Environment}", environment);
app.Logger.LogInformation("AUTO_MIGRATE: {AutoMigrate}", autoMigrate);
app.Logger.LogInformation("IsProduction: {IsProduction}", isProduction);
app.Logger.LogInformation("Should run migrations: {ShouldRun}", autoMigrate || isProduction);

if (autoMigrate || isProduction)
{
    try
    {
        app.Logger.LogInformation("Iniciando aplicación de migraciones automáticas...");
        
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
        dbContext.MigrateDatabase(app.Logger);
        
        app.Logger.LogInformation("Migraciones aplicadas exitosamente.");
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("base de datos está en un estado que no permite conexiones"))
    {
        // Error específico de Azure SQL Database no disponible (pausada, escalando, etc.)
        app.Logger.LogCritical(ex, 
            "ERROR CRÍTICO: La base de datos de Azure SQL no está disponible. " +
            "Esto puede ocurrir si la base de datos está pausada (serverless) o en proceso de escalado. " +
            "Verifica en Azure Portal el estado de la base de datos.");
        
        if (isProduction)
        {
            app.Logger.LogCritical("FALLANDO la aplicación debido a base de datos no disponible (modo producción)");
            throw;
        }
        else
        {
            app.Logger.LogWarning("Continuando sin aplicar migraciones (modo no-producción). La aplicación puede no funcionar correctamente.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error crítico al aplicar migraciones. La aplicación puede no funcionar correctamente.");
        // En producción, es mejor fallar rápido si las migraciones no se pueden aplicar
        if (isProduction)
        {
            app.Logger.LogCritical("FALLANDO la aplicación debido a error en migraciones (modo producción)");
            throw;
        }
        else
        {
            app.Logger.LogWarning("Continuando sin aplicar migraciones (modo no-producción)");
        }
    }
}
else
{
    app.Logger.LogWarning("Las migraciones NO se aplicarán automáticamente. Environment={Environment}, AUTO_MIGRATE={AutoMigrate}", environment, autoMigrate);
}

app.Run();
