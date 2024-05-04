using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using BillingService.Helpers;
using BillingService.ConfigOptions;
using BillingService.Contexts;
using BillingService.Repositories;
using BillingService.HostedServices;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5076); });

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
builder.Services.AddSingleton<IAmountRepository, AmountRepository>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<BillingDbContext>(options =>
{
    options.UseNpgsql();
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddHostedService<KafkaHostedService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"./{AssemblyInfo.AssemblyName}/swagger.json", AssemblyInfo.AssemblyName);
    options.DocumentTitle = $"{AssemblyInfo.ProgramNameVersion} manual";
});
app.UseDeveloperExceptionPage();

app.MapGet("/amount/{userId}", async (Guid userId, HttpContext context, IAmountRepository service) =>
{
    decimal total = await service.GetUserAmount(userId);
    return Results.Ok(total);
});

app.MapGet("/items", async (BillingDbContext context) =>
{
    var response = await context.Amounts.ToArrayAsync();
    return Results.Ok(response);
});

app.Run();