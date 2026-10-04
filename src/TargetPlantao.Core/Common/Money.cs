namespace TargetPlantao.Core.Common;

public static class Money
{
    public static decimal RoundToCents(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
