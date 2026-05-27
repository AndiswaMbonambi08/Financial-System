using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FinancialSystemApp.Data;
using FinancialSystemApp.Services;

namespace FinancialSystemApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Database configuration
            services.AddDbContext<FinancialDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"))
            );

            // Add HttpClient for AI Service
            services.AddHttpClient<IFinancialAIService, FinancialAIService>();

            // Register services
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IFinancialAIService, FinancialAIService>();

            // MVC and API Controllers
            services.AddControllers();

            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Financial System API",
                    Version = "v1",
                    Description = "A comprehensive financial management system with AI-powered insights",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Developer",
                        Email = "dev@financial-system.com"
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            // Enable CORS
            app.UseCors("AllowAll");

            // Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Financial System API v1");
                c.RoutePrefix = string.Empty;
            });

            // Map controllers
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Initialize database with seed data
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<FinancialDbContext>();
                context.Database.EnsureCreated();
            }
        }
    }
}
