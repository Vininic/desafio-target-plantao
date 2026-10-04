using TargetPlantao.Core.Common;

namespace TargetPlantao.Core.Inventory;

public sealed class Product
{
    public Product(int code, string description, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        Code = code;
        Description = description;
        Quantity = quantity;
    }

    public int Code { get; }

    public string Description { get; }

    public int Quantity { get; private set; }

    internal void Apply(MovementType type, int quantity)
    {
        var newQuantity = type switch
        {
            MovementType.Inbound => Quantity + quantity,
            MovementType.Outbound => Quantity - quantity,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

        if (newQuantity < 0)
            throw new DomainException($"Estoque insuficiente: {Description} tem {Quantity} un.");

        Quantity = newQuantity;
    }
}
