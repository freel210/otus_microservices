using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using BillingService.Helpers;
using BillingService.ConfigOptions;
using System.ComponentModel.DataAnnotations;
using BillingService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5076); });

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

    options.SupportNonNullableReferenceTypes();
});

builder.Services.AddOptions<PostgresOptions>().BindConfiguration("PostgresOptions");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
//builder.Services.AddDbContext<PaymentDbContext>(options =>
//{
//    options.UseNpgsql();
//    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
//});

//builder.Services.AddSingleton<IKafkaHostedService, KafkaHostedService>();
builder.Services.AddHostedService<KafkaHostedService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"./{AssemblyInfo.AssemblyName}/swagger.json", AssemblyInfo.AssemblyName);
    options.DocumentTitle = $"{AssemblyInfo.ProgramNameVersion} manual";
});
app.UseDeveloperExceptionPage();

app.Run();