using Gateway.DTO.Income;
using Gateway.DTO.Outcome;
using Gateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace Gateway.Endpoints;

public static class UsersEndpoints
{
    public static void RegisterUsersEndpoints(this WebApplication app)
    {
        var usersGroup = app.MapGroup("/user").WithTags("Users");

        usersGroup.MapPost("/register", RegisterUser);

        [AllowAnonymous]
        async static Task<Results<Ok, Conflict>> RegisterUser([Required] RegistrationRequest? request, IAuthService service)
        {
            try
            {
                await service.RegisterUser(request!);
                return TypedResults.Ok();
            }
            catch (ArgumentOutOfRangeException)
            {
                return TypedResults.Conflict();
            }
        }

        usersGroup.MapPost("/login",
        [AllowAnonymous]
        async Task<Results<Ok<TokensBundleResponse>, UnauthorizedHttpResult, NotFound>> ([Required] RegistrationRequest? request, IAuthService service) =>
        {
            try
            {
                var tokens = await service.LoginUser(request!);
                return TypedResults.Ok(tokens);
            }
            catch (UnauthorizedAccessException)
            {
                return TypedResults.Unauthorized();
            }
            catch (KeyNotFoundException)
            {
                return TypedResults.NotFound();
            }
        });

        usersGroup.MapPut("/",
        [Authorize]
        async Task<Results<Ok, Conflict>> (HttpContext context, UserUpdateRequest request, IUserService service) =>
        {
            try
            {
                var id = context.User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value!;
                Guid userId = new Guid(id);

                UserDemoUpdateRequest demoUpdateRequest = request.ToDemoUpdateRequest(userId);
                await service.Update(demoUpdateRequest);

                return TypedResults.Ok();
            }
            catch (ArgumentOutOfRangeException)
            {
                return TypedResults.Conflict();
            }
        });

        usersGroup.MapGet("/",
        [Authorize]
        async Task<Results<Ok<UserResponse>, NotFound>> (HttpContext context, IUserService service) =>
        {
            try
            {
                var claims = context.User.Claims.ToList();

                var id = context.User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value!;
                Guid userId = new Guid(id);

                var response = await service.Get(userId);
                return TypedResults.Ok(response);
            }
            catch (Exception)
            {
                return TypedResults.NotFound();
            }


        });

    }
}
