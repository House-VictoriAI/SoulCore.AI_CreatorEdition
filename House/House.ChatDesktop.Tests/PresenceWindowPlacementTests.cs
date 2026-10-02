using Avalonia;
using House.ChatDesktop.Services;
using Xunit;

namespace House.ChatDesktop.Tests;

public class PresenceWindowPlacementTests
{
    private static readonly PixelRect Primary = new(0, 0, 1920, 1080);
    private static readonly PixelRect Secondary = new(1920, 0, 1920, 1080);

    [Fact]
    public void ResolveStartPosition_NoSaved_LeavesCenterScreen()
    {
        var (pos, relocated) = PresenceWindowPlacement.ResolveStartPosition(
            null, null, 1180, 780, new[] { Primary });

        Assert.Null(pos);
        Assert.False(relocated);
    }

    [Fact]
    public void ResolveStartPosition_OnScreen_KeepsCoords()
    {
        var (pos, relocated) = PresenceWindowPlacement.ResolveStartPosition(
            100, 80, 1180, 780, new[] { Primary });

        Assert.Equal(new PixelPoint(100, 80), pos);
        Assert.False(relocated);
    }

    [Fact]
    public void ResolveStartPosition_OffScreen_CentersOnPrimary()
    {
        var (pos, relocated) = PresenceWindowPlacement.ResolveStartPosition(
            8000, 5000, 1180, 780, new[] { Primary });

        Assert.True(relocated);
        Assert.NotNull(pos);
        Assert.Equal(
            PresenceWindowPlacement.CenterOnPrimary(new PixelSize(1180, 780), new[] { Primary }),
            pos);
    }

    [Fact]
    public void ResolveStartPosition_SecondMonitor_Kept()
    {
        var (pos, relocated) = PresenceWindowPlacement.ResolveStartPosition(
            2000, 40, 1180, 780, new[] { Primary, Secondary });

        Assert.Equal(new PixelPoint(2000, 40), pos);
        Assert.False(relocated);
    }

    [Fact]
    public void IsPointOnAnyScreen_FalseWhenOutside()
    {
        Assert.False(PresenceWindowPlacement.IsPointOnAnyScreen(-100, -100, new[] { Primary }));
        Assert.True(PresenceWindowPlacement.IsPointOnAnyScreen(10, 10, new[] { Primary }));
    }
}
