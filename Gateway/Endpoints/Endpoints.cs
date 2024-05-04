using Gateway.DTO.Income;
using Gateway.Entities;
using Gateway.Repositories;
using Gateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Gateway.Endpoints;

public static class Endpoints
{
    public static void RegisterEndpoints(this WebApplication app)
    {
        app.MapGet("/buy",
        [AllowAnonymous]
        async Task<Results<Ok, Conflict>> (IPurchaseService service) =>
        {
            var result = await service.Buy();

            if (result == true)
            {
                return TypedResults.Ok();
            }

            return TypedResults.Conflict();
        });

        app.MapGet("/buy-error", 
        [AllowAnonymous] 
        async Task<Results<Ok, Conflict>> (IPurchaseService service) =>
        {
            var result = await service.BuyError();

            if (result == true)
            {
                return TypedResults.Ok();
            }

            return TypedResults.Conflict();
        });

        app.MapGet("/transactions",
        async Task<Results<Ok<IReadOnlyList<DistributedTransaction>>, NotFound>> (ITransactionRepository repository) =>
        {
            var response = await repository.GetAll();
            return TypedResults.Ok(response);
        });

        app.MapPost("/put-money",
        [Authorize] 
        async Task<Results<Ok, Conflict>> (HttpContext context, PutMoneyRequest request, IBillingService service) =>
        {
            var claims = context.User.Claims.ToList();

            var id = context.User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value!;
            Guid userId = new Guid(id);

            var result = await service.PutMoney(userId, request.Ammount);

            if (result == true)
            {
                return TypedResults.Ok();
            }

            return TypedResults.Conflict();
        });

        app.MapGet("/amount",
        [Authorize]
        async Task<Results<Ok<decimal>, NotFound>> (HttpContext context, IBillingService service) =>
        {
            var claims = context.User.Claims.ToList();

            var id = context.User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value!;
            Guid userId = new Guid(id);

            decimal total = await service.GetUserAmount(userId);
            return TypedResults.Ok(total);
        });
    }
}
