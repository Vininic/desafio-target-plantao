using TargetPlantao.Cli.Ui;

namespace TargetPlantao.Tests.Cli;

public class InputParserTests
{
    [Theory]
    [InlineData("1500", "1500")]
    [InlineData("1500,5", "1500.5")]
    [InlineData("1.500,50", "1500.50")]
    [InlineData("R$ 1.500,50", "1500.50")]
    [InlineData("  250,00 ", "250.00")]
    [InlineData("1500.50", "1500.50")]
    [InlineData("1.500", "1500")]
    public void Money_accepts_brazilian_and_unambiguous_formats(string input, string expected)
    {
        Assert.True(InputParser.TryParseMoney(input, out var value));
        Assert.Equal(Dec(expected), value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-10")]
    [InlineData("10,005")]
    public void Money_rejects_invalid_values(string input)
    {
        Assert.False(InputParser.TryParseMoney(input, out _));
    }

    [Theory]
    [InlineData("03/10/2026", 2026, 10, 3)]
    [InlineData("3/1/2026", 2026, 1, 3)]
    public void Date_is_read_as_day_month_year(string input, int year, int month, int day)
    {
        Assert.True(InputParser.TryParseDate(input, out var date));
        Assert.Equal(new DateOnly(year, month, day), date);
    }

    [Theory]
    [InlineData("31/02/2026")]
    [InlineData("2026-10-03")]
    [InlineData("10/2026")]
    public void Date_rejects_invalid_or_foreign_formats(string input)
    {
        Assert.False(InputParser.TryParseDate(input, out _));
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("250", true)]
    [InlineData("0", false)]
    [InlineData("-3", false)]
    [InlineData("2,5", false)]
    public void Quantity_must_be_a_positive_integer(string input, bool valid)
    {
        Assert.Equal(valid, InputParser.TryParseQuantity(input, out _));
    }
}
