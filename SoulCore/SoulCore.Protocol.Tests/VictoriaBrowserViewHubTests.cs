using SoulCore.Inference.Tools.Browser;

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

        Assert.Equal(VictoriaBrowserViewHub.BackendVboxGuest, hub.GetSnapshot().Backend);
        Assert.True(hub.TryGetImageBytes(out var bytes, out _));
        Assert.Equal(png, bytes);
    }
}
