using System.Text;
using Spectre.Console;
using TargetPlantao.Cli.Localization;
using TargetPlantao.Cli.Screens;
using TargetPlantao.Cli.Ui;
using TargetPlantao.Core.Commissions;
using TargetPlantao.Core.Interest;
using TargetPlantao.Core.Inventory;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var terminal = new Terminal(AnsiConsole.Console, args.Contains("--en") ? Locale.English : Locale.Portuguese);
var clock = TimeProvider.System;

try
{
    IScreen[] screens =
    [
        new CommissionScreen(terminal, CommissionPolicy.Default, Load("vendas.json", SalesJsonReader.Read)),
        new InventoryScreen(terminal, new Warehouse(Load("estoque.json", StockJsonReader.Read), clock)),
        new LateInterestScreen(terminal, new LateInterestCalculator(clock)),
    ];

    new MainMenu(terminal, screens).Run();
    return 0;
}
catch (Exception error) when (error is IOException or System.Text.Json.JsonException)
{
    terminal.Error(terminal.Text.LoadFailed(error.Message));
    return 1;
}

static T Load<T>(string fileName, Func<Stream, T> read)
{
    using var json = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Data", fileName));
    return read(json);
}
