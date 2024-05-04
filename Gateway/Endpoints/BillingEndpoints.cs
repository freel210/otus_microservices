using Gateway.DTO.Income;
using Gateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Gateway.Endpoints;

public static class BillingEndpoints
{
    public static void RegisterBillingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/billing").WithTags("Billing");

        group.MapPost("/put-money",
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

        group.MapGet("/",
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
