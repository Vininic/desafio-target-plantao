using TargetPlantao.Core.Common;
using TargetPlantao.Core.Inventory;

namespace TargetPlantao.Tests.Core.Inventory;

public class WarehouseTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 3, 14, 30, 0, TimeSpan.Zero);

    private readonly Warehouse _warehouse = new(
        [new Product(101, "Caneta Azul", 150), new Product(102, "Caderno Universitário", 75)],
        ClockAt(Now));

    [Fact]
    public void Inbound_movement_adds_to_stock_and_returns_the_final_quantity()
    {
        var movement = _warehouse.Register(new MovementRequest(101, MovementType.Inbound, 50, "Compra do fornecedor"));

        Assert.Equal(150, movement.PreviousQuantity);
        Assert.Equal(200, movement.FinalQuantity);
        Assert.Equal(200, Stock(101));
    }

    [Fact]
    public void Outbound_movement_subtracts_from_stock_and_returns_the_final_quantity()
    {
        var movement = _warehouse.Register(new MovementRequest(102, MovementType.Outbound, 75, "Venda no balcão"));

        Assert.Equal(0, movement.FinalQuantity);
        Assert.Equal(0, Stock(102));
    }

    [Fact]
    public void Movement_records_what_happened_and_when()
    {
        var movement = _warehouse.Register(new MovementRequest(101, MovementType.Outbound, 10, "  Venda no balcão  "));

        Assert.Equal(101, movement.ProductCode);
        Assert.Equal("Caneta Azul", movement.ProductDescription);
        Assert.Equal(MovementType.Outbound, movement.Type);
        Assert.Equal(10, movement.Quantity);
        Assert.Equal("Venda no balcão", movement.Description);
        Assert.Equal(Now, movement.OccurredAt);
        Assert.Equal(movement, Assert.Single(_warehouse.Movements));
    }

    [Fact]
    public void Each_movement_gets_a_unique_sequential_id()
    {
        var ids = Enumerable.Range(0, 5)
            .Select(_ => _warehouse.Register(new MovementRequest(101, MovementType.Inbound, 1, "Ajuste")).Id)
            .ToList();

        Assert.Equal([1L, 2L, 3L, 4L, 5L], ids);
    }

    [Fact]
    public void Outbound_beyond_available_stock_is_rejected_without_side_effects()
    {
        var error = Assert.Throws<DomainException>(
            () => _warehouse.Register(new MovementRequest(102, MovementType.Outbound, 76, "Venda")));

        Assert.Equal(new DomainError.InsufficientStock("Caderno Universitário", 75), error.Error);
        Assert.Equal(75, Stock(102));
        Assert.Empty(_warehouse.Movements);
    }

    [Fact]
    public void Rejected_movements_do_not_consume_ids()
    {
        Assert.Throws<DomainException>(
            () => _warehouse.Register(new MovementRequest(102, MovementType.Outbound, 999, "Venda")));

        var movement = _warehouse.Register(new MovementRequest(102, MovementType.Outbound, 1, "Venda"));

        Assert.Equal(1, movement.Id);
    }

    [Theory]
    [InlineData(999, 10, "Venda", typeof(DomainError.ProductNotFound))]
    [InlineData(101, 0, "Venda", typeof(DomainError.NonPositiveQuantity))]
    [InlineData(101, -5, "Venda", typeof(DomainError.NonPositiveQuantity))]
    [InlineData(101, 10, "   ", typeof(DomainError.MissingDescription))]
    public void Invalid_requests_are_rejected(int productCode, int quantity, string description, Type expectedError)
    {
        var error = Assert.Throws<DomainException>(
            () => _warehouse.Register(new MovementRequest(productCode, MovementType.Inbound, quantity, description)));

        Assert.IsType(expectedError, error.Error);
        Assert.Empty(_warehouse.Movements);
    }

    [Fact]
    public void Duplicate_product_codes_are_rejected()
    {
        Assert.Throws<ArgumentException>(() => new Warehouse(
            [new Product(101, "A", 1), new Product(101, "B", 2)], TimeProvider.System));
    }

    [Fact]
    public void Challenge_stock_file_is_loaded_in_code_order()
    {
        using var json = Open("estoque.json");

        var warehouse = new Warehouse(StockJsonReader.Read(json), TimeProvider.System);

        Assert.Equal([101, 102, 103, 104, 105], warehouse.Products.Select(p => p.Code));
        Assert.Equal("Caderno Universitário", warehouse.Products[1].Description);
        Assert.Equal(835, warehouse.Products.Sum(p => p.Quantity));
    }

    private int Stock(int productCode) => _warehouse.Products.Single(p => p.Code == productCode).Quantity;
}
