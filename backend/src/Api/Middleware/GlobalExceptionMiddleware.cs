using System.Net;
using System.Text.Json;
using ClietStockHub.Api.Common;

namespace ClietStockHub.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            BadHttpRequestException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };

        _logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}, Path: {Path}, StatusCode: {StatusCode}",
            context.TraceIdentifier,
            context.Request.Path,
            (int)statusCode);

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var message = statusCode == HttpStatusCode.InternalServerError && !_environment.IsDevelopment()
            ? "Erro interno no servidor."
            : exception.Message;

        var envelope = ApiEnvelope.Error(message);

        var json = JsonSerializer.Serialize(envelope);
        await context.Response.WriteAsync(json);
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}