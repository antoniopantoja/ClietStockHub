
/// <summary>
/// Envelope padrão de resposta da API.
/// Todas as respostas públicas seguem este contrato:
/// {
///   "cod_retorno": 0 ou 1, // 0: sucesso, 1: erro
///   "mensagem": string|null, // mensagem opcional de erro ou aviso
///   "data": objeto|null // payload da operação
/// }
/// </summary>
using System.Text.Json.Serialization;

namespace ClietStockHub.Api.Common;

public sealed record ApiEnvelope(
    [property: JsonPropertyName("cod_retorno")]
    int CodRetorno,
    [property: JsonPropertyName("mensagem")]
    string? Mensagem,
    [property: JsonPropertyName("data")]
    object? Data)
{
    /// <summary>
    /// Cria envelope de sucesso (cod_retorno = 0).
    /// </summary>
    /// <param name="data">Payload da operação.</param>
    public static ApiEnvelope Success(object? data) => new(0, null, data);

    /// <summary>
    /// Cria envelope de erro (cod_retorno = 1).
    /// </summary>
    /// <param name="message">Mensagem de erro.</param>
    public static ApiEnvelope Error(string message) => new(1, message, null);
}