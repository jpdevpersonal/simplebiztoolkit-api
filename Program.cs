using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using simplebiztoolkit_api.Data;
using simplebiztoolkit_api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Add Swashbuckle services required by UseSwagger/UseSwaggerUI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? ["https://www.simplebiztoolkit.com", "http://localhost:5117", "http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var jwtKey = builder.Configuration["Auth:JwtKey"] ?? "dev-secret-change";
var jwtIssuer = builder.Configuration["Auth:Issuer"] ?? "simplebiztoolkit-api";
var jwtAudience = builder.Configuration["Auth:Audience"] ?? "simplebiztoolkit-api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Read connection string from configuration (appsettings.json / appsettings.{Environment}.json / environment variables)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' was not found in configuration. Add it to appsettings.json or provide it via environment variables.");
}

builder.Services.AddDbContext<SimpleBizDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IContentStore, EfContentStore>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IRevalidationService, RevalidationService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Serve Swagger UI at application root in Development for convenience.
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = string.Empty; // serve Swagger UI at "/"
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("DefaultCors");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
