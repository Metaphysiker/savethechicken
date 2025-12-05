using System.Net.Http.Json;
using MauiBlazorWeb.Shared.Factories.FactoriesImpl;
using MauiBlazorWeb.Shared.Services;
using MauiBlazorWeb.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the MauiBlazorWeb.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

var apiBaseUrl = builder.Configuration["API_BASE_URL"] ?? "http://localhost:8081/";
var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
Console.WriteLine($"Using API_BASE_URL: {apiBaseUrl}");
builder.Services.AddScoped(sp =>
{
    return new GenericDtoServiceFactory(http, apiBaseUrl);
});

await builder.Build().RunAsync();
