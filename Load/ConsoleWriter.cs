using System.Collections.Concurrent;

namespace Load;

internal static class ConsoleWriter
{
    private static readonly ConcurrentQueue<string> _messages = new();
    private static readonly System.Timers.Timer _timer = new()
    {
        Interval = 1000,
        AutoReset = true,
        Enabled = true,
    };

    private static int executionCount = 0;

    static ConsoleWriter() 
    {
        _timer.Elapsed += (sender, args) =>
        {
            while (_messages.TryDequeue(out string? message))
            {
                if (message != null)
                {
                    var count = Interlocked.Increment(ref executionCount);
                    Console.WriteLine($"#{count}  {message}");
                }
            }
        };
    }

    public static void WriteLine(string message)
    {
        _messages.Enqueue(message);
    }
}
