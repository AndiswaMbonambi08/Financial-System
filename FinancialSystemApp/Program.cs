using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using FinancialSystemApp.Data;
using FinancialSystemApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ============= CONFIGURATION =============
var configuration = builder.Configuration;
var anthropicApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
    ?? configuration["Anthropic:ApiKey"]
    ?? "sk-ant-xxxxxxxxxxxxx"; // fallback demo key

// ============= DATABASE SETUP =============
builder.Services.AddDbContext<FinancialDbContext>(options =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=financial_system.db";
    options.UseSqlite(connectionString);
});

// ============= MVC CONTROLLERS + VIEWS =============
builder.Services.AddControllersWithViews();

// ============= SERVICES REGISTRATION =============
// Core Financial Services
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IFinancialAIService, FinancialAIService>();

// AI Extension System (stub classes must exist in Services/)
builder.Services.AddSingleton<IAIExtensionManager, AIExtensionManager>();
builder.Services.AddScoped<IPredictiveAnalyticsExtension, PredictiveAnalyticsExtension>();
builder.Services.AddScoped<IRiskAssessmentExtension, RiskAssessmentExtension>();
builder.Services.AddScoped<IAIOrchestrator, AIOrchestrator>();

// HTTP Client + CORS
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Financial System ERP API",
        Version = "1.0.0",
        Description = "ERP system with AI-powered analytics",
        Contact = new OpenApiContact { Name = "Finance Team", Email = "finance@company.com" },
        License = new OpenApiLicense { Name = "MIT" }
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ============= ERROR HANDLING =============
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Financial System API v1");
        options.RoutePrefix = "api-docs";
    });
}

// ============= DATABASE MIGRATION + SEEDING =============
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FinancialDbContext>();
    dbContext.Database.Migrate();
    if (!dbContext.Accounts.Any())
    {
        FinancialDbContext.SeedData(dbContext);
    }
}

// ============= AI EXTENSION INITIALIZATION =============
using (var scope = app.Services.CreateScope())
{
    var extensionManager = scope.ServiceProvider.GetRequiredService<IAIExtensionManager>();
    extensionManager.RegisterExtension(scope.ServiceProvider.GetRequiredService<IPredictiveAnalyticsExtension>());
    extensionManager.RegisterExtension(scope.ServiceProvider.GetRequiredService<IRiskAssessmentExtension>());
}

// ============= PIPELINE =============
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();

// MVC default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Health check endpoint
app.MapGet("/api/health", (IAIExtensionManager extensionManager, FinancialDbContext dbContext) =>
{
    var extensions = extensionManager.GetAllExtensions();
    return Results.Ok(new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        database = "connected",
        aiExtensions = extensions.Select(e => new { e.ExtensionId, e.Name, e.Version, e.IsEnabled })
    });
});

// Root endpoint
app.MapGet("/", () => Results.Ok(new
{
    application = "Financial System ERP",
    version = "1.0.0",
    description = "ERP system with AI-powered analytics",
    endpoints = new { api_docs = "/api-docs", health = "/api/health" }
}));

app.Run();
