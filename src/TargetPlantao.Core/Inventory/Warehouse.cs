using TargetPlantao.Core.Common;

namespace TargetPlantao.Core.Inventory;

public sealed class Warehouse
{
    private readonly Dictionary<int, Product> _products = [];
    private readonly List<StockMovement> _movements = [];
    private readonly TimeProvider _clock;
    private long _lastMovementId;

    public Warehouse(IEnumerable<Product> products, TimeProvider clock)
    {
        foreach (var product in products)
        {
            if (!_products.TryAdd(product.Code, product))
                throw new ArgumentException($"Código de produto duplicado: {product.Code}.", nameof(products));
        }

        _clock = clock;
    }

    public IReadOnlyList<Product> Products => [.. _products.Values.OrderBy(p => p.Code)];

    public IReadOnlyList<StockMovement> Movements => _movements.AsReadOnly();

    public StockMovement Register(MovementRequest request)
    {
        if (request.Quantity <= 0)
            throw new DomainException(new DomainError.NonPositiveQuantity());

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new DomainException(new DomainError.MissingDescription());

        if (!_products.TryGetValue(request.ProductCode, out var product))
            throw new DomainException(new DomainError.ProductNotFound(request.ProductCode));

        var previousQuantity = product.Quantity;
        product.Apply(request.Type, request.Quantity);

        var movement = new StockMovement(
            Id: ++_lastMovementId,
            ProductCode: product.Code,
            ProductDescription: product.Description,
            Type: request.Type,
            Quantity: request.Quantity,
            Description: request.Description.Trim(),
            OccurredAt: _clock.GetLocalNow(),
            PreviousQuantity: previousQuantity,
            FinalQuantity: product.Quantity);

        _movements.Add(movement);
        return movement;
    }
}
