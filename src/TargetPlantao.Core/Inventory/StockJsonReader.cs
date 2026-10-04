using System.Text.Json.Serialization;
using TargetPlantao.Core.Common;

namespace TargetPlantao.Core.Inventory;

public static class StockJsonReader
{
    public static IReadOnlyList<Product> Read(Stream json) =>
    [
        .. JsonData.Deserialize<StockDocument>(json).Items
            .Select(dto => new Product(dto.Code, dto.Description, dto.Quantity)),
    ];

    private sealed record StockDocument(
        [property: JsonRequired, JsonPropertyName("estoque")] IReadOnlyList<ProductDto> Items);

    private sealed record ProductDto(
        [property: JsonRequired, JsonPropertyName("codigoProduto")] int Code,
        [property: JsonRequired, JsonPropertyName("descricaoProduto")] string Description,
        [property: JsonRequired, JsonPropertyName("estoque")] int Quantity);
}
