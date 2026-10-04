using System.Text.Json;

namespace TargetPlantao.Core.Common;

internal static class JsonData
{
    public static T Deserialize<T>(Stream json) =>
        JsonSerializer.Deserialize<T>(json)
        ?? throw new InvalidDataException("O arquivo JSON está vazio.");
}
