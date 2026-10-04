using TargetPlantao.Cli.Ui;
using TargetPlantao.Core.Common;
using TargetPlantao.Core.Inventory;
using Spectre.Console;
using static TargetPlantao.Cli.Ui.Theme;

namespace TargetPlantao.Cli.Screens;

internal sealed class InventoryScreen(IAnsiConsole console, Warehouse warehouse) : IScreen
{
    private const string Section = "02 · estoque";
    private const string NewMovement = "lançar movimentação";
    private const string History = "histórico";

    public string Title => "Movimentação de estoque";

    public string Summary => "entradas, saídas e saldo";

    public void Show()
    {
        while (true)
        {
            console.Header(Section);
            console.Indented(StockTable());

            var actions = new[] { NewMovement, History };
            if (!console.TryChoose(actions, ActionLabel, out var action))
                return;

            switch (action)
            {
                case NewMovement: RegisterMovement(); break;
                case History: ShowHistory(); break;
            }
        }
    }

    private void RegisterMovement()
    {
        console.Header(Section, NewMovement);

        if (!console.TryChoose(warehouse.Products, ProductLabel, out var product))
            return;

        if (!console.TryChoose(Enum.GetValues<MovementType>(), TypeLabel, out var type))
            return;

        console.WriteLine();
        var quantity = console.Ask<int>("quantidade:", InputParser.TryParseQuantity, "quantidade inválida");
        var description = console.Ask<string>("descrição:", InputParser.TryParseText, "descrição obrigatória");

        console.WriteLine();
        try
        {
            var movement = warehouse.Register(new MovementRequest(product.Code, type, quantity, description));

            console.Success($"movimentação #{movement.Id} registrada");
            console.MarkupLine(
                $"    {TypeLabel(movement.Type)} de {Paint(Bright, Format.Units(movement.Quantity))} · " +
                $"{Paint(Bright, movement.ProductDescription)} · {Paint(Muted, movement.Description)}");
            console.MarkupLine(
                $"    estoque final  {Paint(Muted, Format.Units(movement.PreviousQuantity))} → {Paint(Gold, Format.Units(movement.FinalQuantity))}");
        }
        catch (DomainException error)
        {
            console.Error(error.Message);
        }

        console.WaitForBack();
    }

    private void ShowHistory()
    {
        console.Header(Section, History);

        if (warehouse.Movements.Count == 0)
        {
            console.Hint("sem movimentações");
            console.WaitForBack();
            return;
        }

        var table = NewTable()
            .AddColumn(Column("#", alignRight: true))
            .AddColumn(Column("Hora"))
            .AddColumn(Column("Produto"))
            .AddColumn(Column("Tipo"))
            .AddColumn(Column("Qtde", alignRight: true))
            .AddColumn(Column("Descrição"))
            .AddColumn(Column("Saldo final", alignRight: true));

        foreach (var movement in warehouse.Movements)
        {
            var sign = movement.Type == MovementType.Inbound ? "+" : "−";

            table.AddRow(
                Paint(Muted, movement.Id),
                Paint(Muted, movement.OccurredAt.ToString("HH:mm:ss", Format.PtBr)),
                Paint(Bright, movement.ProductDescription),
                TypeLabel(movement.Type),
                Paint(Bright, sign + movement.Quantity),
                Paint(Muted, movement.Description),
                Paint(Gold, Format.Units(movement.FinalQuantity)));
        }

        console.Indented(table);
        console.WaitForBack();
    }

    private Table StockTable()
    {
        var table = NewTable()
            .AddColumn(Column("Código"))
            .AddColumn(Column("Produto"))
            .AddColumn(Column("Estoque", alignRight: true));

        foreach (var product in warehouse.Products)
            table.AddRow(Paint(Muted, product.Code), Paint(Bright, product.Description), Paint(Gold, Format.Units(product.Quantity)));

        return table;
    }

    private string ActionLabel(string action) =>
        Paint(Bright, action == History ? $"{History} ({warehouse.Movements.Count})" : action);

    private static string ProductLabel(Product product) =>
        $"{Paint(Muted, product.Code)}  {Paint(Bright, product.Description)}  {Paint(Muted, Format.Units(product.Quantity))}";

    private static string TypeLabel(MovementType type) => type switch
    {
        MovementType.Inbound => Paint(Soft, "entrada"),
        MovementType.Outbound => Paint(Accent, "saída"),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };
}
