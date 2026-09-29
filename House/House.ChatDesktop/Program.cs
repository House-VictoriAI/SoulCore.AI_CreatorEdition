using Avalonia;
using House.ChatDesktop.Services;
using Velopack;

namespace House.ChatDesktop;

internal static class Program
{
    // Cross-platform Avalonia entry point (Linux, Windows, macOS).
    [STAThread]
    public static void Main(string[] args)
    {
        // PROP-4.2: Velopack hooks must run before any other startup work.
        VelopackApp.Build()
            .SetArgs(args)
            .Run();

        // Host /ws requires Bearer when SOULCORE_COMPANION_API_TOKEN is set (phone + desktop).
        CompanionToken.TryLoadFromEnvFile();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
