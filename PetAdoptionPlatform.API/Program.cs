using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PetAdoptionPlatform.Application.DTOs.Auth;
using PetAdoptionPlatform.Application.Features.Admin;
using PetAdoptionPlatform.Application.Features.Applications;
using PetAdoptionPlatform.Application.Features.Auth;
using PetAdoptionPlatform.Application.Features.Complaints;
using PetAdoptionPlatform.Application.Features.Donations;
using PetAdoptionPlatform.Application.Features.EligibilityForms;
using PetAdoptionPlatform.Application.Features.Favorites;
using PetAdoptionPlatform.Application.Features.Listings;
using PetAdoptionPlatform.Application.Features.Messages;
using PetAdoptionPlatform.Application.Features.Ratings;
using PetAdoptionPlatform.Application.Features.Stories;
using PetAdoptionPlatform.Application.Interfaces;
using PetAdoptionPlatform.Infrastructure.Services.PaymentGateway;
using PetAdoptionPlatform.Infrastructure.Data;
using PetAdoptionPlatform.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pet Adoption Platform API",
        Version = "v1",
        Description = "API for Pet Adoption Platform"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorFrontend",
        policy =>
        {
            policy.WithOrigins(
                    "https://localhost:7001", 
                    "http://localhost:5001",
                    "https://localhost:7199",
                    "http://localhost:5142",
                    "http://localhost:5147",
                    "http://localhost:5173",
                    "http://localhost:5174"
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var useCloudDb = builder.Configuration.GetValue<bool>("DatabaseOptions:UseCloudDatabase", false);
var cloudConnectionString = builder.Configuration.GetValue<string>("DatabaseOptions:CloudConnectionString") 
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

var connectionString = useCloudDb && !string.IsNullOrEmpty(cloudConnectionString)
    ? cloudConnectionString
    : builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPetListingService, PetListingService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IEligibilityFormService, EligibilityFormService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IPaymentGateway, MockedPaymentGateway>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestDto>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazorFrontend");

var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var seeder = new DbSeeder(context, passwordHasher);
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
