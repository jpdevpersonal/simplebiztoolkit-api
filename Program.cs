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

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? ["https://www.simplebiztoolkit.com", "http://localhost:3000"];

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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=JPLAPTOP;Database=simplebiztoolkit;Integrated Security=SSPI;TrustServerCertificate=True";

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
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("DefaultCors");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
