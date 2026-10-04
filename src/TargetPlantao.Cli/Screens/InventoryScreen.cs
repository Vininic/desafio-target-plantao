using Spectre.Console;
using TargetPlantao.Cli.Localization;
using TargetPlantao.Cli.Ui;
using TargetPlantao.Core.Common;
using TargetPlantao.Core.Inventory;
using static TargetPlantao.Cli.Ui.Theme;

namespace TargetPlantao.Cli.Screens;

internal sealed class InventoryScreen(Terminal terminal, Warehouse warehouse) : IScreen
{
    private enum Action
    {
        NewMovement,
        History,
    }

    private Locale Locale => terminal.Locale;

    private InventoryStrings Text => terminal.Text.Inventory;

    public string Title => Text.Title;

    public string Summary => Text.Summary;

    public void Show()
    {
        while (true)
        {
            terminal.Header(Text.Section);
            terminal.Show(StockTable());

            if (!terminal.TryChoose(Enum.GetValues<Action>(), ActionLabel, out var action))
                return;

            switch (action)
            {
                case Action.NewMovement: RegisterMovement(); break;
                case Action.History: ShowHistory(); break;
            }
        }
    }

    private void RegisterMovement()
    {
        terminal.Header(Text.Section, Text.NewMovement);

        if (!terminal.TryChoose(warehouse.Products, ProductLabel, out var product))
            return;

        if (!terminal.TryChoose(Enum.GetValues<MovementType>(), TypeLabel, out var type))
            return;

        terminal.NewLine();
        var quantity = terminal.Ask<int>(Text.QuantityPrompt, Locale.TryParseQuantity, Text.InvalidQuantity);
        var description = terminal.Ask<string>(Text.DescriptionPrompt, Locale.TryParseText, Text.MissingDescription);

        terminal.NewLine();
        try
        {
            var movement = warehouse.Register(new MovementRequest(product.Code, type, quantity, description));

            terminal.Success(Text.Registered(movement.Id));
            terminal.Line(
                $"    {TypeLabel(movement.Type)} {Paint(Muted, Text.Of)} {Paint(Bright, Locale.Units(movement.Quantity))} · " +
                $"{Paint(Bright, movement.ProductDescription)} · {Paint(Muted, movement.Description)}");
            terminal.Line(
                $"    {Paint(Muted, Text.FinalStock)}  {Paint(Muted, Locale.Units(movement.PreviousQuantity))} → {Paint(Gold, Locale.Units(movement.FinalQuantity))}");
        }
        catch (DomainException error)
        {
            terminal.Error(terminal.Text.Error(error.Error));
        }

        terminal.WaitForBack();
    }

    private void ShowHistory()
    {
        terminal.Header(Text.Section, Text.History);

        if (warehouse.Movements.Count == 0)
        {
            terminal.Hint(Text.NoMovements);
            terminal.WaitForBack();
            return;
        }

        var table = NewTable()
            .AddColumn(Column("#", alignRight: true))
            .AddColumn(Column(Text.Time))
            .AddColumn(Column(Text.Product))
            .AddColumn(Column(Text.Type))
            .AddColumn(Column(Text.Quantity, alignRight: true))
            .AddColumn(Column(Text.Description))
            .AddColumn(Column(Text.FinalStock, alignRight: true));

        foreach (var movement in warehouse.Movements)
        {
            var sign = movement.Type == MovementType.Inbound ? "+" : "−";

            table.AddRow(
                Paint(Muted, movement.Id),
                Paint(Muted, Locale.Time(movement.OccurredAt)),
                Paint(Bright, movement.ProductDescription),
                TypeLabel(movement.Type),
                Paint(Bright, sign + movement.Quantity),
                Paint(Muted, movement.Description),
                Paint(Gold, Locale.Units(movement.FinalQuantity)));
        }

        terminal.Show(table);
        terminal.WaitForBack();
    }

    private Table StockTable()
    {
        var table = NewTable()
            .AddColumn(Column(Text.Code))
            .AddColumn(Column(Text.Product))
            .AddColumn(Column(Text.Stock, alignRight: true));

        foreach (var product in warehouse.Products)
            table.AddRow(Paint(Muted, product.Code), Paint(Bright, product.Description), Paint(Gold, Locale.Units(product.Quantity)));

        return table;
    }

    private string ActionLabel(Action action) => action switch
    {
        Action.NewMovement => Paint(Bright, Text.NewMovement),
        Action.History => Paint(Bright, $"{Text.History} ({warehouse.Movements.Count})"),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
    };

    private string ProductLabel(Product product) =>
        $"{Paint(Muted, product.Code)}  {Paint(Bright, product.Description)}  {Paint(Muted, Locale.Units(product.Quantity))}";

    private string TypeLabel(MovementType type) => type switch
    {
        MovementType.Inbound => Paint(Soft, Text.Inbound),
        MovementType.Outbound => Paint(Accent, Text.Outbound),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };
}
