using BlockSense.Backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureApplicationOptions(builder.Configuration);
builder.Services.ConfigureMySqlContext(builder.Configuration);
builder.Services.ConfigureApplicationServices();
builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.ConfigureExceptionHandling();
builder.Services.ConfigureForwardedHeaders();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSecurityHeaders();

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