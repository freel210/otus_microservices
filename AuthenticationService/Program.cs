using AuthenticationService.ConfigOptions;
using AuthenticationService.Contexts;
using AuthenticationService.DTO.Income;
using AuthenticationService.Helpers;
using AuthenticationService.Repositories;
using AuthenticationService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5068); });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
    AssemblyInfo.AssemblyName,
    new OpenApiInfo
    {
        Title = $"{AssemblyInfo.AssemblyName}",
    });

    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{AssemblyInfo.AssemblyName}.xml"), true);

    options.SupportNonNullableReferenceTypes();
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

builder.Services.AddOptions<PrivateKeyOptions>().BindConfiguration("PrivateKeyOptions");
builder.Services.AddSingleton<IPrivateKeyRepository, PrivateKeyRepository>();

builder.Services.AddOptions<JwtServiceOptions>().BindConfiguration("JwtServiceOptions");
builder.Services.AddSingleton<IJwtService, JwtService>();

builder.Services.AddOptions<PostgresOptions>().BindConfiguration("PostgresOptions");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql();
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"./{AssemblyInfo.AssemblyName}/swagger.json", AssemblyInfo.AssemblyName);
    options.DocumentTitle = $"{AssemblyInfo.ProgramNameVersion} manual";
});
app.UseDeveloperExceptionPage();

app.MapPost("/register", async ([Required]RegistrationRequest? request, IAuthService service) =>
{
    try
    {
        var userId = await service.RegisterUser(request!);
        return Results.Ok(new { UserId = userId });
    }
    catch (ArgumentOutOfRangeException)
    {
        return Results.Conflict();
    }
});

app.MapPost("/login", async ([Required] RegistrationRequest? request, IAuthService service) =>
{
    try
    {
        var tokens = await service.LoginUser(request!);
        return Results.Ok(tokens);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});

app.MapGet("/auths", async (IAuthService service) =>
{
    var response = await service.GetAll();
    return Results.Ok(response);
});

app.Run();