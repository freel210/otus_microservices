using DeliveriesService.ConfigOptions;
using DeliveriesService.Contexts;
using DeliveriesService.DTO.Income;
using DeliveriesService.Entities;
using DeliveriesService.Helpers;
using DeliveriesService.HostedServices;
using DeliveriesService.Repositories;
using DeliveriesService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5073); });

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
builder.Services.AddDbContext<DeliveryDbContext>(options =>
{
    options.UseNpgsql();
});

builder.Services.AddHostedService<KafkaConsumerHostedService>();
builder.Services.AddSingleton<IKafkaService, KafkaService>();

builder.Services.AddSingleton<IDeliveryItemService, DeliveryItemService>();
builder.Services.AddSingleton<IDeliveryRepository, DeliveryRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"./{AssemblyInfo.AssemblyName}/swagger.json", AssemblyInfo.AssemblyName);
    options.DocumentTitle = $"{AssemblyInfo.ProgramNameVersion} manual";
});
app.UseDeveloperExceptionPage();

app.MapGet("/deliveries", async (DeliveryDbContext context) =>
{
    var response = await context.Deliveries.ToArrayAsync();
    return Results.Ok(response);
});

app.Run();