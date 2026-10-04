using TargetPlantao.Core.Commissions;

namespace TargetPlantao.Tests.Core.Commissions;

public class CommissionPolicyTests
{
    [Theory]
    [InlineData("0", "0")]
    [InlineData("99.99", "0")]
    [InlineData("100.00", "0.01")]
    [InlineData("499.99", "0.01")]
    [InlineData("500.00", "0.05")]
    [InlineData("2100.40", "0.05")]
    public void Default_policy_applies_the_rate_of_the_tier_the_sale_falls_into(string amount, string expectedRate)
    {
        var tier = CommissionPolicy.Default.TierFor(Dec(amount));

        Assert.Equal(Dec(expectedRate), tier.Rate);
    }

    [Fact]
    public void Negative_sale_amount_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CommissionPolicy.Default.TierFor(-0.01m));
    }

    [Fact]
    public void Policy_must_cover_sales_starting_at_zero()
    {
        Assert.Throws<ArgumentException>(() => new CommissionPolicy([new CommissionTier(100m, 0.01m)]));
    }

    [Fact]
    public void Policy_rejects_two_tiers_with_the_same_minimum()
    {
        Assert.Throws<ArgumentException>(() => new CommissionPolicy(
            [new CommissionTier(0m, 0m), new CommissionTier(100m, 0.01m), new CommissionTier(100m, 0.02m)]));
    }

    [Fact]
    public void Tiers_can_be_declared_in_any_order()
    {
        var policy = new CommissionPolicy([new CommissionTier(1000m, 0.1m), new CommissionTier(0m, 0m)]);

        Assert.Equal(0.1m, policy.TierFor(1500m).Rate);
        Assert.Equal(0m, policy.TierFor(999.99m).Rate);
    }
}
