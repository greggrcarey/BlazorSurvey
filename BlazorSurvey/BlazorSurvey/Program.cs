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
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
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
using Azure.Identity;
using Azure.ResourceManager;
using Azure.Core;

var builder = WebApplication.CreateBuilder(args);

//confirmation email
//https://learn.microsoft.com/en-us/aspnet/core/blazor/security/account-confirmation-and-password-recovery?view=aspnetcore-9.0


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
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("https://localhost:7144", "http://localhost:5072").AllowAnyHeader().AllowAnyMethod();
        });
    });
}

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

var connectionString = string.Empty;

if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING") ?? throw new InvalidOperationException("Connection string 'AZURE_SQL_CONNECTIONSTRING' not found.");
}



builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Tokens.ProviderMap.Add("CustomEmailConfirmation",
        new TokenProviderDescriptor(
            typeof(CustomEmailConfirmationTokenProvider<ApplicationUser>)));
    options.Tokens.EmailConfirmationTokenProvider =
        "CustomEmailConfirmation";
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services
    .AddTransient<CustomEmailConfirmationTokenProvider<ApplicationUser>>();


builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, EmailSender>();

builder.Services.ConfigureApplicationCookie(options => {
    options.ExpireTimeSpan = TimeSpan.FromDays(5);
    options.SlidingExpiration = true;
});

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromHours(3));
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

    if (builder.Environment.IsDevelopment())
    {
        string? endpoint = builder.Configuration["CosmosDbAccountEndpoint"] ?? throw new InvalidOperationException("CosmosDbAccountEndpoint is missing from configuration");
        string? authkey = builder.Configuration["CosmosDbAuthKey"] ?? throw new InvalidOperationException("CosmosDbAuthKey is missing from configuration");

        return new CosmosClient(
            accountEndpoint: endpoint,
            authKeyOrResourceToken: authkey,
            clientOptions: cosmosClientOptions);
    }
    else
    {
        string? endpoint = builder.Configuration.GetConnectionString("CosmosDBConnection") ?? throw new InvalidOperationException("ConnectionStrings__CosmosDBConnection is missing from configuration");
        string? clientId = builder.Configuration["USER_MANAGED_ID"];
        ManagedIdentityCredential? tokenCredential = new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(clientId));


        return new CosmosClient(
            accountEndpoint: endpoint,
            tokenCredential: tokenCredential ,
            clientOptions: cosmosClientOptions);
    }
});

#endregion

#region OTEL
if (builder.Environment.IsDevelopment())
{
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
}

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
builder.Services.AddHttpContextAccessor();
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
