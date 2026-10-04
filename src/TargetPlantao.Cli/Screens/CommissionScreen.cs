using TargetPlantao.Cli.Ui;
using TargetPlantao.Core.Commissions;
using Spectre.Console;
using static TargetPlantao.Cli.Ui.Theme;

namespace TargetPlantao.Cli.Screens;

internal sealed class CommissionScreen(IAnsiConsole console, CommissionPolicy policy, IReadOnlyList<Sale> sales) : IScreen
{
    private const string Section = "01 · comissões";

    private readonly IReadOnlyList<SellerCommission> _summaries = new CommissionCalculator(policy).CalculateBySeller(sales);

    public string Title => "Comissões de vendas";

    public string Summary => "comissão por vendedor";

    public void Show()
    {
        while (true)
        {
            console.Header(Section);
            console.Hint(PolicyDescription());
            console.WriteLine();
            console.Indented(SummaryTable());

            if (!console.TryChoose(_summaries, s => Paint(Bright, s.Seller), out var seller))
                return;

            ShowSeller(seller);
        }
    }

    private void ShowSeller(SellerCommission seller)
    {
        console.Header(Section, seller.Seller);

        var table = NewTable()
            .ShowFooters()
            .AddColumn(Column("#", alignRight: true))
            .AddColumn(Column("Venda", alignRight: true, footer: Paint(Soft, Format.Currency(seller.TotalSold))))
            .AddColumn(Column("Faixa", alignRight: true))
            .AddColumn(Column("Comissão", alignRight: true, footer: Paint(Gold, Format.Currency(seller.TotalCommission))));

        foreach (var (line, index) in seller.Sales.Select((line, index) => (line, index + 1)))
        {
            table.AddRow(
                Paint(Muted, index),
                Paint(Bright, Format.Currency(line.Sale.Amount)),
                RateLabel(line.Rate),
                line.Commission == 0 ? Paint(Muted, "—") : Paint(Bright, Format.Currency(line.Commission)));
        }

        console.Indented(table);
        console.WaitForBack();
    }

    private Table SummaryTable()
    {
        var table = NewTable()
            .ShowFooters()
            .AddColumn(Column("Vendedor", footer: Paint(Soft, "Total")))
            .AddColumn(Column("Vendas", alignRight: true, footer: Paint(Soft, _summaries.Sum(s => s.Sales.Count))))
            .AddColumn(Column($"Por faixa ({string.Join(" · ", policy.Tiers.Select(t => Format.Percent(t.Rate)))})", alignRight: true))
            .AddColumn(Column("Total vendido", alignRight: true, footer: Paint(Soft, Format.Currency(_summaries.Sum(s => s.TotalSold)))))
            .AddColumn(Column("Comissão", alignRight: true, footer: Paint(Gold, Format.Currency(_summaries.Sum(s => s.TotalCommission)))));

        foreach (var seller in _summaries)
        {
            var salesPerTier = policy.Tiers.Select(tier => seller.Sales.Count(line => line.Rate == tier.Rate));

            table.AddRow(
                Paint(Bright, seller.Seller),
                Paint(Muted, seller.Sales.Count),
                Paint(Muted, string.Join(" · ", salesPerTier)),
                Paint(Bright, Format.Currency(seller.TotalSold)),
                Paint(Gold, Format.Currency(seller.TotalCommission)));
        }

        return table;
    }

    private string PolicyDescription()
    {
        var tiers = policy.Tiers;
        var rules = tiers.Select((tier, i) => i == 0 && tiers.Count > 1
            ? $"< {Format.Currency(tiers[1].MinimumAmount)} → {Format.Percent(tier.Rate)}"
            : $"≥ {Format.Currency(tier.MinimumAmount)} → {Format.Percent(tier.Rate)}");

        return string.Join("  ·  ", rules);
    }

    private string RateLabel(decimal rate)
    {
        var color = rate == 0 ? Muted : rate == policy.Tiers[^1].Rate ? Gold : Soft;
        return Paint(color, Format.Percent(rate));
    }
}
