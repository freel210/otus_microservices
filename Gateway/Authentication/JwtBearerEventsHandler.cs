using Gateway.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Gateway.Authentication;

public class JwtBearerEventsHandler : JwtBearerEvents
{

    public JwtBearerEventsHandler()
    {
        Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
    }

    public override async Task MessageReceived(MessageReceivedContext context)
    {
        // regular authorization
        if (context.Request.Headers.Authorization.Any())
        {
            await base.MessageReceived(context);
        }

        // try set token from query string
        else if (context.Request.Query.TryGetValue("access_token", out var signalRQueryAccessToken))
        {
            context.Token = signalRQueryAccessToken;
        }

        await base.MessageReceived(context);
    }

    public override Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        return Task.CompletedTask;
    }

    public override Task Forbidden(ForbiddenContext context)
    {
        return Task.CompletedTask;
    }

    public override async Task Challenge(JwtBearerChallengeContext context)
    {
        context.HandleResponse();
        await Responses.Response401Unauthorized(context.HttpContext);
        await base.Challenge(context);
    }
}
