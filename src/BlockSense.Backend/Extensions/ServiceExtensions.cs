using BlockSense.Backend.Data;
using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Exceptions.Handlers;
using BlockSense.Backend.Repositories.Implementations;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Implementations;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;

namespace BlockSense.Backend.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/> that register and configure
    /// application services in logical groups.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Binds strongly-typed configuration option classes from <c>appsettings.json</c>
        /// and validates them against data annotations at startup.
        /// </summary>
        /// <param name="services">The application's service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public static IServiceCollection ConfigureApplicationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptions<JwtTokenConfig>()
                .Bind(configuration.GetSection("JwtTokenConfig"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services
                .AddOptions<RefreshTokenConfig>()
                .Bind(configuration.GetSection("RefreshTokenConfig"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services
                .AddOptions<TwoFactorAuthConfig>()
                .Bind(configuration.GetSection("TwoFactorAuthConfig"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services
                .AddOptions<CryptoConfig>()
                .Bind(configuration.GetSection("CryptoConfig"))
                .ValidateDataAnnotations()
                .ValidateOnStart();


            return services;
        }

        /// <summary>
        /// Registers the MySQL database connection, database context, and all repository implementations.
        /// All registrations are scoped to the lifetime of a single HTTP request.
        /// </summary>
        /// <param name="services">The application's service collection.</param>
        /// <param name="configuration">The application configuration used to resolve the connection string.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public static IServiceCollection ConfigureMySqlContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<MySqlConnection>(_ =>
                new MySqlConnection(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<DatabaseContext>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IInvitationRepository, InvitationRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ITotpCredentialRepository, TotpCredentialRepository>();
            services.AddScoped<IActivityLogRepository, ActivityLogRepository>();


            return services;
        }

        /// <summary>
        /// Registers all application-layer services and their interface-to-implementation mappings.
        /// </summary>
        /// <param name="services">The application's service collection.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services)
        {
            services.AddHttpClient<CryptoApiClient>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
            services.AddScoped<IActivityLogService, ActivityLogService>();

            services.AddKeyedScoped<ICryptoService, BitcoinService>("bitcoin");
            services.AddKeyedScoped<ICryptoService, EthereumService>("ethereum");

            services.AddSingleton<IExchangeRateService, ExchangeRateService>();


            return services;
        }

        /// <summary>
        /// Configures JWT Bearer authentication with symmetric key validation and registers
        /// role-based authorization policies.
        /// </summary>
        /// <param name="services">The application's service collection.</param>
        /// <param name="configuration">The application configuration used to resolve JWT settings.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtConfig = configuration.GetSection("JwtTokenConfig").Get<JwtTokenConfig>()
                ?? throw new InvalidOperationException("JwtTokenConfig section is missing or invalid.");

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = BuildTokenValidationParameters(jwtConfig);
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            throw new AuthenticationRequiredException();
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdministratorPolicy", policy =>
                    policy.RequireClaim(
                        JwtRegisteredClaimNames.Typ,
                        UserRole.Administrator.ToString(),
                        UserRole.Founder.ToString()));
            });


            return services;
        }

        /// <summary>
        /// Configures global exception handling, model validation error responses, and
        /// registers MVC controllers with custom API behavior options.
        /// </summary>
        /// <param name="services">The application's service collection.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public static IServiceCollection ConfigureExceptionHandling(this IServiceCollection services)
        {
            services
                .AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = BuildValidationErrorResponse;
                });

            services.AddProblemDetails();
            services.AddExceptionHandler<ApiExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();


            return services;
        }

        /// <summary>
        /// Configures forwarded header processing to support deployments behind a reverse proxy.
        /// Trusts all networks and proxies — intended for controlled or development environments only.
        /// </summary>
        /// <param name="services">The application's service collection.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public static IServiceCollection ConfigureForwardedHeaders(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;

                // Development / trusted environment only.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });


            return services;
        }

        /// <summary>
        /// Builds a <see cref="TokenValidationParameters"/> instance from the provided JWT configuration.
        /// </summary>
        /// <param name="jwtConfig">The JWT configuration containing issuer, audience, and signing key.</param>
        /// <returns>A fully configured <see cref="TokenValidationParameters"/> instance.</returns>
        private static TokenValidationParameters BuildTokenValidationParameters(JwtTokenConfig jwtConfig)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Convert.FromBase64String(jwtConfig.SigningKey)),
                ClockSkew = TimeSpan.Zero
            };
        }

        /// <summary>
        /// Builds a standardized <see cref="ProblemDetails"/> response for model validation failures.
        /// </summary>
        /// <param name="context">The action context containing the invalid model state.</param>
        /// <returns>A <see cref="BadRequestObjectResult"/> containing structured validation error details.</returns>
        private static IActionResult BuildValidationErrorResponse(ActionContext context)
        {
            var validationErrors = context.ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors
                        .Select(e => e.ErrorMessage)
                        .ToArray());

            var problemDetails = new ProblemDetails
            {
                Type = StandardizedCodes.Generic.BadRequest,
                Title = "Invalid request",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path,
                Extensions =
                {
                    ["errors"] = validationErrors,
                    ["traceId"] = context.HttpContext.TraceIdentifier
                }
            };

            return new BadRequestObjectResult(problemDetails);
        }
    }
}