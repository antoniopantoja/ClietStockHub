using System.Text.Json.Serialization;

namespace ClietStockHub.Api.Common;

public sealed record ApiEnvelope(
    [property: JsonPropertyName("cod_retorno")] int CodRetorno,
    [property: JsonPropertyName("mensagem")] string? Mensagem,
    [property: JsonPropertyName("data")] object? Data)
{
    public static ApiEnvelope Success(object? data) => new(0, null, data);

    public static ApiEnvelope Error(string message) => new(1, message, null);
}