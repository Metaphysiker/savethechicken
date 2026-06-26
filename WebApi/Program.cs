using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using QuestPDF.Infrastructure;
using System.Text;
using WebApi.Factories.FactoriesImpl;
using WebApi.Services;
using WebApi.Services.ServicesImpl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddLogging();
builder.Services.AddScoped<AutoMapperService>();
builder.Services.AddScoped<GenericModelServiceFactory>();
builder.Services.AddScoped<ModelSearchFactory>();
builder.Services.AddScoped<BlackListDetectorService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddScoped<CsvImportService>();
builder.Services.AddDbContext<DatabaseContext>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.MaxDepth = 32; // Limit depth to prevent stack overflow
});

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
});

builder.Services
    .AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var key = Environment.GetEnvironmentVariable("SIGNING_KEY") ?? "THIS_IS_A_DEFAULT_SIGNING_KEY_1234567890_ABCDEFGH";

        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "apiWithAuthBackend",
            ValidAudience = "apiWithAuthBackend",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)
            ),
        };
    });
builder.Services.AddAuthorization();


builder.Services.AddScoped<TokenService, TokenService>();

builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DatabaseContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
});


var awsOptions = builder.Configuration.GetAWSOptions();

var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
if (!isDocker)
{
    var awsAccessKeyId = builder.Configuration["AWS:AWS_ACCESS_KEY_ID"];
    var awsSecretAccessKey = builder.Configuration["AWS:AWS_SECRET_ACCESS_KEY"];
    var region = builder.Configuration["AWS:AWS_REGION"] ?? "eu-central-1";

    awsOptions = new AWSOptions
    {
        Credentials = new Amazon.Runtime.BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey),
        Region = Amazon.RegionEndpoint.GetBySystemName(region)
    };

    builder.Services.AddDefaultAWSOptions(awsOptions);
} else
{
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
    Console.WriteLine("Running in Docker - using AWS options from environment");
}


builder.Services.AddAWSService<IAmazonS3>();

var app = builder.Build();

// Automatically create database and apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Ensure the database exists and apply migrations
        logger.LogInformation("Ensuring database exists and applying migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating/migrating the database");
        throw;
    }
}

// Enable Swagger in production
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(builder =>
{
    builder
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
