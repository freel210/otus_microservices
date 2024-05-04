using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PaymentsService.ConfigOptions;
using PaymentsService.Contexts;
using PaymentsService.DTO.Income;
using PaymentsService.Entities;
using PaymentsService.Helpers;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5071); });

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
builder.Services.AddDbContext<PaymentDbContext>(options =>
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

app.MapPost("/add", async ([Required] StorageRequest? request, PaymentDbContext context) =>
{
    Guid id = Guid.NewGuid();
    Payment entity = new ()
    {
        Id = id,
        Tid = request!.Tid,
        Status = true
    };

    await context.Payments.AddAsync(entity);
    context.SaveChanges();

    return Results.Ok(new { Id = id });
});

app.MapPost("/cancel", async ([Required] StorageRequest? request, PaymentDbContext context) =>
{
    var entity = await context.Payments.FirstOrDefaultAsync(x => x.Tid == request!.Tid);

    if (entity != null)
    {
        entity.Status = false;
        context.Payments.Update(entity);
        context.SaveChanges();
    }

    return Results.Ok();
});

app.MapGet("/items", async (PaymentDbContext context) =>
{
    var response = await context.Payments.ToArrayAsync();
    return Results.Ok(response);
});

app.Run();