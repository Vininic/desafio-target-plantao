using TargetPlantao.Core.Common;
using TargetPlantao.Core.Interest;

namespace TargetPlantao.Tests.Core.Interest;

public class LateInterestCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 10, 3);

    private readonly LateInterestCalculator _calculator = new(ClockAt(Today));

    [Fact]
    public void Overdue_amount_accrues_simple_interest_per_day()
    {
        var quote = _calculator.Calculate(1000m, Today.AddDays(-10));

        Assert.Equal(10, quote.DaysOverdue);
        Assert.Equal(250m, quote.Interest);
        Assert.Equal(1250m, quote.UpdatedAmount);
        Assert.True(quote.IsOverdue);
    }

    [Fact]
    public void Interest_is_rounded_to_cents()
    {
        var quote = _calculator.Calculate(123.45m, Today.AddDays(-3));

        Assert.Equal(9.26m, quote.Interest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    public void Amount_not_yet_overdue_accrues_no_interest(int daysUntilDue)
    {
        var quote = _calculator.Calculate(500m, Today.AddDays(daysUntilDue));

        Assert.Equal(0, quote.DaysOverdue);
        Assert.Equal(0m, quote.Interest);
        Assert.Equal(500m, quote.UpdatedAmount);
        Assert.False(quote.IsOverdue);
    }

    [Fact]
    public void Days_are_counted_across_months_and_leap_years()
    {
        var calculator = new LateInterestCalculator(ClockAt(new DateOnly(2028, 3, 1)));

        var quote = calculator.Calculate(100m, new DateOnly(2028, 2, 27));

        Assert.Equal(3, quote.DaysOverdue);
    }

    [Fact]
    public void Daily_rate_is_configurable()
    {
        var calculator = new LateInterestCalculator(ClockAt(Today), dailyRate: 0.01m);

        Assert.Equal(20m, calculator.Calculate(1000m, Today.AddDays(-2)).Interest);
    }

    [Fact]
    public void Quote_uses_the_injected_clock_as_today()
    {
        Assert.Equal(Today, _calculator.Calculate(1m, Today).ReferenceDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Non_positive_amount_is_rejected(int amount)
    {
        var error = Assert.Throws<DomainException>(() => _calculator.Calculate(amount, Today));

        Assert.IsType<DomainError.NonPositiveAmount>(error.Error);
    }

    private static TimeProvider ClockAt(DateOnly date) =>
        TestData.ClockAt(new DateTimeOffset(date.ToDateTime(new TimeOnly(9, 0)), TimeSpan.Zero));
}
