using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrdersService.ConfigOptions;
using OrdersService.Contexts;
using OrdersService.DTO.Income;
using OrdersService.Helpers;
using OrdersService.HostedServices;
using OrdersService.Repositories;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5074); });

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

builder.Services.AddOptions<PostgresOptions>().BindConfiguration("PostgresOptions");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<OrdersDbContext>(options =>
{
    options.UseNpgsql();
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddSingleton<IBasketItemRepository, BasketItemRepository>();
builder.Services.AddHostedService<KafkaHostedService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"./{AssemblyInfo.AssemblyName}/swagger.json", AssemblyInfo.AssemblyName);
    options.DocumentTitle = $"{AssemblyInfo.ProgramNameVersion} manual";
});
app.UseDeveloperExceptionPage();

app.MapPost("/add", async ([Required] BasketItemRequest? request, IBasketItemRepository repository) =>
{
    await repository.Add(request!.UserId);
    return TypedResults.Ok();
});

app.MapPost("/remove", async ([Required] BasketItemRequest? request, IBasketItemRepository repository) =>
{
    await repository.Remove(request!.UserId);
    return TypedResults.Ok();
});

app.MapGet("/item/{userId}", async ([Required] Guid? userId, IBasketItemRepository repository) =>
{
    int count = await repository.GetItemsQuantity(userId!.Value);
    return TypedResults.Ok(count);
});

app.MapGet("/items", async (IBasketItemRepository repository) =>
{
    var response = await repository.GetAll();
    return TypedResults.Ok(response);
});

app.Run();