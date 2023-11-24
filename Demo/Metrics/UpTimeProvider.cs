using System.Diagnostics;

namespace Demo.Metrics;

public class UpTimeProvider
{
    private static readonly DateTime StartTime;

    static UpTimeProvider()
    {
        using var process = Process.GetCurrentProcess();
        StartTime = process.StartTime;
    }

    public static TimeSpan GetUpTime() => DateTime.Now - StartTime;
}
