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
    public static class ServiceExtensions
    {
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


            return services;
        }

        public static IServiceCollection ConfigureMySqlContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<MySqlConnection>(_ => 
                new MySqlConnection(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<DatabaseContext>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IInvitationRepository, InvitationRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ITwoFactorAuthRepository, TwoFactorAuthRepository>();


            return services;
        }

        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();


            return services;
        }

        public static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtTokenConfig =
                configuration.GetSection("JwtTokenConfig").Get<JwtTokenConfig>() ?? throw new NullReferenceException(nameof(JwtTokenConfig));


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
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtTokenConfig.Issuer,
                        ValidAudience = jwtTokenConfig.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Convert.FromBase64String(jwtTokenConfig.SigningKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                    
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
                options.AddPolicy("AdministratorPolicy", policy => policy.RequireClaim(
                    JwtRegisteredClaimNames.Typ,
                    UserType.Administrator.ToString()));
            });


            return services;
        }

        public static IServiceCollection ConfigureExceptionHandling(this IServiceCollection services)
        {
            services
                .AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var validationErrors = context.ModelState
                        .Where(kvp => kvp.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors
                            .Select(e => e.ErrorMessage)
                            .ToArray());
                        
                        var httpContext = context.HttpContext;
                        
                        var problemDetails = new ProblemDetails
                        {
                            Type = ApiProblemTypes.Generic.BadRequest,
                            Title = "Invalid request",
                            Status = StatusCodes.Status400BadRequest,
                            Detail = "One or more validation errors occurred.",
                            Instance = httpContext.Request.Path,
                            Extensions =
                            {
                                ["errors"] = validationErrors,
                                ["traceId"] = httpContext.TraceIdentifier
                            }
                        };
                        
                        return new BadRequestObjectResult(problemDetails);
                    };
                });
            
            services.AddProblemDetails();
            services.AddExceptionHandler<ApiExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();


            return services;
        }

        public static IServiceCollection ConfigureForwardedHeaders(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;

                // Development / trusted environment only
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });


            return services;
        }
    }
}
