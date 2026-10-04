using TargetPlantao.Cli.Ui;
using TargetPlantao.Core.Common;
using TargetPlantao.Core.Interest;
using Spectre.Console;
using static TargetPlantao.Cli.Ui.Theme;

namespace TargetPlantao.Cli.Screens;

internal sealed class LateInterestScreen(IAnsiConsole console, LateInterestCalculator calculator) : IScreen
{
    private const string Section = "03 · juros";
    private const string CalculateAgain = "novo cálculo";

    public string Title => "Juros por atraso";

    public string Summary => $"{Format.Percent(calculator.DailyRate)} ao dia";

    public void Show()
    {
        do
        {
            console.Header(Section);
            console.Hint($"hoje {Format.Date(calculator.Today)}  ·  {Format.Percent(calculator.DailyRate)} ao dia");
            console.WriteLine();

            var amount = console.Ask<decimal>("valor:", InputParser.TryParseMoney, "valor inválido");
            var dueDate = console.Ask<DateOnly>("vencimento (dd/mm/aaaa):", InputParser.TryParseDate, "data inválida");

            console.WriteLine();
            try
            {
                console.Indented(QuoteTable(calculator.Calculate(amount, dueDate)));
            }
            catch (DomainException error)
            {
                console.Error(error.Message);
            }
        }
        while (console.TryChoose<string>([CalculateAgain], label => Paint(Bright, label), out _));
    }

    private static Table QuoteTable(LateInterestQuote quote)
    {
        var table = NewTable()
            .HideHeaders()
            .ShowFooters()
            .AddColumn(Column("", footer: Paint(Soft, "total atualizado")))
            .AddColumn(Column("", alignRight: true, footer: Paint(Gold, Format.Currency(quote.UpdatedAmount))));

        table.AddRow(Paint(Muted, "valor original"), Paint(Bright, Format.Currency(quote.Amount)));
        table.AddRow(Paint(Muted, "vencimento"), Paint(Bright, Format.Date(quote.DueDate)));
        table.AddRow(Paint(Muted, "dias em atraso"), Paint(Bright, quote.DaysOverdue));

        if (quote.IsOverdue)
        {
            var formula = $"juros ({Format.Percent(quote.DailyRate)} × {quote.DaysOverdue} dias)";
            table.AddRow(Paint(Muted, formula), Paint(Gold, Format.Currency(quote.Interest)));
        }
        else
        {
            table.AddRow(Paint(Muted, "juros"), Paint(Soft, "sem juros"));
        }

        return table;
    }
}
