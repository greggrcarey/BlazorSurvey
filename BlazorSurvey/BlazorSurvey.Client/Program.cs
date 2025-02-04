using BlazorSurvey.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

//builder.Services.AddScoped<ISurveyService, ClientSurveyService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7246") });

await builder.Build().RunAsync();
