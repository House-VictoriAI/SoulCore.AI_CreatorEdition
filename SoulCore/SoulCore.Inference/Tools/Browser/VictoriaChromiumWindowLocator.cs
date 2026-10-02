using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SoulCore.Inference.Tools.Browser;

/// <summary>
/// PROP-14.2: locate Victoria's headed Playwright Chromium top-level HWND on Windows.
/// Never attaches to Kayleigh's Google Chrome / Edge (path filter = ms-playwright chromium).
/// </summary>
public static class VictoriaChromiumWindowLocator
{
    public const string ChromeWidgetClass = "Chrome_WidgetWin_1";

    public sealed record FoundWindow(nint Hwnd, int Pid, string Title, string ExePath);

    /// <summary>
    /// Best-effort find of the Victoria Chromium top-level window.
    /// Returns null on non-Windows or when no matching window is visible.
    /// </summary>
    public static FoundWindow? TryFind()
    {
        if (!OperatingSystem.IsWindows())
            return null;

        FoundWindow? best = null;
        EnumWindows((hwnd, _) =>
        {
            if (!IsWindowVisible(hwnd))
                return true;
            if (GetWindow(hwnd, GW_OWNER) != nint.Zero)
                return true;

            var className = GetClass(hwnd);
            if (!string.Equals(className, ChromeWidgetClass, StringComparison.Ordinal))
                return true;

            GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid == 0)
                return true;

            string? exe;
            try
            {
                using var proc = Process.GetProcessById(unchecked((int)pid));
                exe = proc.MainModule?.FileName;
            }
            catch
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(exe))
                return true;
            if (!IsVictoriaPlaywrightChromium(exe))
                return true;

            var title = GetTitle(hwnd);
            // Prefer a real content window over blank/new-tabless chrome shells.
            var score = string.IsNullOrWhiteSpace(title) ? 0 : 1;
            if (title.Contains("SoulCore", StringComparison.OrdinalIgnoreCase)
                || title.Contains("Victoria", StringComparison.OrdinalIgnoreCase))
                score += 2;

            var candidate = new FoundWindow(hwnd, unchecked((int)pid), title, exe);
            if (best is null || score > Score(best))
                best = candidate;
            return true;

            static int Score(FoundWindow w) =>
                string.IsNullOrWhiteSpace(w.Title) ? 0
                : w.Title.Contains("SoulCore", StringComparison.OrdinalIgnoreCase)
                  || w.Title.Contains("Victoria", StringComparison.OrdinalIgnoreCase)
                    ? 3
                    : 1;
        }, 0);

        return best;
    }

    public static bool IsVictoriaPlaywrightChromium(string exePath)
    {
        var p = exePath.Replace('/', '\\');
        if (p.Contains(@"\Google\Chrome\", StringComparison.OrdinalIgnoreCase)
            || p.Contains(@"\Microsoft\Edge\", StringComparison.OrdinalIgnoreCase)
            || p.Contains(@"\Chrome Core\", StringComparison.OrdinalIgnoreCase))
            return false;

        return p.Contains("ms-playwright", StringComparison.OrdinalIgnoreCase)
               && p.Contains("chromium", StringComparison.OrdinalIgnoreCase)
               && p.EndsWith("chrome.exe", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetClass(nint hwnd)
    {
        var sb = new StringBuilder(256);
        _ = GetClassName(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private static string GetTitle(nint hwnd)
    {
        var len = GetWindowTextLength(hwnd);
        if (len <= 0)
            return "";
        var sb = new StringBuilder(len + 1);
        _ = GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private const uint GW_OWNER = 4;

    private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(nint hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(nint hWnd);

    [DllImport("user32.dll")]
    private static extern nint GetWindow(nint hWnd, uint uCmd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);
}
