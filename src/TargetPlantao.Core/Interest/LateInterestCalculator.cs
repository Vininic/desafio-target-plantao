using TargetPlantao.Core.Common;

namespace TargetPlantao.Core.Interest;

public sealed record LateInterestQuote(
    decimal Amount,
    DateOnly DueDate,
    DateOnly ReferenceDate,
    int DaysOverdue,
    decimal DailyRate,
    decimal Interest)
{
    public bool IsOverdue => DaysOverdue > 0;

    public decimal UpdatedAmount => Amount + Interest;
}

public sealed class LateInterestCalculator
{
    public const decimal DefaultDailyRate = 0.025m;

    private readonly TimeProvider _clock;

    public LateInterestCalculator(TimeProvider clock, decimal dailyRate = DefaultDailyRate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(dailyRate);

        _clock = clock;
        DailyRate = dailyRate;
    }

    public decimal DailyRate { get; }

    public DateOnly Today => DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);

    public LateInterestQuote Calculate(decimal amount, DateOnly dueDate)
    {
        if (amount <= 0)
            throw new DomainException(new DomainError.NonPositiveAmount());

        var today = Today;
        var daysOverdue = Math.Max(0, today.DayNumber - dueDate.DayNumber);
        var interest = Money.RoundToCents(amount * DailyRate * daysOverdue);

        return new LateInterestQuote(amount, dueDate, today, daysOverdue, DailyRate, interest);
    }
}
