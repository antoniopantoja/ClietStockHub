using ClietStockHub.Api.Common;
using ClietStockHub.Api.Middleware;
using ClietStockHub.Application;
using ClietStockHub.Infrastructure;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

namespace ClietStockHub.Api.Extensions;

public static class ApiCompositionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ClietStockHub API",
                Version = "v1",
                Description = "API de Catálogo e Pedidos com envelope padrão de resposta."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }

    public static IHostBuilder AddStructuredLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext());

        return hostBuilder;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseGlobalExceptionHandling();
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "ClietStockHub API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "ClietStockHub API Docs";
            });
        }

        app.UseAuthorization();

        app.MapControllers();
        app.MapGet("/", () => Results.Ok(ApiEnvelope.Success(new
        {
            service = "ClietStockHub.Api",
            status = "running"
        })));

        return app;
    }
}