using TargetPlantao.Core.Common;

namespace TargetPlantao.Core.Commissions;

public sealed record SaleCommission(Sale Sale, decimal Rate, decimal Commission);

public sealed record SellerCommission(string Seller, IReadOnlyList<SaleCommission> Sales)
{
    public decimal TotalSold => Sales.Sum(s => s.Sale.Amount);

    public decimal TotalCommission => Sales.Sum(s => s.Commission);
}

public sealed class CommissionCalculator(CommissionPolicy policy)
{
    public SaleCommission Calculate(Sale sale)
    {
        var rate = policy.TierFor(sale.Amount).Rate;

        return new SaleCommission(sale, rate, Money.RoundToCents(sale.Amount * rate));
    }

    public IReadOnlyList<SellerCommission> CalculateBySeller(IEnumerable<Sale> sales) =>
    [
        .. sales
            .Select(Calculate)
            .GroupBy(c => c.Sale.Seller)
            .Select(group => new SellerCommission(group.Key, [.. group]))
            .OrderByDescending(s => s.TotalCommission),
    ];
}
