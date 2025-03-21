using BlazorSurvey;
using BlazorSurvey.Components;
using BlazorSurvey.Components.Account;
using BlazorSurvey.Data;
using BlazorSurvey.Services;
using BlazorSurvey.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor.Services;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

//probably in here somewhere
//https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-9.0&tabs=visual-studio

#region Configuration
IConfigurationRoot configuration = new ConfigurationBuilder()
.AddUserSecrets<Program>()
.Build();
#endregion
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

#region CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
    policy =>
    {
        policy.WithOrigins("https://localhost:7144", "http://localhost:5072").AllowAnyHeader().AllowAnyMethod();
    });
});

#endregion



#region Authentication
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
#endregion

#region CosmosDb

builder.Services.AddSingleton<CosmosClient>(sp =>
{
    JsonSerializerOptions jsOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        //Web defaults
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };


    CosmosClientOptions cosmosClientOptions = new()
    {
        ApplicationName = nameof(BlazorSurvey),
        ConnectionMode = ConnectionMode.Direct,
        MaxRetryAttemptsOnRateLimitedRequests = 2,
        RequestTimeout = TimeSpan.FromSeconds(60),
        ConsistencyLevel = ConsistencyLevel.Session,
        UseSystemTextJsonSerializerWithOptions = jsOptions
    };

    string? endpoint = configuration["CosmosDbAccountEndpoint"] ?? throw new InvalidOperationException("CosmosDbAccountEndpoint is missing from configuration");
    string? authkey = configuration["CosmosDbAuthKey"] ?? throw new InvalidOperationException("CosmosDbAuthKey is missing from configuration");

    return new CosmosClient(
        accountEndpoint: endpoint,
        authKeyOrResourceToken: authkey,
        clientOptions: cosmosClientOptions);

});

#endregion

#region OTEL
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(nameof(BlazorSurvey)))
    .WithMetrics(metrics =>
    {
        metrics
        .AddAspNetCoreInstrumentation()
        .AddAspNetCoreInstrumentation();

        metrics
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/logs");
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                options.Headers = $"X-Seq-ApiKey={configuration["local:SeqApiKey"]}";

            });
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        tracing.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/logs");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            options.Headers = $"X-Seq-ApiKey={configuration["local:SeqApiKey"]}";

        });
    });

builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter(options =>
{
    options.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/logs");
    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
    options.Headers = $"X-Seq-ApiKey={configuration["local:SeqApiKey"]}";

}));

#endregion



#region Rate Limiter
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));

#endregion


builder.Services.AddMudServices();
builder.Services.TryAddScoped<IWebAssemblyHostEnvironment, ServerHostEnvironment>();
builder.Services.AddScoped<SurveyState>();
builder.Services.AddScoped<CosmosDbService>();
builder.Services.AddScoped<ISurveyService, ServerSurveyService>();
builder.Services.AddScoped<SurveyBaseModule>();
builder.Services.AddScoped<UserService>();
builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<CircuitHandler, UserCircuitHandler>());
builder.Services.AddMemoryCache();





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();

    await Task.Run(async () =>
    {
        await Task.Delay(3000);

        var homeUrl = "https://localhost:7246";

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = homeUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Falied to open homepage: {ex.Message}", ex.Message);
        }

    });
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();


app.UseAntiforgery();

//var surveyModule = app.Services.GetRequiredService<SurveyBaseModule>();
//surveyModule.MapSurveyBaseEndpoints(app);

using ServiceProvider serviceProvider = builder.Services.BuildServiceProvider(validateScopes: true);
using (IServiceScope scope = serviceProvider.CreateScope())
{
    var surveyModule1 = scope.ServiceProvider.GetRequiredService<SurveyBaseModule>();
    surveyModule1.MapSurveyBaseEndpoints(app);
}


//need to review for ServiceLocator Pattern
//https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines#scoped-service-as-singleton


app.MapStaticAssets();
app.UseMiddleware<UserServiceMiddleware>();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorSurvey.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

await app.RunAsync();
