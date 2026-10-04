using System.Globalization;
using System.Text.RegularExpressions;

namespace TargetPlantao.Cli.Localization;

internal sealed partial class Locale
{
    private readonly string[] _dateFormats;

    private Locale(string cultureName, string[] dateFormats, Strings text)
    {
        Culture = CultureInfo.GetCultureInfo(cultureName);
        _dateFormats = dateFormats;
        Text = text;
    }

    public static Locale Portuguese { get; } = new("pt-BR", ["dd/MM/yyyy", "d/M/yyyy"], Strings.Portuguese);

    public static Locale English { get; } = new("en-US", ["MM/dd/yyyy", "M/d/yyyy"], Strings.English);

    public CultureInfo Culture { get; }

    public Strings Text { get; }

    public string Currency(decimal value) => "R$ " + value.ToString("N2", Culture);

    public string Percent(decimal rate) => (rate * 100).ToString("0.##", Culture) + "%";

    public string Date(DateOnly date) => date.ToString(_dateFormats[0], Culture);

    public string Time(DateTimeOffset moment) => moment.ToString("HH:mm:ss", Culture);

    public string Units(int quantity) => $"{quantity.ToString("N0", Culture)} {Text.Unit}";

    public bool TryParseMoney(string? input, out decimal value)
    {
        value = 0;
        var text = (input ?? "").Replace("R$", "", StringComparison.OrdinalIgnoreCase).Trim();

        var native = Culture.NumberFormat.NumberDecimalSeparator;
        if (ShortFraction().Match(text) is { Success: true } match && match.Groups[1].Value != native)
            text = text.Replace(match.Groups[1].Value, native, StringComparison.Ordinal);

        if (!decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, Culture, out var parsed))
            return false;

        if (parsed <= 0 || decimal.Round(parsed, 2) != parsed)
            return false;

        value = parsed;
        return true;
    }

    public bool TryParseDate(string? input, out DateOnly date) =>
        DateOnly.TryParseExact((input ?? "").Trim(), _dateFormats, Culture, DateTimeStyles.None, out date);

    public bool TryParseQuantity(string? input, out int quantity) =>
        int.TryParse((input ?? "").Trim(), NumberStyles.None, Culture, out quantity) && quantity > 0;

    public static bool TryParseText(string? input, out string text)
    {
        text = (input ?? "").Trim();
        return text.Length > 0;
    }

    [GeneratedRegex(@"^\d+([.,])\d{1,2}$")]
    private static partial Regex ShortFraction();
}
