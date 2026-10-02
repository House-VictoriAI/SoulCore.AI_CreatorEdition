using Avalonia;

namespace House.ChatDesktop.Services;

/// <summary>
/// Keeps restored Presence window bounds on a visible screen.
/// Frameless + transparent shells look like "started then nothing" when parked off-screen.
/// </summary>
public static class PresenceWindowPlacement
{
    public static bool IsPointOnAnyScreen(int x, int y, IReadOnlyList<PixelRect> screenWorkingAreas)
    {
        if (screenWorkingAreas.Count == 0)
            return true;

        var point = new PixelPoint(x, y);
        foreach (var area in screenWorkingAreas)
        {
            if (area.Contains(point))
                return true;
        }

        return false;
    }

    /// <summary>
    /// True when the window rectangle intersects at least one working area.
    /// </summary>
    public static bool IntersectsAnyScreen(PixelRect windowBounds, IReadOnlyList<PixelRect> screenWorkingAreas)
    {
        if (screenWorkingAreas.Count == 0)
            return true;

        foreach (var area in screenWorkingAreas)
        {
            if (area.Intersects(windowBounds))
                return true;
        }

        return false;
    }

    public static PixelPoint CenterOnPrimary(
        PixelSize windowSize,
        IReadOnlyList<PixelRect> screenWorkingAreas)
    {
        var area = screenWorkingAreas.Count > 0
            ? screenWorkingAreas[0]
            : new PixelRect(0, 0, 1920, 1080);

        var x = area.X + Math.Max(0, (area.Width - windowSize.Width) / 2);
        var y = area.Y + Math.Max(0, (area.Height - windowSize.Height) / 2);
        return new PixelPoint(x, y);
    }

    /// <summary>
    /// When saved coords exist and are on-screen, returns them.
    /// When saved coords are off every screen, centers on primary and sets <c>Relocated</c>.
    /// When no saved coords, returns null position so XAML CenterScreen applies.
    /// </summary>
    public static (PixelPoint? Position, bool Relocated) ResolveStartPosition(
        double? savedX,
        double? savedY,
        double width,
        double height,
        IReadOnlyList<PixelRect> screenWorkingAreas)
    {
        if (savedX is not double sx || savedY is not double sy
            || double.IsNaN(sx) || double.IsNaN(sy)
            || double.IsInfinity(sx) || double.IsInfinity(sy))
        {
            return (null, Relocated: false);
        }

        var w = Math.Max(1, (int)Math.Round(width));
        var h = Math.Max(1, (int)Math.Round(height));
        var size = new PixelSize(w, h);
        var x = (int)Math.Round(sx);
        var y = (int)Math.Round(sy);
        var bounds = new PixelRect(new PixelPoint(x, y), size);

        if (screenWorkingAreas.Count == 0
            || IsPointOnAnyScreen(x, y, screenWorkingAreas)
            || IntersectsAnyScreen(bounds, screenWorkingAreas))
        {
            return (new PixelPoint(x, y), Relocated: false);
        }

        return (CenterOnPrimary(size, screenWorkingAreas), Relocated: true);
    }
}
