namespace House.ChatDesktop.Services;

/// <summary>
/// Resolves <c>SOULCORE_COMPANION_API_TOKEN</c> for Host /ws auth when the token is set.
/// Never logs the value.
/// </summary>
public static class CompanionToken
{
    public const string EnvName = "SOULCORE_COMPANION_API_TOKEN";

    /// <summary>
    /// Load SOULCORE_* keys from SoulCore/.env into the process.
    /// <b>.env wins</b> over stale Process/User-inherited values — same footgun as Host
    /// (HTTP /health looks "up" while /ws 401s with the wrong Bearer).
    /// </summary>
    /// <param name="repoRoot">
    /// Optional SoulCore checkout root (folder with ALLSTART.ps1). Required for Velopack
    /// installs whose BaseDirectory is outside the repo.
    /// </param>
    /// <returns>Count of keys set or updated; 0 if no .env found.</returns>
    public static int TryLoadFromEnvFile(string? repoRoot = null)
    {
        var envPath = FindSoulCoreEnvFile(repoRoot);
        if (envPath is null)
            return 0;

        var applied = 0;
        foreach (var line in File.ReadLines(envPath))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 0 && trimmed[0] == '\uFEFF')
                trimmed = trimmed[1..].TrimStart();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;
            var eq = trimmed.IndexOf('=');
            if (eq < 1)
                continue;
            var key = trimmed[..eq].Trim();
            if (!key.StartsWith("SOULCORE_", StringComparison.Ordinal))
                continue;

            var value = Unquote(trimmed[(eq + 1)..].Trim());
            if (string.IsNullOrEmpty(value))
            {
                Environment.SetEnvironmentVariable(key, null);
                applied++;
                continue;
            }

            var existing = Environment.GetEnvironmentVariable(key);
            if (string.Equals(existing, value, StringComparison.Ordinal))
                continue;

            Environment.SetEnvironmentVariable(key, value);
            applied++;
        }

        return applied;
    }

    /// <summary>
    /// Apply process env from .env (if found) then overlay the Settings-saved token
    /// when present. Settings wins so an installed app can auth without a repo checkout.
    /// </summary>
    public static int ApplyAllSources(string? repoRoot = null)
    {
        var applied = TryLoadFromEnvFile(repoRoot);
        if (ApplySavedSettingsToken())
            applied++;
        return applied;
    }

    /// <summary>
    /// Push Settings-saved token into process env. Returns true when a saved token was applied.
    /// </summary>
    public static bool ApplySavedSettingsToken()
    {
        var saved = CompanionTokenStore.Read();
        if (string.IsNullOrWhiteSpace(saved))
            return false;
        var existing = Environment.GetEnvironmentVariable(EnvName);
        if (string.Equals(existing, saved, StringComparison.Ordinal))
            return true;
        Environment.SetEnvironmentVariable(EnvName, saved);
        return true;
    }

    public static string? Resolve()
    {
        // Settings store first — operator-entered value for Setup.exe installs.
        var saved = CompanionTokenStore.Read();
        if (!string.IsNullOrWhiteSpace(saved))
            return saved.Trim();

        var fromEnv = Environment.GetEnvironmentVariable(EnvName);
        return string.IsNullOrWhiteSpace(fromEnv) ? null : fromEnv.Trim();
    }

    /// <summary>Safe for UI / logs — never includes the secret.</summary>
    public static string DescribePresence()
    {
        var token = Resolve();
        var source = CompanionTokenStore.HasSavedToken()
            ? "source=settings"
            : (Environment.GetEnvironmentVariable(EnvName) is { Length: > 0 }
                ? "source=env"
                : "source=none");
        return token is null
            ? $"tokenPresent=false tokenLen=0 {source}"
            : $"tokenPresent=true tokenLen={token.Length} {source}";
    }

    /// <summary>Path of the .env that would be loaded (for diagnostics; never read secrets).</summary>
    public static string? ResolvedEnvFilePath(string? repoRoot = null) =>
        FindSoulCoreEnvFile(repoRoot);

    private static string Unquote(string raw)
    {
        if (raw.Length >= 2
            && ((raw[0] == '"' && raw[^1] == '"') || (raw[0] == '\'' && raw[^1] == '\'')))
        {
            return raw[1..^1];
        }

        return raw;
    }

    private static string? FindSoulCoreEnvFile(string? repoRoot)
    {
        foreach (var candidate in EnumerateEnvFileCandidates(repoRoot))
        {
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    public static IEnumerable<string> EnumerateEnvFileCandidates(string? repoRoot)
    {
        foreach (var root in LocalStackControl.EnumerateRepoRootCandidates(repoRoot))
        {
            if (string.IsNullOrWhiteSpace(root))
                continue;
            yield return Path.Combine(root, "SoulCore", ".env");
            if (root.EndsWith("SoulCore", StringComparison.OrdinalIgnoreCase)
                || root.EndsWith($"SoulCore{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                || root.EndsWith($"SoulCore{Path.AltDirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            {
                yield return Path.Combine(root, ".env");
            }
        }

        // Legacy walk from BaseDirectory / cwd (dev launches).
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 8 && dir is not null; i++, dir = dir.Parent)
        {
            yield return Path.Combine(dir.FullName, "SoulCore", ".env");
            if (dir.Name.Equals("SoulCore", StringComparison.OrdinalIgnoreCase))
                yield return Path.Combine(dir.FullName, ".env");
        }

        string? cwd = null;
        try { cwd = Directory.GetCurrentDirectory(); }
        catch { /* ignore */ }
        if (!string.IsNullOrWhiteSpace(cwd))
        {
            dir = new DirectoryInfo(cwd);
            for (var i = 0; i < 6 && dir is not null; i++, dir = dir.Parent)
                yield return Path.Combine(dir.FullName, "SoulCore", ".env");
        }
    }
}
