using Gateway.Entities;
using Gateway.Repositories;
using Gateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Gateway.Endpoints;

public static class OrderEndpoints
{
    public static void RegisterOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/order").WithTags("Order");

        group.MapPost("/add-item-basket",
        [Authorize]
        async Task<Results<Ok, Conflict>> (IPurchaseService service) =>
        {
            return TypedResults.Ok();
        });

        group.MapPost("/remove-item-basket",
        [Authorize]
        async Task<Results<Ok, Conflict>> (IPurchaseService service) =>
        {
            return TypedResults.Ok();
        });

        group.MapGet("/",
        [AllowAnonymous]
        async Task<Results<Ok, Conflict>> (IPurchaseService service) =>
        {
            return TypedResults.Ok();
        });

        group.MapPost("/buy",
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

        group.MapGet("/transactions",
        async Task<Results<Ok<IReadOnlyList<DistributedTransaction>>, NotFound>> (ITransactionRepository repository) =>
        {
            var response = await repository.GetAll();
            return TypedResults.Ok(response);
        });
    }
}
