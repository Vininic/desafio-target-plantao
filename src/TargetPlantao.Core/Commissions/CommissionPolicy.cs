namespace TargetPlantao.Core.Commissions;

public sealed class CommissionPolicy
{
    private readonly CommissionTier[] _tiers;

    public CommissionPolicy(IEnumerable<CommissionTier> tiers)
    {
        _tiers = [.. tiers.OrderBy(t => t.MinimumAmount)];

        if (_tiers.Length == 0 || _tiers[0].MinimumAmount != 0m)
            throw new ArgumentException("A política precisa de uma faixa a partir de R$ 0,00.", nameof(tiers));

        if (_tiers.DistinctBy(t => t.MinimumAmount).Count() != _tiers.Length)
            throw new ArgumentException("Duas faixas não podem começar no mesmo valor.", nameof(tiers));
    }

    public static CommissionPolicy Default { get; } = new(
    [
        new CommissionTier(MinimumAmount: 0m, Rate: 0.00m),
        new CommissionTier(MinimumAmount: 100m, Rate: 0.01m),
        new CommissionTier(MinimumAmount: 500m, Rate: 0.05m),
    ]);

    public IReadOnlyList<CommissionTier> Tiers => _tiers;

    public CommissionTier TierFor(decimal saleAmount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(saleAmount);
        return _tiers.Last(t => saleAmount >= t.MinimumAmount);
    }
}
