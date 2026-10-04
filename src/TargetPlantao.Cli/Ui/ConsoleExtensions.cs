using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace TargetPlantao.Cli.Ui;

internal static class ConsoleExtensions
{
    public const string AppName = "target_plantao";

    public static void Header(this IAnsiConsole console, params string[] path)
    {
        console.Clear();

        var trail = string.Concat(path.Select(segment => $" {Theme.Paint(Theme.Muted, "/")} {Theme.Paint(Theme.Soft, segment)}"));
        console.Write(new Rule($"{Theme.Paint(Theme.Accent, ">")} {Theme.Paint(Theme.Bright, AppName)}{trail}")
            .LeftJustified()
            .RuleStyle(Theme.LineStyle));
        console.WriteLine();
    }

    public static void Hint(this IAnsiConsole console, string text) =>
        console.MarkupLine($"  {Theme.Paint(Theme.Muted, text)}");

    public static void Success(this IAnsiConsole console, string text) =>
        console.MarkupLine($"  {Theme.Paint(Theme.Soft, "✓")} {Theme.Paint(Theme.Bright, text)}");

    public static void Error(this IAnsiConsole console, string text) =>
        console.MarkupLine($"  {Theme.Paint(Theme.Danger, "✗")} {Theme.Paint(Theme.Danger, text)}");

    public static void Indented(this IAnsiConsole console, IRenderable content) =>
        console.Write(new Padder(content, new Padding(2, 0, 0, 0)));

    public static bool TryChoose<T>(
        this IAnsiConsole console, IEnumerable<T> choices, Func<T, string> label, [MaybeNullWhen(false)] out T choice)
        where T : notnull
    {
        console.WriteLine();
        var selected = console.Prompt(
            new SelectionPrompt<Choice<T>>()
                .AddChoices(choices.Select(value => new Choice<T>(value)))
                .AddChoices(Choice<T>.Back)
                .AddCancelResult(Choice<T>.Back)
                .UseConverter(option => (option.IsBack ? Theme.Paint(Theme.Muted, "← voltar") : label(option.Value!)))
                .HighlightStyle(Theme.HighlightStyle)
                .WrapAround(true)
                .PageSize(12));

        choice = selected.Value;
        return !selected.IsBack;
    }

    public static T Ask<T>(this IAnsiConsole console, string question, InputParser.TryParse<T> parse, string invalidMessage)
    {
        T result = default!;

        console.Prompt(
            new TextPrompt<string>($"  {Theme.Paint(Theme.Soft, "›")} {Theme.Paint(Theme.Bright, question)}")
                .PromptStyle(new Style(Color.FromHex(Theme.Gold)))
                .Validate(answer => parse(answer, out result)
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"  {Theme.Paint(Theme.Danger, invalidMessage)}")));

        return result;
    }

    public static void WaitForBack(this IAnsiConsole console) =>
        console.TryChoose<string>([], label => label, out _);

    private sealed record Choice<T>(T? Value, bool IsBack = false)
    {
        public static readonly Choice<T> Back = new(default, IsBack: true);
    }
}
