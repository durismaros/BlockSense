using BlockSense.Backend.Data;
using BlockSense.Backend.Data.Configurations;
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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
