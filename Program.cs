using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WeddingHallAPI.Data;
using WeddingHallAPI.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? Environment.GetEnvironmentVariable("CONNECTION_STRING");

builder.Services.AddDbContext<WeddingHallDbContext>(options =>
    options.UseNpgsql(connectionString));




builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{

    // JWT Security Definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    // Global xavfsizlik talablari
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddEndpointsApiExplorer();

// JWT Authentication
var authSettingsSection = builder.Configuration.GetSection("AuthSettings");
AuthSettings? authSettings = authSettingsSection.Exists() && authSettingsSection.Get<AuthSettings>() != null
    ? authSettingsSection.Get<AuthSettings>()
    : System.Text.Json.JsonSerializer.Deserialize<AuthSettings>(
        Environment.GetEnvironmentVariable("AuthSettings") ?? "Unknown!AUTH*settings$"
    );

builder.Services.Configure<AuthSettings>(options =>
{
    if (authSettings != null)
    {
        options.Secret = authSettings.Secret;
    }
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new
        SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.Secret)),
        ValidateIssuer = false,
        ValidateAudience = false
    };

});


builder.Services.AddScoped<IEmailOtpService, EmailOtpService>();
builder.Services.AddScoped<IDataService, DataService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});



var app = builder.Build();
app.UseCors("AllowAll");
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseStaticFiles(); 
app.Run();
