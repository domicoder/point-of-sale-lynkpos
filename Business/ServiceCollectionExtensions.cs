using Business.Authentication;
using Business.Controllers;
using Business.Services.INVENTARIO_API;
using Data.Repositories;
using Domain.Authentication;
using Domain.Environment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Business
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection envToken = configuration.GetSection("Token");
            TokenSetting tokenSetting = envToken.Get<TokenSetting>() ?? throw new(nameof(TokenSetting));

            services.Configure<TokenSetting>(envToken);

            services.AddSingleton(sp =>
            {
                var storageOptions = new StorageSetting();
                configuration.GetSection("Storage").Bind(storageOptions);
                return storageOptions;
            });

            var userKey = Encoding.UTF8.GetBytes(tokenSetting.UserScheme.Key);

            services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = AuthScheme.User.ToSchemeName();
                opt.DefaultChallengeScheme = AuthScheme.User.ToSchemeName();
            })
            .AddJwtBearer(AuthScheme.User.ToSchemeName(), opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = tokenSetting.Issuer,
                    ValidateAudience = true,
                    ValidAudience = tokenSetting.UserScheme.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(userKey),
                    ValidateLifetime = true
                };
            });

            IConfigurationSection envINVENTARIOCredential = configuration.GetSection("INVENTARIO_API");
            InventarioApiSetting inventarioCredential = envINVENTARIOCredential.Get<InventarioApiSetting>() ?? throw new(nameof(InventarioApiSetting));

            services
                .AddOptions<InventarioApiSetting>()
                .Bind(configuration.GetSection("INVENTARIO_API"))
                .Validate(x => !string.IsNullOrWhiteSpace(x.URL), "INVENTARIO_API:URL is required")
                .Validate(x => x.Credential is not null, "INVENTARIO_API:Credential is required")
                .Validate(x => !string.IsNullOrWhiteSpace(x.Credential.Email), "INVENTARIO_API:Credential:Email is required")
                .Validate(x => !string.IsNullOrWhiteSpace(x.Credential.Password), "INVENTARIO_API:Credential:Password is required")
                .ValidateOnStart();

            services.AddHttpClient<INVENTARIOApiService>((serviceProvider, httpClient) =>
            {
                var inventarioSetting = serviceProvider.GetRequiredService<IOptions<InventarioApiSetting>>().Value;
                
                var baseUrl = inventarioSetting.URL?.TrimEnd('/');
                
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    httpClient.BaseAddress = new Uri(baseUrl);
                }

                httpClient.Timeout = TimeSpan.FromSeconds(100);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false });

            services.AddScoped<AuthenticationService>();
            services.AddScoped<AuthService>();
            services.AddScoped<UsuarioRepository>();
            services.AddScoped<CajaRepository>();
            services.AddScoped<CajaBitacoraRepository>();
            services.AddScoped<FacturaRepository>();
            services.AddScoped<FacturaDetalleRepository>();
            services.AddScoped<InventarioApiProductoService>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();

            return services;
        }
    }
}
