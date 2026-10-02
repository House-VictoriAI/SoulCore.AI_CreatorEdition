using SoulCore.Inference.Tools.Browser;
using SoulCore.Inference.Tools.Desktop;
using Xunit;

namespace SoulCore.Protocol.Tests;

public class VictoriaBrowserViewHubTests
{
    [Fact]
    public void Publish_SetsBackend_AndPreservesImage()
    {
        var hub = new VictoriaBrowserViewHub();
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3, 4 };

        hub.Publish(png, null, "victoria-sandbox", "desktop_screenshot", backend: VictoriaBrowserViewHub.BackendVboxGuest);

        var snap = hub.GetSnapshot();
        Assert.True(snap.HasImage);
        Assert.Equal(VictoriaBrowserViewHub.BackendVboxGuest, snap.Backend);
        Assert.Equal("victoria-sandbox", snap.Title);
        Assert.Equal("desktop_screenshot", snap.LastAction);
        Assert.True(hub.TryGetImageBytes(out var bytes, out var ct));
        Assert.Equal(png, bytes);
        Assert.Equal("image/png", ct);
    }

    [Fact]
    public void TryPublishFromToolData_MirrorsGuestBytes()
    {
        var hub = new VictoriaBrowserViewHub();
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 9, 9, 9 };

        Assert.True(VictoriaBrowserViewHub.TryPublishFromToolData(
            hub,
            new { bytes = png, width = 2, height = 2, format = "png" },
            "desktop_screenshot"));

        var snap = hub.GetSnapshot();
        Assert.Equal(VictoriaBrowserViewHub.BackendVboxGuest, snap.Backend);
        Assert.Equal(2, snap.FrameWidth);
        Assert.Equal(2, snap.FrameHeight);
        Assert.True(hub.TryGetImageBytes(out var bytes, out _));
        Assert.Equal(png, bytes);
    }

    [Fact]
    public void RecordCursor_ExposesPinkTealState()
    {
        var hub = new VictoriaBrowserViewHub();
        hub.RecordCursor(40, 30, VictoriaBrowserViewHub.CursorIdle);
        var idle = hub.GetSnapshot();
        Assert.Equal(40, idle.CursorX);
        Assert.Equal(30, idle.CursorY);
        Assert.Equal(VictoriaBrowserViewHub.CursorIdle, idle.CursorState);
        Assert.NotNull(idle.CursorAt);

        hub.RecordCursor(80, 60, VictoriaBrowserViewHub.CursorClick);
        var click = hub.GetSnapshot();
        Assert.Equal(80, click.CursorX);
        Assert.Equal(60, click.CursorY);
        Assert.Equal(VictoriaBrowserViewHub.CursorClick, click.CursorState);
    }

    [Theory]
    [InlineData("clicked left at (10,20)", "click")]
    [InlineData("agent cursor → (10,20)", "idle")]
    [InlineData("dragged left from (1,1) to (2,2)", "click")]
    [InlineData("typed 3 character(s)", "idle")]
    public void InferSoftCursorState_FromDesktopAction(string action, string expected) =>
        Assert.Equal(expected, DesktopViewHub.InferSoftCursorState(action));

    [Theory]
    [InlineData("vbox-guest", "none", true)]
    [InlineData("playwright", "vm", true)]
    [InlineData("playwright", "playwright", false)]
    [InlineData("playwright", "none", false)]
    public void WantsPresenceSoftCursor_OnlyVmOrGuest(string backend, string surface, bool expected) =>
        Assert.Equal(expected, VictoriaBrowserViewHub.WantsPresenceSoftCursor(backend, surface));

    [Fact]
    public void DesktopViewHub_MirrorsCursorToBrowserHub()
    {
        var browser = new VictoriaBrowserViewHub();
        var desktop = new DesktopViewHub(mirrorCursor: (x, y, state) => browser.RecordCursor(x, y, state));
        desktop.RecordAction("clicked left at (12,34)", 12, 34);
        var snap = browser.GetSnapshot();
        Assert.Equal(12, snap.CursorX);
        Assert.Equal(34, snap.CursorY);
        Assert.Equal(VictoriaBrowserViewHub.CursorClick, snap.CursorState);
    }
}
