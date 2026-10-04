using TargetPlantao.Cli.Screens;
using Spectre.Console;
using Spectre.Console.Rendering;
using static TargetPlantao.Cli.Ui.Theme;

namespace TargetPlantao.Cli.Ui;

internal sealed class MainMenu(Terminal terminal, IReadOnlyList<IScreen> screens)
{
    private const string About = "Vinícius Nicoluci Espíndola  ·  github.com/Vininic  ·  C# / .NET 8";

    public void Run()
    {
        var selected = 0;

        while (Choose(selected) is { } choice)
        {
            selected = choice;
            screens[choice].Show();
        }

        terminal.Console.Clear();
    }

    private int? Choose(int initial)
    {
        var exitIndex = screens.Count;
        var index = initial;
        int? chosen = null;

        terminal.Header();
        terminal.Console.Live(Render(index))
            .AutoClear(true)
            .Start(context =>
            {
                context.Refresh();

                while (chosen is null)
                {
                    if (terminal.Console.Input.ReadKey(intercept: true) is not { } key)
                        continue;

                    var letter = char.ToLowerInvariant(key.KeyChar);

                    if (key.Key == ConsoleKey.UpArrow || letter == 'k')
                        index = (index + exitIndex) % (exitIndex + 1);
                    else if (key.Key == ConsoleKey.DownArrow || letter == 'j')
                        index = (index + 1) % (exitIndex + 1);
                    else if (key.Key == ConsoleKey.Enter)
                        chosen = index;
                    else if (letter == 'l')
                        terminal.ToggleLanguage();
                    else if (key.Key == ConsoleKey.Escape || letter == 'q')
                        chosen = exitIndex;
                    else if (letter - '1' is var shortcut && shortcut >= 0 && shortcut < exitIndex)
                        chosen = shortcut;

                    context.UpdateTarget(Render(chosen ?? index));
                }
            });

        return chosen == exitIndex ? null : chosen;
    }

    private Rows Render(int selected)
    {
        var options = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadLeft(2).PadRight(1))
            .AddColumn(new GridColumn().NoWrap())
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn(new GridColumn());

        for (var i = 0; i < screens.Count; i++)
        {
            var isSelected = i == selected;
            options.AddRow(
                isSelected ? Paint(Accent, "▸") : " ",
                Paint(isSelected ? Gold : Muted, i + 1),
                Paint(isSelected ? Soft : Bright, screens[i].Title),
                Paint(isSelected ? Bright : Muted, screens[i].Summary));
        }

        var exitSelected = selected == screens.Count;
        options.AddRow(exitSelected ? Paint(Accent, "▸") : " ", Paint(exitSelected ? Gold : Muted, "q"), Paint(exitSelected ? Soft : Muted, terminal.Text.Quit), "");

        IRenderable[] lines =
        [
            options,
            Text.Empty,
            new Markup($"  {Paint(Muted, terminal.Text.MenuHint(screens.Count))}"),
            Text.Empty,
            new Rule().RuleStyle(LineStyle),
            new Markup($"  {Paint(Muted, About)}"),
        ];

        return new Rows(lines);
    }
}
