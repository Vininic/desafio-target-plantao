using TargetPlantao.Cli.Screens;
using TargetPlantao.Cli.Ui;
using TargetPlantao.Core.Commissions;
using TargetPlantao.Core.Interest;
using TargetPlantao.Core.Inventory;
using Spectre.Console.Testing;

namespace TargetPlantao.Tests.Cli;

public sealed class ScreenFlowTests : IDisposable
{
    private readonly TestConsole _console = new TestConsole().Interactive().Width(120);

    public void Dispose() => _console.Dispose();

    [Fact]
    public void Commission_screen_ranks_the_sellers_and_opens_the_detail_of_one()
    {
        using var json = Open("vendas.json");
        var screen = new CommissionScreen(_console, CommissionPolicy.Default, SalesJsonReader.Read(json));

        _console.Input.PushKey(ConsoleKey.Enter);
        _console.Input.PushKey(ConsoleKey.Enter);
        _console.Input.PushKey(ConsoleKey.Escape);
        screen.Show();

        Assert.Contains("João Silva", _console.Output, StringComparison.Ordinal);
        Assert.Contains("R$ 1.746,02", _console.Output, StringComparison.Ordinal);
        Assert.Contains("R$ 60,03", _console.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Inventory_screen_registers_an_outbound_movement_and_shows_the_final_stock()
    {
        var warehouse = new Warehouse([new Product(101, "Caneta Azul", 150)], TimeProvider.System);
        var screen = new InventoryScreen(_console, warehouse);

        _console.Input.PushKey(ConsoleKey.Enter);
        _console.Input.PushKey(ConsoleKey.Enter);
        _console.Input.PushKey(ConsoleKey.DownArrow);
        _console.Input.PushKey(ConsoleKey.Enter);
        _console.Input.PushTextWithEnter("0");
        _console.Input.PushTextWithEnter("10");
        _console.Input.PushTextWithEnter("Venda no balcão");
        _console.Input.PushKey(ConsoleKey.Enter);
        _console.Input.PushKey(ConsoleKey.Escape);
        screen.Show();

        var movement = Assert.Single(warehouse.Movements);
        Assert.Equal(MovementType.Outbound, movement.Type);
        Assert.Equal(140, movement.FinalQuantity);
        Assert.Contains("quantidade inválida", _console.Output, StringComparison.Ordinal);
        Assert.Contains("movimentação #1 registrada", _console.Output, StringComparison.Ordinal);
        Assert.Contains("150 un → 140 un", _console.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Inventory_screen_shows_the_domain_error_when_stock_is_insufficient()
    {
        var warehouse = new Warehouse([new Product(101, "Caneta Azul", 5)], TimeProvider.System);
        var screen = new InventoryScreen(_console, warehouse);

        _console.Input.PushKey(ConsoleKey.Enter);
        _console.Input.PushKey(ConsoleKey.Enter);
        _console.Input.PushKey(ConsoleKey.DownArrow);
        _console.Input.PushKey(ConsoleKey.Enter);
        _console.Input.PushTextWithEnter("6");
        _console.Input.PushTextWithEnter("Venda");
        _console.Input.PushKey(ConsoleKey.Enter);
        _console.Input.PushKey(ConsoleKey.Escape);
        screen.Show();

        Assert.Empty(warehouse.Movements);
        Assert.Contains("Estoque insuficiente", _console.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Late_interest_screen_reprompts_invalid_input_and_shows_the_quote()
    {
        var clock = ClockAt(new DateTimeOffset(2026, 10, 3, 12, 0, 0, TimeSpan.Zero));
        var screen = new LateInterestScreen(_console, new LateInterestCalculator(clock));

        _console.Input.PushTextWithEnter("abc");
        _console.Input.PushTextWithEnter("1.000,00");
        _console.Input.PushTextWithEnter("31/02/2026");
        _console.Input.PushTextWithEnter("23/09/2026");
        _console.Input.PushKey(ConsoleKey.Escape);
        screen.Show();

        Assert.Contains("valor inválido", _console.Output, StringComparison.Ordinal);
        Assert.Contains("data inválida", _console.Output, StringComparison.Ordinal);
        Assert.Contains("2,5% × 10 dias", _console.Output, StringComparison.Ordinal);
        Assert.Contains("R$ 250,00", _console.Output, StringComparison.Ordinal);
        Assert.Contains("R$ 1.250,00", _console.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Main_menu_opens_screens_by_arrows_and_shortcuts_and_quits_with_q()
    {
        var first = new RecordingScreen("primeira");
        var second = new RecordingScreen("segunda");

        _console.Input.PushKey(ConsoleKey.DownArrow);
        _console.Input.PushKey(ConsoleKey.Enter);
        _console.Input.PushCharacter('1');
        _console.Input.PushCharacter('q');
        new MainMenu(_console, [first, second]).Run();

        Assert.Equal(1, first.Opened);
        Assert.Equal(1, second.Opened);
        Assert.Contains("github.com/Vininic", _console.Output, StringComparison.Ordinal);
    }

    private sealed class RecordingScreen(string title) : IScreen
    {
        public int Opened { get; private set; }

        public string Title => title;

        public string Summary => "tela de teste";

        public void Show() => Opened++;
    }
}
