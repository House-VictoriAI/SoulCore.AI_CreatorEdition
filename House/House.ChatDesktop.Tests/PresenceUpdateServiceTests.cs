using House.ChatDesktop.Services;
using Xunit;

namespace House.ChatDesktop.Tests;

public class PresenceUpdateServiceTests
{
    [Fact]
    public void Constructor_ReportsCurrentVersion_AndDefaultGithubFeed()
    {
        var svc = new PresenceUpdateService();
        Assert.False(string.IsNullOrWhiteSpace(svc.CurrentVersion));
        Assert.Contains("GitHub", svc.FeedDescription, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SoulCore.AI", svc.FeedDescription, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_HttpFeedOverride_UsesSimpleDescription()
    {
        var svc = new PresenceUpdateService("https://example.com/presence-updates");
        Assert.Equal("https://example.com/presence-updates", svc.FeedDescription);
    }

    [Fact]
    public async Task CheckAsync_Unpackaged_ReturnsDevBuild()
    {
        var svc = new PresenceUpdateService();
        // Cloud/dev runs are not Velopack-installed.
        Assert.False(svc.IsInstalled);
        var result = await svc.CheckAsync();
        Assert.Equal(PresenceUpdateCheckResult.Kind.DevBuild, result.Status);
        Assert.Contains("Setup.exe", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
