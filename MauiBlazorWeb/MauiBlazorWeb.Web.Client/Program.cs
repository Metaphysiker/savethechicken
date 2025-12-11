using Blazored.LocalStorage;
using MauiBlazorWeb.Shared.Factories.FactoriesImpl;
using MauiBlazorWeb.Shared.Services;
using MauiBlazorWeb.Shared.Singletons.SingletonsImpl;
using MauiBlazorWeb.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddSingleton<AuthResponseSingleton>();

// Add device-specific services used by the MauiBlazorWeb.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

var apiBaseUrl = builder.Configuration["API_BASE_URL"] ?? "http://localhost:8081/";

// Register HttpClient for WebAssembly DI
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// Register factory and let it use HttpClient from DI
builder.Services.AddScoped<GenericDtoServiceFactory>();

builder.Services.AddMudServices();


await builder.Build().RunAsync();
