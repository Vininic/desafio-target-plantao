using System.Globalization;

namespace TargetPlantao.Cli.Ui;

internal static class Format
{
    public static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Currency(decimal value) => value.ToString("C2", PtBr);

    public static string Percent(decimal rate) => (rate * 100).ToString("0.##", PtBr) + "%";

    public static string Date(DateOnly date) => date.ToString("dd/MM/yyyy", PtBr);

    public static string Units(int quantity) => quantity.ToString("N0", PtBr) + " un";
}
