namespace House.ChatDesktop.Services;

/// <summary>
/// Maps pointer position over a Uniform-stretched Her-screen frame to pixel coords
/// Kayleigh can give Victoria for <c>desktop_click</c> / <c>browser_click</c>
/// (guest framebuffer or Playwright page pixels).
/// </summary>
public static class VictoriaBrowserCoordMap
{
    /// <summary>
    /// Returns page/framebuffer (x,y) when the pointer is over the letterboxed image; otherwise null.
    /// </summary>
    public static (int X, int Y)? TryMapPointerToPage(
        double pointerX,
        double pointerY,
        double surfaceWidth,
        double surfaceHeight,
        int imagePixelWidth,
        int imagePixelHeight)
    {
        if (surfaceWidth <= 0 || surfaceHeight <= 0
            || imagePixelWidth <= 0 || imagePixelHeight <= 0)
            return null;

        var scale = Math.Min(surfaceWidth / imagePixelWidth, surfaceHeight / imagePixelHeight);
        if (scale <= 0)
            return null;

        var dispW = imagePixelWidth * scale;
        var dispH = imagePixelHeight * scale;
        var offsetX = (surfaceWidth - dispW) / 2.0;
        var offsetY = (surfaceHeight - dispH) / 2.0;

        if (pointerX < offsetX || pointerY < offsetY
            || pointerX > offsetX + dispW || pointerY > offsetY + dispH)
            return null;

        var x = (int)Math.Floor((pointerX - offsetX) / scale);
        var y = (int)Math.Floor((pointerY - offsetY) / scale);
        x = Math.Clamp(x, 0, imagePixelWidth - 1);
        y = Math.Clamp(y, 0, imagePixelHeight - 1);
        return (x, y);
    }

    public static string FormatClickHint(int x, int y) => $"click ({x}, {y})";
}
