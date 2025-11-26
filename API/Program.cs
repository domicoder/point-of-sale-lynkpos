using DotNetEnv;
using API;
using Business;
using Business.Authentication;
using Data;
using Data.Repositories;
using Domain.Authentication;
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

                var domainFrontend = "https://lynkpos.app";
                var vercelDomainFrontend = "https://lynkpos-frontend-o303fknwe-domicoder-team.vercel.app";

                // Allow specific production origin
                if (origin == domainFrontend || origin == vercelDomainFrontend)
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


try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error applying migrations at startup. API will continue running.");
    // IMPORTANTE: no volver a lanzar la excepción
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
