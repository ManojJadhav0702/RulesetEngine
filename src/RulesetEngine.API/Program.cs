using Microsoft.EntityFrameworkCore;
using RulesetEngine.Application;
using RulesetEngine.Data;
using RulesetEngine.Data.Repositories;
using RulesetEngine.Domain.Evaluators;
using RulesetEngine.Domain.Strategies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep original casing
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Database Configuration
builder.Services.AddDbContext<RulesetEngineDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// Memory Cache
builder.Services.AddMemoryCache();

// Repositories
builder.Services.AddScoped<IRulesetRepository, RulesetRepository>();
builder.Services.AddScoped<IEvaluationLogRepository, EvaluationLogRepository>();

// Domain Services
builder.Services.AddSingleton<OperatorStrategyFactory>();
builder.Services.AddScoped<ConditionEvaluator>();
builder.Services.AddScoped<RulesetEvaluator>();

// Application Services
builder.Services.AddScoped<IOrderEvaluationService, OrderEvaluationService>();
builder.Services.AddScoped<IRulesetConfigurationService, RulesetConfigurationService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Ruleset Evaluation Engine API",
        Version = "v1",
        Description = "API for evaluating orders and determining production plants based on configurable rulesets"
    });
});

// CORS (configure as needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ruleset Evaluation Engine API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Database initialization\
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<RulesetEngineDbContext>();

            // Ensure database is created and migrated
            context.Database.Migrate();

            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Database initialized successfully");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while initializing the database");
        }
    }
}

app.Run();

public partial class Program { }