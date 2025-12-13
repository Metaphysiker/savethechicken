using Blazored.LocalStorage;
using MauiBlazorWeb.Shared.Factories.FactoriesImpl;
using MauiBlazorWeb.Shared.Services;
using MauiBlazorWeb.Shared.Services.ServicesImpl;
using MauiBlazorWeb.Shared.Singletons.SingletonsImpl;
using MauiBlazorWeb.Web.Components;
using MauiBlazorWeb.Web.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AwsFileService>();

builder.Services.AddSingleton<AuthResponseSingleton>();


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add device-specific services used by the MauiBlazorWeb.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
var baseUrl = isDocker ? "http://webapi:8080/" : "https://localhost:7101/";


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseUrl) });
builder.Services.AddScoped<GenericDtoServiceFactory>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new GenericDtoServiceFactory(httpClient);
});
builder.Services.AddMudServices();

builder.Services.AddLocalization();

var supportedCultures = new[] { "en", "de", "fr" };

builder.Services.AddControllers();

var app = builder.Build();
app.MapStaticAssets();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("de")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(MauiBlazorWeb.Shared._Imports).Assembly,
        typeof(MauiBlazorWeb.Web.Client._Imports).Assembly);

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
