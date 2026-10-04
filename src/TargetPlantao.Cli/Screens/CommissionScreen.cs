using Spectre.Console;
using TargetPlantao.Cli.Localization;
using TargetPlantao.Cli.Ui;
using TargetPlantao.Core.Commissions;
using static TargetPlantao.Cli.Ui.Theme;

namespace TargetPlantao.Cli.Screens;

internal sealed class CommissionScreen(Terminal terminal, CommissionPolicy policy, IReadOnlyList<Sale> sales) : IScreen
{
    private readonly IReadOnlyList<SellerCommission> _summaries = new CommissionCalculator(policy).CalculateBySeller(sales);

    private Locale Locale => terminal.Locale;

    private CommissionStrings Text => terminal.Text.Commission;

    public string Title => Text.Title;

    public string Summary => Text.Summary;

    public void Show()
    {
        while (true)
        {
            terminal.Header(Text.Section);
            terminal.Hint(PolicyDescription());
            terminal.NewLine();
            terminal.Show(SummaryTable());

            if (!terminal.TryChoose(_summaries, s => Paint(Bright, s.Seller), out var seller))
                return;

            ShowSeller(seller);
        }
    }

    private void ShowSeller(SellerCommission seller)
    {
        terminal.Header(Text.Section, seller.Seller);

        var table = NewTable()
            .ShowFooters()
            .AddColumn(Column("#", alignRight: true))
            .AddColumn(Column(Text.Sale, alignRight: true, footer: Paint(Soft, Locale.Currency(seller.TotalSold))))
            .AddColumn(Column(Text.Tier, alignRight: true))
            .AddColumn(Column(Text.Commission, alignRight: true, footer: Paint(Gold, Locale.Currency(seller.TotalCommission))));

        foreach (var (line, index) in seller.Sales.Select((line, index) => (line, index + 1)))
        {
            table.AddRow(
                Paint(Muted, index),
                Paint(Bright, Locale.Currency(line.Sale.Amount)),
                RateLabel(line.Rate),
                line.Commission == 0 ? Paint(Muted, "—") : Paint(Bright, Locale.Currency(line.Commission)));
        }

        terminal.Show(table);
        terminal.WaitForBack();
    }

    private Table SummaryTable()
    {
        var tiers = string.Join(" · ", policy.Tiers.Select(t => Locale.Percent(t.Rate)));

        var table = NewTable()
            .ShowFooters()
            .AddColumn(Column(Text.Seller, footer: Paint(Soft, Text.Total)))
            .AddColumn(Column(Text.Sales, alignRight: true, footer: Paint(Soft, _summaries.Sum(s => s.Sales.Count))))
            .AddColumn(Column($"{Text.ByTier} ({tiers})", alignRight: true))
            .AddColumn(Column(Text.TotalSold, alignRight: true, footer: Paint(Soft, Locale.Currency(_summaries.Sum(s => s.TotalSold)))))
            .AddColumn(Column(Text.Commission, alignRight: true, footer: Paint(Gold, Locale.Currency(_summaries.Sum(s => s.TotalCommission)))));

        foreach (var seller in _summaries)
        {
            var salesPerTier = policy.Tiers.Select(tier => seller.Sales.Count(line => line.Rate == tier.Rate));

            table.AddRow(
                Paint(Bright, seller.Seller),
                Paint(Muted, seller.Sales.Count),
                Paint(Muted, string.Join(" · ", salesPerTier)),
                Paint(Bright, Locale.Currency(seller.TotalSold)),
                Paint(Gold, Locale.Currency(seller.TotalCommission)));
        }

        return table;
    }

    private string PolicyDescription()
    {
        var tiers = policy.Tiers;
        var rules = tiers.Select((tier, i) => i == 0 && tiers.Count > 1
            ? $"< {Locale.Currency(tiers[1].MinimumAmount)} → {Locale.Percent(tier.Rate)}"
            : $"≥ {Locale.Currency(tier.MinimumAmount)} → {Locale.Percent(tier.Rate)}");

        return string.Join("  ·  ", rules);
    }

    private string RateLabel(decimal rate)
    {
        var color = rate == 0 ? Muted : rate == policy.Tiers[^1].Rate ? Gold : Soft;
        return Paint(color, Locale.Percent(rate));
    }
}
