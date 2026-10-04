using Spectre.Console;
using TargetPlantao.Cli.Localization;
using TargetPlantao.Cli.Ui;
using TargetPlantao.Core.Common;
using TargetPlantao.Core.Interest;
using static TargetPlantao.Cli.Ui.Theme;

namespace TargetPlantao.Cli.Screens;

internal sealed class LateInterestScreen(Terminal terminal, LateInterestCalculator calculator) : IScreen
{
    private Locale Locale => terminal.Locale;

    private InterestStrings Text => terminal.Text.Interest;

    public string Title => Text.Title;

    public string Summary => Text.Summary(Locale.Percent(calculator.DailyRate));

    public void Show()
    {
        do
        {
            terminal.Header(Text.Section);
            terminal.Hint(Text.Today(Locale.Date(calculator.Today), Locale.Percent(calculator.DailyRate)));
            terminal.NewLine();

            var amount = terminal.Ask<decimal>(Text.AmountPrompt, Locale.TryParseMoney, Text.InvalidAmount);
            var dueDate = terminal.Ask<DateOnly>(Text.DueDatePrompt, Locale.TryParseDate, Text.InvalidDate);

            terminal.NewLine();
            try
            {
                terminal.Show(QuoteTable(calculator.Calculate(amount, dueDate)));
            }
            catch (DomainException error)
            {
                terminal.Error(terminal.Text.Error(error.Error));
            }
        }
        while (terminal.TryChoose<string>([Text.NewCalculation], label => Paint(Bright, label), out _));
    }

    private Table QuoteTable(LateInterestQuote quote)
    {
        var table = NewTable()
            .HideHeaders()
            .ShowFooters()
            .AddColumn(Column("", footer: Paint(Soft, Text.UpdatedTotal)))
            .AddColumn(Column("", alignRight: true, footer: Paint(Gold, Locale.Currency(quote.UpdatedAmount))));

        table.AddRow(Paint(Muted, Text.OriginalAmount), Paint(Bright, Locale.Currency(quote.Amount)));
        table.AddRow(Paint(Muted, Text.DueDate), Paint(Bright, Locale.Date(quote.DueDate)));
        table.AddRow(Paint(Muted, Text.DaysOverdue), Paint(Bright, quote.DaysOverdue));

        if (quote.IsOverdue)
            table.AddRow(Paint(Muted, Text.Formula(Locale.Percent(quote.DailyRate), quote.DaysOverdue)), Paint(Gold, Locale.Currency(quote.Interest)));
        else
            table.AddRow(Paint(Muted, Text.Interest), Paint(Soft, Text.NoInterest));

        return table;
    }
}
