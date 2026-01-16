using BlockSense.Backend.Data;
using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Exceptions.Handlers;
using BlockSense.Backend.Repositories.Implementations;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Implementations;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.Enums.User;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

//
// --------------------------------
//      CONFIGURATION BINDING
// --------------------------------
//
// Load hard-coded configuration from `appsettings.json` into model classes.
//

builder.Services.Configure<JwtTokenConfig>(builder.Configuration.GetSection("JwtTokenConfig"));
builder.Services.Configure<RefreshTokenConfig>(builder.Configuration.GetSection("RefreshTokenConfig"));
builder.Services.Configure<TwoFactorAuthConfig>(builder.Configuration.GetSection("TwoFactorAuthConfig"));

//
// --------------------------------
//   SCOPED SERVICES REGISTRATION
// --------------------------------
//
// Services created once per HTTP request.
//

builder.Services.AddScoped<MySqlConnection>(_ =>
    new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<DatabaseContext>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IInvitationRepository, InvitationRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITwoFactorAuthRepository, TwoFactorAuthRepository>();

//
// --------------------------------
//    APPLICATION SERVICE LAYER
// --------------------------------
//
// Interface-to-implementation mapping.
//

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();

//
// --------------------------------
//  AUTHENTICATION & AUTHORIZATION
// --------------------------------
//
// Configuration of JWT Bearer authentication, validating access tokens using symmetric key signing.
//

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtTokenConfig = builder.Configuration.GetSection("JwtTokenConfig").Get<JwtTokenConfig>() ?? throw new NullReferenceException(nameof(JwtTokenConfig));

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

                throw new InvalidAccessTokenException();
            }
        };
    });

builder.Services
    .AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireClaim(JwtRegisteredClaimNames.Typ, UserType.Administrator.ToString()));
    });

//
// --------------------------------
//  CONTROLLERS, SWAGGER & LOGGING
// --------------------------------
//
// Controllers for API endpoints and SwaggerUI integration.
//

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Development / trusted environment only
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});


builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var validationErrors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var httpContext = context.HttpContext;

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

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
                    ["traceId"] = httpContext.TraceIdentifier,
                }
            };

            return new BadRequestObjectResult(problemDetails);
        };
    });

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

//
// --------------------------------
//  GLOBAL MIDDLEWARE CONFIGURATION
// --------------------------------
//
// Sssential HTTP security headers to all responses.
//

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;

    headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    headers["X-Content-Type-Options"] = "nosniff";
    headers["Referrer-Policy"] = "no-referrer";
    headers["Cache-Control"] = "no-store";
    headers["Pragma"] = "no-cache";

    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseForwardedHeaders();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
