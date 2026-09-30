using House.ChatDesktop.Services;
using Xunit;

namespace House.ChatDesktop.Tests;

public class VictoriaBrowserCoordMapTests
{
    [Fact]
    public void TryMap_CenterOfLetterboxedImage_MapsToImageCenter()
    {
        // 1280x800 frame in a 640x500 surface → scale=0.5, letterbox vertical (offsetY=50)
        var mapped = VictoriaBrowserCoordMap.TryMapPointerToPage(
            pointerX: 320,
            pointerY: 50 + 200, // middle of displayed image
            surfaceWidth: 640,
            surfaceHeight: 500,
            imagePixelWidth: 1280,
            imagePixelHeight: 800);

        Assert.NotNull(mapped);
        Assert.Equal(640, mapped!.Value.X);
        Assert.Equal(400, mapped.Value.Y);
    }

    [Fact]
    public void TryMap_OutsideLetterbox_ReturnsNull()
    {
        var mapped = VictoriaBrowserCoordMap.TryMapPointerToPage(
            pointerX: 10,
            pointerY: 10, // in top letterbox band
            surfaceWidth: 640,
            surfaceHeight: 500,
            imagePixelWidth: 1280,
            imagePixelHeight: 800);

        Assert.Null(mapped);
    }

    [Fact]
    public void TryMap_TopLeftOfImage_IsZeroZero()
    {
        var mapped = VictoriaBrowserCoordMap.TryMapPointerToPage(
            pointerX: 0,
            pointerY: 50,
            surfaceWidth: 640,
            surfaceHeight: 500,
            imagePixelWidth: 1280,
            imagePixelHeight: 800);

        Assert.NotNull(mapped);
        Assert.Equal(0, mapped!.Value.X);
        Assert.Equal(0, mapped.Value.Y);
    }

    [Fact]
    public void FormatClickHint_MatchesWhatKayleighSays()
    {
        Assert.Equal("click (412, 277)", VictoriaBrowserCoordMap.FormatClickHint(412, 277));
    }
}
