using System.Reflection;
using Velopack;
using Velopack.Sources;

namespace House.ChatDesktop.Services;

/// <summary>PROP-4.2: Velopack update checks for Presence (House.ChatDesktop).</summary>
public sealed class PresenceUpdateService
{
    /// <summary>Default feed — GitHub Releases for this repo (override with env / settings).</summary>
    public const string DefaultGithubRepoUrl = "https://github.com/Linearthrone/SoulCore.AI";

    public string CurrentVersion { get; }

    public string FeedDescription { get; }

    public bool IsInstalled { get; }

    private readonly UpdateManager? _manager;

    public PresenceUpdateService(string? feedOverride = null)
    {
        CurrentVersion = ResolveVersion();
        var feed = ResolveFeed(feedOverride);
        FeedDescription = feed.Description;

        try
        {
            _manager = feed.Source is null
                ? new UpdateManager(feed.UrlOrPath!)
                : new UpdateManager(feed.Source);
            IsInstalled = _manager.IsInstalled;
        }
        catch
        {
            _manager = null;
            IsInstalled = false;
        }
    }

    public async Task<PresenceUpdateCheckResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        if (_manager is null || !_manager.IsInstalled)
        {
            return PresenceUpdateCheckResult.DevBuild(
                $"Dev / unpackaged build {CurrentVersion}. Install via Setup.exe from pack-presence.ps1 to enable updates.");
        }

        try
        {
            var info = await _manager.CheckForUpdatesAsync().ConfigureAwait(false);
            if (info is null)
                return PresenceUpdateCheckResult.UpToDate(CurrentVersion);

            var remote = info.TargetFullRelease?.Version?.ToString() ?? "newer";
            return PresenceUpdateCheckResult.Available(CurrentVersion, remote, info);
        }
        catch (Exception ex)
        {
            return PresenceUpdateCheckResult.Fail($"Update check failed: {ex.Message}");
        }
    }

    public async Task<PresenceUpdateApplyResult> DownloadAndApplyAsync(
        UpdateInfo update,
        Action<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (_manager is null)
            return PresenceUpdateApplyResult.Fail("Update manager unavailable.");

        try
        {
            await _manager.DownloadUpdatesAsync(update, progress).ConfigureAwait(false);
            _manager.ApplyUpdatesAndRestart(update);
            return PresenceUpdateApplyResult.Restarting();
        }
        catch (Exception ex)
        {
            return PresenceUpdateApplyResult.Fail($"Update apply failed: {ex.Message}");
        }
    }

    private static string ResolveVersion()
    {
        var asm = Assembly.GetExecutingAssembly();
        var informational = asm
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plus = informational.IndexOf('+', StringComparison.Ordinal);
            return plus > 0 ? informational[..plus] : informational;
        }

        return asm.GetName().Version?.ToString(3) ?? "0.0.0";
    }

    private static Feed ResolveFeed(string? feedOverride)
    {
        var raw = (feedOverride
                   ?? Environment.GetEnvironmentVariable("HOUSE_VICTORIA_UPDATE_URL")
                   ?? Environment.GetEnvironmentVariable("SOULCORE_PRESENCE_UPDATE_URL")
                   ?? "").Trim();

        if (string.IsNullOrEmpty(raw))
        {
            return new Feed(
                Description: $"GitHub Releases ({DefaultGithubRepoUrl})",
                Source: new GithubSource(DefaultGithubRepoUrl, string.Empty, prerelease: false),
                UrlOrPath: null);
        }

        if (raw.Contains("github.com", StringComparison.OrdinalIgnoreCase))
        {
            return new Feed(
                Description: $"GitHub Releases ({raw})",
                Source: new GithubSource(raw, string.Empty, prerelease: false),
                UrlOrPath: null);
        }

        return new Feed(
            Description: raw,
            Source: null,
            UrlOrPath: raw);
    }

    private sealed record Feed(string Description, IUpdateSource? Source, string? UrlOrPath);
}

public sealed class PresenceUpdateCheckResult
{
    public enum Kind
    {
        UpToDate,
        Available,
        DevBuild,
        Failed
    }

    public Kind Status { get; init; }
    public string Message { get; init; } = "";
    public string? CurrentVersion { get; init; }
    public string? AvailableVersion { get; init; }
    public UpdateInfo? Update { get; init; }

    public static PresenceUpdateCheckResult UpToDate(string version) => new()
    {
        Status = Kind.UpToDate,
        CurrentVersion = version,
        Message = $"You're on the latest Presence ({version})."
    };

    public static PresenceUpdateCheckResult Available(string current, string remote, UpdateInfo info) => new()
    {
        Status = Kind.Available,
        CurrentVersion = current,
        AvailableVersion = remote,
        Update = info,
        Message = $"Update available: {remote} (you have {current})."
    };

    public static PresenceUpdateCheckResult DevBuild(string message) => new()
    {
        Status = Kind.DevBuild,
        Message = message
    };

    public static PresenceUpdateCheckResult Fail(string message) => new()
    {
        Status = Kind.Failed,
        Message = message
    };
}

public sealed class PresenceUpdateApplyResult
{
    public bool Ok { get; init; }
    public string Message { get; init; } = "";

    public static PresenceUpdateApplyResult Restarting() => new()
    {
        Ok = true,
        Message = "Installing update and restarting…"
    };

    public static PresenceUpdateApplyResult Fail(string message) => new()
    {
        Ok = false,
        Message = message
    };
}
