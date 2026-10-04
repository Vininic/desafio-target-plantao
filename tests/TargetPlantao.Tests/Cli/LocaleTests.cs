using System.Reflection;
using TargetPlantao.Cli.Localization;
using TargetPlantao.Core.Common;

namespace TargetPlantao.Tests.Cli;

public class LocaleTests
{
    [Theory]
    [InlineData("1500", "1500")]
    [InlineData("1500,5", "1500.5")]
    [InlineData("1.500,50", "1500.50")]
    [InlineData("R$ 1.500,50", "1500.50")]
    [InlineData("  250,00 ", "250.00")]
    [InlineData("1500.50", "1500.50")]
    [InlineData("1.500", "1500")]
    public void Portuguese_money_input(string input, string expected)
    {
        Assert.True(Locale.Portuguese.TryParseMoney(input, out var value));
        Assert.Equal(Dec(expected), value);
    }

    [Theory]
    [InlineData("1500", "1500")]
    [InlineData("1,500.50", "1500.50")]
    [InlineData("R$ 1,500.50", "1500.50")]
    [InlineData("1500,50", "1500.50")]
    [InlineData("1,500", "1500")]
    public void English_money_input(string input, string expected)
    {
        Assert.True(Locale.English.TryParseMoney(input, out var value));
        Assert.Equal(Dec(expected), value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-10")]
    [InlineData("10,005")]
    [InlineData("1,234")]
    public void Invalid_money_is_rejected(string input)
    {
        Assert.False(Locale.Portuguese.TryParseMoney(input, out _));
    }

    [Theory]
    [InlineData("03/10/2026", 2026, 10, 3)]
    [InlineData("3/1/2026", 2026, 1, 3)]
    public void Portuguese_date_is_day_month_year(string input, int year, int month, int day)
    {
        Assert.True(Locale.Portuguese.TryParseDate(input, out var date));
        Assert.Equal(new DateOnly(year, month, day), date);
    }

    [Theory]
    [InlineData("10/03/2026", 2026, 10, 3)]
    [InlineData("1/3/2026", 2026, 1, 3)]
    public void English_date_is_month_day_year(string input, int year, int month, int day)
    {
        Assert.True(Locale.English.TryParseDate(input, out var date));
        Assert.Equal(new DateOnly(year, month, day), date);
    }

    [Theory]
    [InlineData("31/02/2026")]
    [InlineData("2026-10-03")]
    [InlineData("10/2026")]
    public void Invalid_date_is_rejected(string input)
    {
        Assert.False(Locale.Portuguese.TryParseDate(input, out _));
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("250", true)]
    [InlineData("0", false)]
    [InlineData("-3", false)]
    [InlineData("2,5", false)]
    public void Quantity_must_be_a_positive_integer(string input, bool valid)
    {
        Assert.Equal(valid, Locale.Portuguese.TryParseQuantity(input, out _));
    }

    [Fact]
    public void Values_are_formatted_in_each_language()
    {
        Assert.Equal("R$ 1.200,50", Locale.Portuguese.Currency(1200.50m));
        Assert.Equal("R$ 1,200.50", Locale.English.Currency(1200.50m));
        Assert.Equal("2,5%", Locale.Portuguese.Percent(0.025m));
        Assert.Equal("2.5%", Locale.English.Percent(0.025m));
        Assert.Equal("03/10/2026", Locale.Portuguese.Date(new DateOnly(2026, 10, 3)));
        Assert.Equal("10/03/2026", Locale.English.Date(new DateOnly(2026, 10, 3)));
    }

    [Theory]
    [InlineData("pt")]
    [InlineData("en")]
    public void Every_domain_error_has_a_message(string language)
    {
        var locale = language == "en" ? Locale.English : Locale.Portuguese;
        var errors = typeof(DomainError)
            .GetNestedTypes()
            .Where(type => type.IsSubclassOf(typeof(DomainError)))
            .Select(type => (DomainError)CreateSample(type));

        Assert.All(errors, error => Assert.False(string.IsNullOrWhiteSpace(locale.Text.Error(error))));
    }

    private static object CreateSample(Type type)
    {
        var constructor = type.GetConstructors().Single();
        var arguments = constructor.GetParameters()
            .Select(p => p.ParameterType == typeof(string) ? "x" : Activator.CreateInstance(p.ParameterType))
            .ToArray();

        return constructor.Invoke(BindingFlags.Default, null, arguments, CultureInfo.InvariantCulture);
    }
}
