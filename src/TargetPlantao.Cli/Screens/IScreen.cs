namespace TargetPlantao.Cli.Screens;

internal interface IScreen
{
    string Title { get; }

    string Summary { get; }

    void Show();
}
