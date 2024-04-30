using System.Diagnostics.Metrics;

namespace UsersService.Services;

public class MetricsService : IMetricsService
{
    private readonly Counter<int> _500Count;

    public MetricsService(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("UserService");
        _500Count = meter.CreateCounter<int>("500.count");
    }

    public void TakeErrorIntoAccount(string methodName)
    {
        _500Count.Add(1, new KeyValuePair<string, object?>("500.count", methodName));
    }
}
