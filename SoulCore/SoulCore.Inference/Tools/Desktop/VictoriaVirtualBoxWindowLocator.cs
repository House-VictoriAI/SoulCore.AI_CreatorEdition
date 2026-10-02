using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SoulCore.Inference.Tools.Desktop;

/// <summary>
/// Locate the Victoria VirtualBox guest window (victoria-sandbox) for Presence HWND embed.
/// Prefer VirtualBoxVM.exe (the running VM) over VirtualBox Manager.
/// </summary>
public static class VictoriaVirtualBoxWindowLocator
{
    public sealed record FoundWindow(nint Hwnd, int Pid, string Title, string ExePath);

    /// <summary>
    /// Best-effort find of the victoria-sandbox VirtualBox top-level window.
    /// <paramref name="titleSubstring"/> defaults to matching any VirtualBox VM when empty.
    /// </summary>
    public static FoundWindow? TryFind(string? titleSubstring = "victoria-sandbox")
    {
        if (!OperatingSystem.IsWindows())
            return null;

        var needle = (titleSubstring ?? "").Trim();
        FoundWindow? best = null;
        var bestScore = -1;

        EnumWindows((hwnd, _) =>
        {
            if (!IsWindowVisible(hwnd))
                return true;
            if (GetWindow(hwnd, GW_OWNER) != nint.Zero)
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

            if (string.IsNullOrWhiteSpace(exe) || !IsVirtualBoxUiProcess(exe))
                return true;

            var title = GetTitle(hwnd);
            if (string.IsNullOrWhiteSpace(title))
                return true;

            if (!string.IsNullOrEmpty(needle)
                && !title.Contains(needle, StringComparison.OrdinalIgnoreCase))
                return true;

            // Prefer titled Running VM windows from VirtualBoxVM.exe.
            var score = 0;
            if (exe.EndsWith("VirtualBoxVM.exe", StringComparison.OrdinalIgnoreCase))
                score += 10;
            else if (exe.EndsWith("VirtualBox.exe", StringComparison.OrdinalIgnoreCase))
                score += 2;

            if (title.Contains("[Running]", StringComparison.OrdinalIgnoreCase))
                score += 5;
            if (title.Contains("Oracle VirtualBox", StringComparison.OrdinalIgnoreCase)
                || title.Contains("VirtualBox", StringComparison.OrdinalIgnoreCase))
                score += 2;
            if (!string.IsNullOrEmpty(needle)
                && title.Contains(needle, StringComparison.OrdinalIgnoreCase))
                score += 3;

            // Prefer larger windows (actual VM UI over tiny dialogs).
            if (GetWindowRect(hwnd, out var rc))
            {
                var area = Math.Max(0, rc.Right - rc.Left) * Math.Max(0, rc.Bottom - rc.Top);
                score += Math.Min(area / 50000, 8);
            }

            var candidate = new FoundWindow(hwnd, unchecked((int)pid), title, exe);
            if (score > bestScore)
            {
                bestScore = score;
                best = candidate;
            }

            return true;
        }, 0);

        return best;
    }

    /// <summary>True for VirtualBoxVM.exe or VirtualBox.exe only (not VBoxSVC / helpers).</summary>
    public static bool IsVirtualBoxUiProcess(string exePath)
    {
        var normalized = exePath.Replace('/', '\\');
        var slash = normalized.LastIndexOf('\\');
        var name = slash >= 0 && slash < normalized.Length - 1
            ? normalized[(slash + 1)..]
            : normalized;
        return name.Equals("VirtualBoxVM.exe", StringComparison.OrdinalIgnoreCase)
               || name.Equals("VirtualBox.exe", StringComparison.OrdinalIgnoreCase);
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

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(nint hWnd);

    [DllImport("user32.dll")]
    private static extern nint GetWindow(nint hWnd, uint uCmd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(nint hWnd, out RECT lpRect);
}
