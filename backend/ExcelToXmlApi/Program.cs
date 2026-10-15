using ExcelToXmlApi.Data;
using ExcelToXmlApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Ensure data directory exists
var dataDir = Path.Combine(builder.Environment.ContentRootPath, "data");
if (!Directory.Exists(dataDir))
{
    Directory.CreateDirectory(dataDir);
}

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database (SQL Server / SQLite fallback)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=sqlserver,1433;Database=SitemenggarisDb;User Id=sa;Password=Simenggaris2026!;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False";

    if (connectionString.Contains("Server=") || connectionString.Contains("Data Source=") && connectionString.Contains(";"))
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// Configure DI
builder.Services.AddScoped<ISotProductionConverter, SotProductionConverter>();
builder.Services.AddScoped<ISotLpumaConverter, SotLpumaConverter>();
builder.Services.AddSingleton<TemplateGeneratorService>();

// CORS configuration for React Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto-migrate or ensure database is created with retry loop for container readiness
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    int maxRetries = 15;
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Attempting to connect to database (Attempt {Attempt}/{MaxRetries})...", attempt, maxRetries);
            db.Database.EnsureCreated();
            logger.LogInformation("Database connection successful and schema verified.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Database not ready yet (Attempt {Attempt}/{MaxRetries}): {Message}", attempt, maxRetries, ex.Message);
            if (attempt == maxRetries)
            {
                logger.LogError(ex, "Failed to connect to database after {MaxRetries} attempts.", maxRetries);
                throw;
            }
            Thread.Sleep(3000);
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
