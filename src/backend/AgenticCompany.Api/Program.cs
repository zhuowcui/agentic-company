using System.Text;
using AgenticCompany.Core.Services;
using AgenticCompany.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT Authentication
const string devFallbackKey = "agentic-company-dev-signing-key-min-32-chars!";
var jwtKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (builder.Environment.IsDevelopment())
        jwtKey = devFallbackKey;
    else
        throw new InvalidOperationException("Jwt:SigningKey is not configured. Set a signing key of at least 32 characters.");
}
if (jwtKey.Length < 32)
    throw new InvalidOperationException($"Jwt:SigningKey must be at least 32 characters (current length: {jwtKey.Length}).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "agentic-company",
            ValidAudience = "agentic-company",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });
builder.Services.AddAuthorization();

// Infrastructure (DbContext, repositories, agent providers)
builder.Services.AddInfrastructure(builder.Configuration);

// Domain services
builder.Services.AddSingleton<PrincipleInheritanceService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
