using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StorageService.ConfigOptions;
using StorageService.Contexts;
using StorageService.DTO.Income;
using StorageService.Helpers;
using StorageService.Repositoreis;
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
});

builder.Services.AddSingleton<IStorageRepository, StorageRepository>(); 

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"./{AssemblyInfo.AssemblyName}/swagger.json", AssemblyInfo.AssemblyName);
    options.DocumentTitle = $"{AssemblyInfo.ProgramNameVersion} manual";
});
app.UseDeveloperExceptionPage();

app.MapPost("/add", async ([Required] StorageRequest? request, IStorageRepository repository) =>
{
    Guid id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6");

    await repository.AddItem(id, request.Quantity);

    return Results.Ok();
});

app.MapGet("/items", async (IStorageRepository repository) =>
{
    var response = await repository.GetAll();
    return Results.Ok(response);
});

app.Run();