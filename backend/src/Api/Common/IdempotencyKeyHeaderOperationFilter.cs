using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ClietStockHub.Api.Common;

/// <summary>
/// Adiciona o header Idempotency-Key como obrigatório no endpoint de criação de pedidos.
/// </summary>
public class IdempotencyKeyHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.RelativePath?.ToLower().Contains("orders") == true &&
            context.ApiDescription.HttpMethod?.ToUpper() == "POST")
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Idempotency-Key",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Chave única para idempotência da requisição. Use um UUID. Exemplo: 123e4567-e89b-12d3-a456-426614174000",
                Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
            });
        }
    }
}