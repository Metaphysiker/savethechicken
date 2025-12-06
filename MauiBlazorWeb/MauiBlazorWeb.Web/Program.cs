using MauiBlazorWeb.Web.Components;
using MauiBlazorWeb.Shared.Services;
using MauiBlazorWeb.Web.Services;
using MauiBlazorWeb.Shared.Factories.FactoriesImpl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add device-specific services used by the MauiBlazorWeb.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// get API_BASE_URL from environment variables docker
var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? throw new ArgumentNullException("API_BASE_URL");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseUrl) });
builder.Services.AddScoped<GenericDtoServiceFactory>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new GenericDtoServiceFactory(httpClient);
});

var app = builder.Build();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(MauiBlazorWeb.Shared._Imports).Assembly,
        typeof(MauiBlazorWeb.Web.Client._Imports).Assembly);

app.Run();
