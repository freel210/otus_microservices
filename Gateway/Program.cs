using Gateway.Authentication;
using Gateway.ConfigOptions;
using Gateway.Contexts;
using Gateway.Endpoints;
using Gateway.Helpers;
using Gateway.Repositories;
using Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5168); });

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

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Authorization: Bearer JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });

    options.SupportNonNullableReferenceTypes();
});

builder.Services.AddOptions<ApiPointsOptions>().BindConfiguration("ApiPointsOptions");

builder.Services.AddOptions<PublicKeyOptions>().BindConfiguration("PublicKeyOptions");
builder.Services.AddSingleton<IPublicKeyRepository, PublicKeyRepository>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IPurchaseService, PurchaseService>();
builder.Services.AddSingleton<IKafkaService, KafkaService>();
builder.Services.AddSingleton<IBillingService, BillingService>();

builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();

builder.Services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, TokenValidatorPostConfigure>();
builder.Services.AddScoped<JwtBearerEventsHandler>();

builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.IncludeErrorDetails = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = false,
        ValidateAudience = true,
        ValidateIssuer = false,
        ValidateLifetime = false,
        ValidAudience = "otus_microservices_aud",
        ClockSkew = TimeSpan.Zero,
    };

    o.EventsType = typeof(JwtBearerEventsHandler);
});

builder.Services.AddAuthorization();

builder.Services.AddOptions<PostgresOptions>().BindConfiguration("PostgresOptions");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<GatewayDbContext>(options =>
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

app.UseAuthentication();
app.UseAuthorization();

app.RegisterEndpoints();
app.RegisterUsersEndpoints();

app.Run();
