using TargetPlantao.Core.Common;

namespace TargetPlantao.Cli.Localization;

internal sealed partial record Strings
{
    public static Strings English { get; } = new()
    {
        Back = "← back",
        Quit = "quit",
        Unit = "units",
        MenuHint = screens => $"↑↓ navigate  ·  enter open  ·  1-{screens} shortcut  ·  l português  ·  q quit",
        LoadFailed = reason => $"could not load the data: {reason}",
        Error = error => error switch
        {
            DomainError.ProductNotFound e => $"Product {e.ProductCode} not found.",
            DomainError.NonPositiveQuantity => "Quantity must be greater than zero.",
            DomainError.MissingDescription => "Description is required.",
            DomainError.InsufficientStock e => $"Insufficient stock: {e.Product} has {e.Available} units.",
            DomainError.NonPositiveAmount => "Amount must be greater than zero.",
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, null),
        },
        Commission = new()
        {
            Title = "Sales commissions",
            Summary = "commission per seller",
            Section = "01 · commissions",
            Seller = "Seller",
            Sales = "Sales",
            ByTier = "By tier",
            TotalSold = "Total sold",
            Commission = "Commission",
            Total = "Total",
            Sale = "Sale",
            Tier = "Tier",
        },
        Inventory = new()
        {
            Title = "Stock movements",
            Summary = "inbound, outbound and balance",
            Section = "02 · stock",
            NewMovement = "new movement",
            History = "history",
            Code = "Code",
            Product = "Product",
            Stock = "Stock",
            Time = "Time",
            Type = "Type",
            Quantity = "Qty",
            Description = "Description",
            FinalStock = "Final stock",
            Inbound = "inbound",
            Outbound = "outbound",
            QuantityPrompt = "quantity:",
            DescriptionPrompt = "description:",
            InvalidQuantity = "invalid quantity",
            MissingDescription = "description is required",
            NoMovements = "no movements",
            Of = "of",
            Registered = id => $"movement #{id} registered",
        },
        Interest = new()
        {
            Title = "Late interest",
            Summary = rate => $"{rate} per day",
            Section = "03 · interest",
            Today = (date, rate) => $"today {date}  ·  {rate} per day",
            AmountPrompt = "amount:",
            DueDatePrompt = "due date (mm/dd/yyyy):",
            InvalidAmount = "invalid amount",
            InvalidDate = "invalid date",
            OriginalAmount = "original amount",
            DueDate = "due date",
            DaysOverdue = "days overdue",
            Formula = (rate, days) => $"interest ({rate} × {days} days)",
            Interest = "interest",
            NoInterest = "none",
            UpdatedTotal = "updated total",
            NewCalculation = "new calculation",
        },
    };
}
