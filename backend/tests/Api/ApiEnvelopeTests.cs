using System.Text.Json;
using ClietStockHub.Api.Common;
using Xunit;

namespace ClietStockHub.Tests.Api;

public class ApiEnvelopeTests
{
    [Fact]
    public void Success_ShouldSetCodRetornoZero_AndData()
    {
        var data = new { foo = "bar" };
        var envelope = ApiEnvelope.Success(data);
        Assert.Equal(0, envelope.CodRetorno);
        Assert.Null(envelope.Mensagem);
        Assert.Equal(data, envelope.Data);
    }

    [Fact]
    public void Error_ShouldSetCodRetornoOne_AndMensagem()
    {
        var envelope = ApiEnvelope.Error("erro");
        Assert.Equal(1, envelope.CodRetorno);
        Assert.Equal("erro", envelope.Mensagem);
        Assert.Null(envelope.Data);
    }

    [Fact]
    public void ShouldSerializeAndDeserializeEnvelope()
    {
        var original = ApiEnvelope.Success(new { x = 1 });
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ApiEnvelope>(json);
        Assert.Equal(original.CodRetorno, deserialized!.CodRetorno);
        Assert.Equal(original.Mensagem, deserialized.Mensagem);
        Assert.NotNull(deserialized.Data);
    }
}
