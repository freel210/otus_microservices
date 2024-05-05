using DeliveriesService.ConfigOptions;
using DeliveriesService.Contexts;
using DeliveriesService.DTO.Income;
using DeliveriesService.Entities;
using DeliveriesService.Helpers;
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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"./{AssemblyInfo.AssemblyName}/swagger.json", AssemblyInfo.AssemblyName);
    options.DocumentTitle = $"{AssemblyInfo.ProgramNameVersion} manual";
});
app.UseDeveloperExceptionPage();

app.MapPost("/add", async ([Required] DeliveryRequest? request, DeliveryDbContext context) =>
{
    Guid id = Guid.NewGuid();
    Delivery entity = new Delivery()
    {
        Id = id,
        Tid = request!.Tid,
        Status = true
    };

    await context.Deliveries.AddAsync(entity);
    context.SaveChanges();

    return Results.Ok(new { Id = id });
});

app.MapPost("/cancel", async ([Required] DeliveryRequest? request, DeliveryDbContext context) =>
{
    var entity = await context.Deliveries.FirstOrDefaultAsync(x => x.Tid == request!.Tid);

    if (entity != null)
    {
        entity.Status = false;
        context.Deliveries.Update(entity);
        context.SaveChanges();
    }

    return Results.Ok();
});

app.MapGet("/deliveries", async (DeliveryDbContext context) =>
{
    var response = await context.Deliveries.ToArrayAsync();
    return Results.Ok(response);
});

app.Run();