using ClietStockHub.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ClietStockHub.Api.Filters;

public sealed class ApiEnvelopeResultFilter : IAsyncResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.Value is ApiEnvelope)
            {
                return next();
            }

            var statusCode = objectResult.StatusCode ?? context.HttpContext.Response.StatusCode;
            if (statusCode == 0)
            {
                statusCode = 200;
            }

            objectResult.StatusCode = statusCode;
            objectResult.Value = CreateEnvelope(statusCode, objectResult.Value);

            return next();
        }

        if (context.Result is JsonResult jsonResult)
        {
            if (jsonResult.Value is ApiEnvelope)
            {
                return next();
            }

            var statusCode = jsonResult.StatusCode ?? context.HttpContext.Response.StatusCode;
            if (statusCode == 0)
            {
                statusCode = 200;
            }

            jsonResult.StatusCode = statusCode;
            jsonResult.Value = CreateEnvelope(statusCode, jsonResult.Value);
        }

        return next();
    }

    private static ApiEnvelope CreateEnvelope(int statusCode, object? value)
    {
        if (statusCode >= 400)
        {
            var message = value switch
            {
                ProblemDetails details when !string.IsNullOrWhiteSpace(details.Detail) => details.Detail,
                ProblemDetails details when !string.IsNullOrWhiteSpace(details.Title) => details.Title,
                string text when !string.IsNullOrWhiteSpace(text) => text,
                _ => "Erro na requisição."
            };

            return ApiEnvelope.Error(message);
        }

        return ApiEnvelope.Success(value ?? new { });
    }
}