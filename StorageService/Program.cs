using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StorageService.ConfigOptions;
using StorageService.Contexts;
using StorageService.DTO.Income;
using StorageService.Entities;
using StorageService.Helpers;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5072); });

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
builder.Services.AddDbContext<StorageDbContext>(options =>
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

app.MapPost("/add", async ([Required] StorageRequest? request, StorageDbContext context) =>
{
    Guid id = Guid.NewGuid();
    Item entity = new Item()
    {
        Id = id,
        Tid = request!.Tid,
        Status = true
    };

    await context.Items.AddAsync(entity);
    context.SaveChanges();

    return Results.Ok(new { Id = id });
});

app.MapPost("/cancel", async ([Required] StorageRequest? request, StorageDbContext context) =>
{
    var entity = await context.Items.FirstOrDefaultAsync(x => x.Tid == request!.Tid);

    if (entity != null)
    {
        entity.Status = false;
        context.Items.Update(entity);
        context.SaveChanges();
    }

    return Results.Ok();
});

app.MapGet("/items", async (StorageDbContext context) =>
{
    var response = await context.Items.ToArrayAsync();
    return Results.Ok(response);
});

app.Run();