using System.Reflection;

namespace SoulCore.Host.Hosting;

/// <summary>Build version for Presence Settings /health so Kayleigh can verify Host is current.</summary>
public static class HostBuildVersion
{
    public static string Current { get; } = Resolve();

    public static string Resolve(Assembly? assembly = null)
    {
        var asm = assembly ?? typeof(HostBuildVersion).Assembly;
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(info))
        {
            // Strip optional +git metadata from InformationalVersion.
            var plus = info.IndexOf('+', StringComparison.Ordinal);
            return plus > 0 ? info[..plus] : info.Trim();
        }

        return asm.GetName().Version?.ToString(3) ?? "0.0.0";
    }
}
