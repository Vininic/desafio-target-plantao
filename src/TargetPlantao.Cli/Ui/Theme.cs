using Spectre.Console;

namespace TargetPlantao.Cli.Ui;

internal static class Theme
{
    public const string Accent = "#4A69C7";
    public const string Soft = "#A8B8F4";
    public const string Bright = "#EEF4FF";
    public const string Muted = "#7F8AA9";
    public const string Line = "#263A70";
    public const string Gold = "#F7D25C";
    public const string Danger = "#F2777A";

    public static readonly Style LineStyle = new(Color.FromHex(Line));
    public static readonly Style HighlightStyle = new(Color.FromHex(Bright), Color.FromHex(Line));

    public static string Paint(string color, object? text) => $"[{color}]{Markup.Escape(text?.ToString() ?? "")}[/]";

    public static Table NewTable() =>
        new Table().Border(TableBorder.Rounded).BorderStyle(LineStyle).HideFooters();

    public static TableColumn Column(string header, bool alignRight = false, string footer = "")
    {
        var column = new TableColumn(Paint(Soft, header)).Footer(footer);
        return alignRight ? column.RightAligned() : column;
    }
}
