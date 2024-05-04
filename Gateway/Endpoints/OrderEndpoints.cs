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
        async Task<Results<Ok, Conflict<string>>> (HttpContext context, IPurchaseService service) =>
        {
            var id = context.User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value!;
            Guid userId = new Guid(id);

            try
            {
                bool isOk = await service.AddItemBasket(userId);
                return isOk ? TypedResults.Ok() : TypedResults.Conflict("false");
            }
            catch (Exception ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        group.MapPost("/remove-item-basket",
        [Authorize]
        async Task<Results<Ok, Conflict<string>>> (HttpContext context, IPurchaseService service) =>
        {
            var id = context.User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value!;
            Guid userId = new Guid(id);

            try
            {
                bool isOk = await service.RemoveItemBasket(userId);
                return isOk ? TypedResults.Ok() : TypedResults.Conflict("false");
            }
            catch (Exception ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        group.MapGet("/",
        [Authorize]
        async Task<Results<Ok<int>, Conflict<string>>> (HttpContext context, IPurchaseService service) =>
        {
            try
            {
                var id = context.User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value!;
                Guid userId = new Guid(id);

                var result = await service.GetItemsBasket(userId);

                return TypedResults.Ok(result);
            }
            catch (Exception ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });

        group.MapPost("/buy",
        [Authorize]
        async Task<Results<Ok, Conflict<string>>> (HttpContext context, IPurchaseService service) =>
        {
            try
            {
                var id = context.User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value!;
                Guid userId = new Guid(id);

                var isOk = await service.Buy(userId);
                return isOk ? TypedResults.Ok() : TypedResults.Conflict("false");
            }
            catch (Exception ex)
            {
                return TypedResults.Conflict(ex.Message);
            }          
        });

        group.MapGet("/transactions",
        async Task<Results<Ok<IReadOnlyList<DistributedTransaction>>, Conflict<string>>> (ITransactionRepository repository) =>
        {
            try
            {
                var response = await repository.GetAll();
                return TypedResults.Ok(response);
            }
            catch (Exception ex)
            {
                return TypedResults.Conflict(ex.Message);
            }
        });
    }
}
