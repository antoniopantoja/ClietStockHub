using ClietStockHub.Api.Common;
using ClietStockHub.Api.Filters;
using ClietStockHub.Api.Middleware;
using ClietStockHub.Application;
using ClietStockHub.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text.Json;

namespace ClietStockHub.Api.Extensions;

public static class ApiCompositionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ApiEnvelopeResultFilter>();
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

                return new BadRequestObjectResult(ApiEnvelope.Error("Requisição inválida."))
                {
                    Value = new ApiEnvelope(
                        1,
                        "Requisição inválida.",
                        new
                        {
                            errors
                        })
                };
            };
        });

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
        app.UseStatusCodePages(async statusCodeContext =>
        {
            var httpContext = statusCodeContext.HttpContext;
            var response = httpContext.Response;
            var requestPath = httpContext.Request.Path;

            if (response.StatusCode < 400)
            {
                return;
            }

            var isApiRoute = requestPath.StartsWithSegments("/api") ||
                             requestPath.StartsWithSegments("/health");

            if (!isApiRoute)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(response.ContentType))
            {
                return;
            }

            response.ContentType = "application/json";

            var message = response.StatusCode switch
            {
                404 => "Recurso não encontrado.",
                405 => "Método HTTP não permitido.",
                401 => "Não autorizado.",
                403 => "Acesso negado.",
                _ => "Erro na requisição."
            };

            var envelope = ApiEnvelope.Error(message);
            await response.WriteAsync(JsonSerializer.Serialize(envelope));
        });

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