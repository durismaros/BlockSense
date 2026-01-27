using BlockSense.Backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

//
// --------------------------------
//      CONFIGURATION BINDING
// --------------------------------
//
// Load hard-coded configuration from `appsettings.json` into model classes.
//

builder.Services.ConfigureApplicationOptions(builder.Configuration);

//
// --------------------------------
//   SCOPED SERVICES REGISTRATION
// --------------------------------
//
// Services created once per HTTP request.
//

builder.Services.ConfigureMySqlContext(builder.Configuration);

//
// --------------------------------
//    APPLICATION SERVICE LAYER
// --------------------------------
//
// Interface-to-implementation mapping.
//

builder.Services.ConfigureApplicationServices();

//
// --------------------------------
//  AUTHENTICATION & AUTHORIZATION
// --------------------------------
//
// Configuration of JWT Bearer authentication, validating access tokens using symmetric key signing.
//

builder.Services.ConfigureJwtAuthentication(builder.Configuration);

//
// --------------------------------
//  CONTROLLERS, SWAGGER & LOGGING
// --------------------------------
//
// Controllers for API endpoints and SwaggerUI integration.
//

builder.Services.ConfigureExceptionHandling();
builder.Services.ConfigureForwardedHeaders();

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
