using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Rendering;
using TargetPlantao.Cli.Localization;
using static TargetPlantao.Cli.Ui.Theme;

namespace TargetPlantao.Cli.Ui;

internal delegate bool TryParse<T>(string? input, out T value);

internal sealed class Terminal(IAnsiConsole console, Locale locale)
{
    public const string AppName = "target_plantao";

    public IAnsiConsole Console => console;

    public Locale Locale { get; private set; } = locale;

    public Strings Text => Locale.Text;

    public void ToggleLanguage() =>
        Locale = Locale == Locale.Portuguese ? Locale.English : Locale.Portuguese;

    public void Header(params string[] path)
    {
        console.Clear();

        var trail = string.Concat(path.Select(segment => $" {Paint(Muted, "/")} {Paint(Soft, segment)}"));
        console.Write(new Rule($"{Paint(Accent, ">")} {Paint(Bright, AppName)}{trail}")
            .LeftJustified()
            .RuleStyle(LineStyle));
        console.WriteLine();
    }

    public void NewLine() => console.WriteLine();

    public void Line(string markup) => console.MarkupLine(markup);

    public void Hint(string text) => console.MarkupLine($"  {Paint(Muted, text)}");

    public void Success(string text) => console.MarkupLine($"  {Paint(Soft, "✓")} {Paint(Bright, text)}");

    public void Error(string text) => console.MarkupLine($"  {Paint(Danger, "✗")} {Paint(Danger, text)}");

    public void Show(IRenderable content) => console.Write(new Padder(content, new Padding(2, 0, 0, 0)));

    public bool TryChoose<T>(IEnumerable<T> choices, Func<T, string> label, [MaybeNullWhen(false)] out T choice)
        where T : notnull
    {
        console.WriteLine();
        var selected = console.Prompt(
            new SelectionPrompt<Choice<T>>()
                .AddChoices(choices.Select(value => new Choice<T>(value)))
                .AddChoices(Choice<T>.Back)
                .AddCancelResult(Choice<T>.Back)
                .UseConverter(option => option.IsBack ? Paint(Muted, Text.Back) : label(option.Value!))
                .HighlightStyle(HighlightStyle)
                .WrapAround(true)
                .PageSize(12));

        choice = selected.Value;
        return !selected.IsBack;
    }

    public T Ask<T>(string question, TryParse<T> parse, string invalidMessage)
    {
        T result = default!;

        console.Prompt(
            new TextPrompt<string>($"  {Paint(Soft, "›")} {Paint(Bright, question)}")
                .PromptStyle(new Style(Color.FromHex(Gold)))
                .Validate(answer => parse(answer, out result)
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"  {Paint(Danger, invalidMessage)}")));

        return result;
    }

    public void WaitForBack() => TryChoose<string>([], label => label, out _);

    private sealed record Choice<T>(T? Value, bool IsBack = false)
    {
        public static readonly Choice<T> Back = new(default, IsBack: true);
    }
}
