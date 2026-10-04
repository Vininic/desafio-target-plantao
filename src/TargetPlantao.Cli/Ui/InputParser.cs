using System.Globalization;
using System.Text.RegularExpressions;

namespace TargetPlantao.Cli.Ui;

internal static partial class InputParser
{
    public delegate bool TryParse<T>(string? input, out T value);

    private static readonly string[] DateFormats = ["dd/MM/yyyy", "d/M/yyyy"];

    public static bool TryParseMoney(string? input, out decimal value)
    {
        value = 0;
        var text = (input ?? "").Replace("R$", "", StringComparison.OrdinalIgnoreCase).Trim();

        var culture = DotAsDecimalSeparator().IsMatch(text) ? CultureInfo.InvariantCulture : Format.PtBr;
        if (!decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, culture, out var parsed))
            return false;

        if (parsed <= 0 || decimal.Round(parsed, 2) != parsed)
            return false;

        value = parsed;
        return true;
    }

    public static bool TryParseQuantity(string? input, out int quantity) =>
        int.TryParse((input ?? "").Trim(), NumberStyles.None, Format.PtBr, out quantity) && quantity > 0;

    public static bool TryParseText(string? input, out string text)
    {
        text = (input ?? "").Trim();
        return text.Length > 0;
    }

    public static bool TryParseDate(string? input, out DateOnly date) =>
        DateOnly.TryParseExact((input ?? "").Trim(), DateFormats, Format.PtBr, DateTimeStyles.None, out date);

    [GeneratedRegex(@"^\d+\.\d{1,2}$")]
    private static partial Regex DotAsDecimalSeparator();
}
