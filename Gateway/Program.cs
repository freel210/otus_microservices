using Gateway.Authentication;
using Gateway.ConfigOptions;
using Gateway.Contexts;
using Gateway.DTO.Income;
using Gateway.Helpers;
using Gateway.Repositories;
using Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5168); });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
    AssemblyInfo.AssemblyName,
    new OpenApiInfo
    {
        Title = $"{AssemblyInfo.ProgramNameVersion} manual",
    });

    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{AssemblyInfo.AssemblyName}.xml"), true);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Authorization: Bearer JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });

    options.SupportNonNullableReferenceTypes();
});

builder.Services.AddOptions<ApiPointsOptions>().BindConfiguration("ApiPointsOptions");

builder.Services.AddOptions<PublicKeyOptions>().BindConfiguration("PublicKeyOptions");
builder.Services.AddSingleton<IPublicKeyRepository, PublicKeyRepository>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IKafkaService, KafkaService>();
builder.Services.AddScoped<IBillingService, BillingService>();

builder.Services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, TokenValidatorPostConfigure>();
builder.Services.AddScoped<JwtBearerEventsHandler>();

builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.IncludeErrorDetails = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = false,
        ValidateAudience = true,
        ValidateIssuer = false,
        ValidateLifetime = false,
        ValidAudience = "otus_microservices_aud",
        ClockSkew = TimeSpan.Zero,
    };

    o.EventsType = typeof(JwtBearerEventsHandler);
});

builder.Services.AddAuthorization();

builder.Services.AddOptions<PostgresOptions>().BindConfiguration("PostgresOptions");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<GatewayDbContext>(options =>
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

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/register", [AllowAnonymous] async ([Required] RegistrationRequest? request, IAuthService service) =>
{
    try
    {
        await service.RegisterUser(request!);
        return Results.Ok();
    }
    catch (ArgumentOutOfRangeException)
    {
        return Results.Conflict();
    }
});

app.MapPost("/login", [AllowAnonymous] async ([Required] RegistrationRequest? request, IAuthService service) =>
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

app.MapPut("/user", [Authorize] async (HttpContext context, UserUpdateRequest request, IUserService service) =>
{
    try
    {
        var id = context.User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value!;
        Guid userId = new Guid(id);

        UserDemoUpdateRequest demoUpdateRequest = request.ToDemoUpdateRequest(userId);
        await service.Update(demoUpdateRequest);

        return Results.Ok();
    }
    catch (ArgumentOutOfRangeException)
    {
        return Results.Conflict();
    }
});

app.MapGet("/user", [Authorize] async (HttpContext context, IUserService service) =>
{
    var claims = context.User.Claims.ToList();

    var id = context.User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value!;
    Guid userId = new Guid(id);

    var response = await service.Get(userId);
    return Results.Ok(response);
});

app.MapGet("/buy", [AllowAnonymous] async (IPurchaseService service) =>
{
    var result = await service.Buy();

    if (result == true)
    {
        return Results.Ok();
    }

    return Results.Conflict();
});

app.MapGet("/buy-error", [AllowAnonymous] async (IPurchaseService service) =>
{
    var result = await service.BuyError();

    if (result == true)
    {
        return Results.Ok();
    }

    return Results.Conflict();
});

app.MapGet("/transactions", async (GatewayDbContext context) =>
{
    var response = await context.Transactions.ToArrayAsync();
    return Results.Ok(response);
});

app.MapPost("/put-money", [Authorize] async (HttpContext context, PutMoneyRequest request, IBillingService service) =>
{
    var claims = context.User.Claims.ToList();

    var id = context.User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value!;
    Guid userId = new Guid(id);

    var result = await service.PutMoney(userId, request.Ammount);

    if (result == true)
    {
        return Results.Ok();
    }

    return Results.Conflict();
});

app.Run();
