namespace Gateway.Services
{
    public interface IPurchaseService
    {
        Task<bool> Buy();
        Task<bool> BuyError();
    }
}
