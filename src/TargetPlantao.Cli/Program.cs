using System.Text;
using TargetPlantao.Cli.Screens;
using TargetPlantao.Cli.Ui;
using TargetPlantao.Core.Commissions;
using TargetPlantao.Core.Interest;
using TargetPlantao.Core.Inventory;
using Spectre.Console;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var console = AnsiConsole.Console;
var clock = TimeProvider.System;

try
{
    IScreen[] screens =
    [
        new CommissionScreen(console, CommissionPolicy.Default, Load("vendas.json", SalesJsonReader.Read)),
        new InventoryScreen(console, new Warehouse(Load("estoque.json", StockJsonReader.Read), clock)),
        new LateInterestScreen(console, new LateInterestCalculator(clock)),
    ];

    new MainMenu(console, screens).Run();
    return 0;
}
catch (Exception error) when (error is IOException or System.Text.Json.JsonException)
{
    console.Error($"não foi possível carregar os dados: {error.Message}");
    return 1;
}

static T Load<T>(string fileName, Func<Stream, T> read)
{
    using var json = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Data", fileName));
    return read(json);
}
