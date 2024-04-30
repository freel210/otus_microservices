namespace UsersService.Services;

public interface IMetricsService
{
    void TakeErrorIntoAccount(string methodName);
}
