namespace House.ChatDesktop.Services;

/// <summary>SemVer helpers for Presence/Host version bumps (PROP-4.2).</summary>
public static class SemVerBump
{
    public static string Bump(string version, string part = "patch")
    {
        var core = (version ?? "").Trim();
        var plus = core.IndexOf('+', StringComparison.Ordinal);
        if (plus > 0) core = core[..plus];
        var dash = core.IndexOf('-', StringComparison.Ordinal);
        if (dash > 0) core = core[..dash];

        var bits = core.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var major = bits.Length > 0 && int.TryParse(bits[0], out var maj) ? maj : 0;
        var minor = bits.Length > 1 && int.TryParse(bits[1], out var min) ? min : 0;
        var patch = bits.Length > 2 && int.TryParse(bits[2], out var pat) ? pat : 0;

        switch ((part ?? "patch").Trim().ToLowerInvariant())
        {
            case "major":
                major++;
                minor = 0;
                patch = 0;
                break;
            case "minor":
                minor++;
                patch = 0;
                break;
            default:
                patch++;
                break;
        }

        return $"{major}.{minor}.{patch}";
    }
}
