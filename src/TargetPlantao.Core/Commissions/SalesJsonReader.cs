using System.Text.Json.Serialization;
using TargetPlantao.Core.Common;

namespace TargetPlantao.Core.Commissions;

public static class SalesJsonReader
{
    public static IReadOnlyList<Sale> Read(Stream json) =>
        [.. JsonData.Deserialize<SalesDocument>(json).Sales.Select(dto => new Sale(dto.Seller, dto.Amount))];

    private sealed record SalesDocument(
        [property: JsonRequired, JsonPropertyName("vendas")] IReadOnlyList<SaleDto> Sales);

    private sealed record SaleDto(
        [property: JsonRequired, JsonPropertyName("vendedor")] string Seller,
        [property: JsonRequired, JsonPropertyName("valor")] decimal Amount);
}
