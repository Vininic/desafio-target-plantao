namespace TargetPlantao.Core.Inventory;

public sealed record MovementRequest(int ProductCode, MovementType Type, int Quantity, string Description);

public sealed record StockMovement(
    long Id,
    int ProductCode,
    string ProductDescription,
    MovementType Type,
    int Quantity,
    string Description,
    DateTimeOffset OccurredAt,
    int PreviousQuantity,
    int FinalQuantity);
