using Blazored.LocalStorage;
using MauiBlazorWeb.Services;
using MauiBlazorWeb.Shared.Factories.FactoriesImpl;
using MauiBlazorWeb.Shared.Services;
using MauiBlazorWeb.Shared.Singletons.SingletonsImpl;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using System.Globalization;

namespace MauiBlazorWeb;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add device-specific services used by the MauiBlazorWeb.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddScoped<TokenService>();
        builder.Services.AddScoped<AuthService>();

        builder.Services.AddSingleton<AuthResponseSingleton>();

        // Add device-specific services used by the MauiBlazorWeb.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        var apiBaseUrl = builder.Configuration["API_BASE_URL"] ?? "https://localhost:7106/";

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

        return builder.Build();
    }
}
