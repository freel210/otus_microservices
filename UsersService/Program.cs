using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using Microsoft.OpenApi.Models;
using UsersService.DTO.Income;
using UsersService.Services;
using UsersService.Enums;
using UsersService.ConfigOptions;
using UsersService.Helpers;
using UsersService.Contexts;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(8000); });

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

builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<MetricsService>();

builder.Services.AddOptions<PostgresOptions>().BindConfiguration("PostgresOptions");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<UserDbContext>(options =>
{
    options.UseNpgsql();
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder.AddPrometheusExporter();

        builder.AddMeter("Microsoft.AspNetCore.Hosting",
                         "Microsoft.AspNetCore.Server.Kestrel");
        builder.AddView("http.server.request.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
            });
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"./{AssemblyInfo.AssemblyName}/swagger.json", AssemblyInfo.AssemblyName);
    options.DocumentTitle = $"{AssemblyInfo.ProgramNameVersion} manual";
});
app.UseDeveloperExceptionPage();

app.MapPrometheusScrapingEndpoint();

app.MapGet("/health/", () => TypedResults.Ok(new {status = "Ok"}));

app.MapPost("/user", async ([FromHeader(Name = "X-Request-Id")] Guid? requestId, UserAddRequest request, IUserService service) =>
{
    if (requestId == null)
    {
        return Results.BadRequest("Header X-Request-Id cannot be null");
    }
    
    var response = await service.Add(requestId!.Value, request);

    return Results.Created($"/user/{response.Id}", response);
});

app.MapGet("/user/{id}", async (Guid id, IUserService service) =>
{
    var response = await service.Get(id);

    return response is not null
        ? Results.Ok(response)
        : Results.NotFound(id);
});

app.MapGet("/users", async (IUserService service, MetricsService metrics) =>
{
    var response = await service.GetAll();
    metrics.TakeErrorIntoAccount("qwer");
    return Results.Ok(response);
});

app.MapPut("/user", async (UserUpdateRequest request, IUserService service) =>
{
    var result = await service.Update(request);

    return result switch
    {
        UpdateResults.Ok => Results.Ok(),
        UpdateResults.NotFound => Results.NotFound(request.Id),
        UpdateResults.Conflict => Results.Conflict(request.VersionId),
        _ => throw new NotImplementedException(),
    };
});

app.MapDelete("/user/{id}", async (Guid id, IUserService service) =>
{
    bool isDeleted = await service.Delete(id);
    return isDeleted ? Results.NoContent() : Results.NotFound(id);
});

app.MapDelete("/user", async (IUserService service) =>
{
    await service.DeleteAll();
    return Results.NoContent();
});

app.Run();
