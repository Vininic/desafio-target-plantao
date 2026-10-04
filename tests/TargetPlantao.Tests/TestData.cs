using System.Text;
using Microsoft.Extensions.Time.Testing;

namespace TargetPlantao.Tests;

internal static class TestData
{
    public static Stream Open(string fileName) =>
        File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Data", fileName));

    public static Stream FromString(string json) =>
        new MemoryStream(Encoding.UTF8.GetBytes(json));

    public static FakeTimeProvider ClockAt(DateTimeOffset utcNow)
    {
        var clock = new FakeTimeProvider(utcNow);
        clock.SetLocalTimeZone(TimeZoneInfo.Utc);
        return clock;
    }

    public static decimal Dec(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);
}
