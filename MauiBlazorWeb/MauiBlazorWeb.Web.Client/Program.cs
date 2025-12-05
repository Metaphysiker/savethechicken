using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MauiBlazorWeb.Shared.Services;
using MauiBlazorWeb.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the MauiBlazorWeb.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

await builder.Build().RunAsync();
