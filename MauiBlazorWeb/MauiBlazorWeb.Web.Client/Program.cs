using Blazored.LocalStorage;
using MauiBlazorWeb.Shared.Factories.FactoriesImpl;
using MauiBlazorWeb.Shared.Services;
using MauiBlazorWeb.Shared.Services.ServicesImpl;
using MauiBlazorWeb.Shared.Singletons.SingletonsImpl;
using MauiBlazorWeb.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Shared.Dtos.DtosImpl;
using System.Globalization;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AwsFileService>();
builder.Services.AddSingleton<AuthResponseSingleton>();

// Add device-specific services used by the MauiBlazorWeb.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

//var apiBaseUrl = builder.Configuration["API_BASE_URL"] ?? "https://localhost:7101/";
using var hostHttp = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

ConfigDto? config = await hostHttp.GetFromJsonAsync<ConfigDto>("api/config");
if (config is null)
{
    config = new ConfigDto
    {
        ApiBaseUrl = "https://localhost:7101/"
    };
}

var apiBaseUrl = config.ApiBaseUrl ?? "https://localhost:7101/";

// Register HttpClient for WebAssembly DI
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// Register factory and let it use HttpClient from DI
builder.Services.AddScoped<GenericDtoServiceFactory>();

builder.Services.AddMudServices();

builder.Services.AddLocalization();

var ci = new CultureInfo("de");
CultureInfo.DefaultThreadCurrentCulture = ci;
CultureInfo.DefaultThreadCurrentUICulture = ci;

await builder.Build().RunAsync();
