using Demo.ConfigOptions;
using Demo.Contexts;
using Demo.DTO.Income;
using Demo.Metrics;
using Demo.Services;
using Demo.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(8000); });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<MetricsService>();

builder.Services.AddOpenTelemetry()
    .WithMetrics(x =>
    {
        x.AddPrometheusExporter();
        //x.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel", "UserService");
        //x.AddMeter("UserService");
        //x.AddInstrumentation(new MetricsService());
        x.AddUpTimeInstrumentation();
        x.AddView("request-duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05, 0.07, 0.1, 0.25, 0.5, 0.7, 0.9, 0.95, 0.98, 0.99, 0.999, 1 }
            });
    });
         

builder.Services.AddOptions<PostgresOptions>().BindConfiguration("PostgresOptions");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<UserDbContext>(options =>
{
    options.UseNpgsql();
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseDeveloperExceptionPage();

//app.MapPrometheusScrapingEndpoint();

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

app.UseOpenTelemetryPrometheusScrapingEndpoint("metrics");

app.Run();
