using TargetPlantao.Core.Common;

namespace TargetPlantao.Cli.Localization;

internal sealed partial record Strings
{
    public required string Back { get; init; }

    public required string Quit { get; init; }

    public required string Unit { get; init; }

    public required Func<int, string> MenuHint { get; init; }

    public required Func<string, string> LoadFailed { get; init; }

    public required Func<DomainError, string> Error { get; init; }

    public required CommissionStrings Commission { get; init; }

    public required InventoryStrings Inventory { get; init; }

    public required InterestStrings Interest { get; init; }
}

internal sealed record CommissionStrings
{
    public required string Title { get; init; }

    public required string Summary { get; init; }

    public required string Section { get; init; }

    public required string Seller { get; init; }

    public required string Sales { get; init; }

    public required string ByTier { get; init; }

    public required string TotalSold { get; init; }

    public required string Commission { get; init; }

    public required string Total { get; init; }

    public required string Sale { get; init; }

    public required string Tier { get; init; }
}

internal sealed record InventoryStrings
{
    public required string Title { get; init; }

    public required string Summary { get; init; }

    public required string Section { get; init; }

    public required string NewMovement { get; init; }

    public required string History { get; init; }

    public required string Code { get; init; }

    public required string Product { get; init; }

    public required string Stock { get; init; }

    public required string Time { get; init; }

    public required string Type { get; init; }

    public required string Quantity { get; init; }

    public required string Description { get; init; }

    public required string FinalStock { get; init; }

    public required string Inbound { get; init; }

    public required string Outbound { get; init; }

    public required string QuantityPrompt { get; init; }

    public required string DescriptionPrompt { get; init; }

    public required string InvalidQuantity { get; init; }

    public required string MissingDescription { get; init; }

    public required string NoMovements { get; init; }

    public required string Of { get; init; }

    public required Func<long, string> Registered { get; init; }
}

internal sealed record InterestStrings
{
    public required string Title { get; init; }

    public required Func<string, string> Summary { get; init; }

    public required string Section { get; init; }

    public required Func<string, string, string> Today { get; init; }

    public required string AmountPrompt { get; init; }

    public required string DueDatePrompt { get; init; }

    public required string InvalidAmount { get; init; }

    public required string InvalidDate { get; init; }

    public required string OriginalAmount { get; init; }

    public required string DueDate { get; init; }

    public required string DaysOverdue { get; init; }

    public required Func<string, int, string> Formula { get; init; }

    public required string Interest { get; init; }

    public required string NoInterest { get; init; }

    public required string UpdatedTotal { get; init; }

    public required string NewCalculation { get; init; }
}
