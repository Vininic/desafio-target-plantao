using System.Text.Json;
using TargetPlantao.Core.Commissions;

namespace TargetPlantao.Tests.Core.Commissions;

public class SalesJsonReaderTests
{
    [Fact]
    public void Reads_every_sale_from_the_challenge_file()
    {
        using var json = Open("vendas.json");

        var sales = SalesJsonReader.Read(json);

        Assert.Equal(36, sales.Count);
        Assert.Equal(new Sale("João Silva", 1200.50m), sales[0]);
        Assert.Equal(new Sale("Ana Lima", 315.40m), sales[^1]);
    }

    [Theory]
    [InlineData("{ }")]
    [InlineData("{ \"vendas\": [ { \"valor\": 10 } ] }")]
    [InlineData("{ \"vendas\": [ { \"vendedor\": \"Ana\" } ] }")]
    public void Missing_required_fields_are_reported(string json)
    {
        using var stream = FromString(json);

        Assert.ThrowsAny<JsonException>(() => SalesJsonReader.Read(stream));
    }
}
