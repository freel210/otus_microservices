namespace Demo.Services;

public interface IMetricsService
{
    void TakeErrorIntoAccount(string methodName);
}
