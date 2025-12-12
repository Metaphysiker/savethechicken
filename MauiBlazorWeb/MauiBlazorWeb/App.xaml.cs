using System.Globalization;

namespace MauiBlazorWeb;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        SetCulture("de");

    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "MauiBlazorWeb" };
    }

    public void SetCulture(string culture)
    {
        var ci = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = ci;
        CultureInfo.DefaultThreadCurrentUICulture = ci;
    }
}
