using Gateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Gateway.Endpoints;

public static class NotificationsEndpoints
{
    public static void RegisterNotificationsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/notifications").WithTags("Notifications");

        group.MapGet("/",
        [AllowAnonymous]
        async Task<Results<Ok, Conflict>> (INotificationsService service) =>
        {
            return TypedResults.Ok();
        });
    }
}
