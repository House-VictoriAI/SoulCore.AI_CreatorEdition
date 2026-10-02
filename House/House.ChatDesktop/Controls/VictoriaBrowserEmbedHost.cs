using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace House.ChatDesktop.Controls;

/// <summary>
/// PROP-14.2: hosts an existing Win32 HWND (Victoria Playwright Chromium) inside Presence.
/// Non-Windows / zero hwnd → no native child (JPEG fallback stays visible).
/// </summary>
public sealed class VictoriaBrowserEmbedHost : NativeControlHost
{
    private nint _hwnd;
    private nint _previousParent;
    private bool _attached;

    public nint Hwnd
    {
        get => _hwnd;
        set => Bind(value);
    }

    public void Bind(nint hwnd)
    {
        if (_hwnd == hwnd && (_attached || hwnd == 0))
            return;

        DetachIfNeeded();
        _hwnd = hwnd;

        if (!OperatingSystem.IsWindows() || _hwnd == 0)
        {
            IsVisible = false;
            return;
        }

        // Toggle visibility so NativeControlHost recreates with the new HWND.
        IsVisible = false;
        IsVisible = true;
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsWindows() || _hwnd == 0 || parent.Handle == 0)
            return base.CreateNativeControlCore(parent);

        _previousParent = GetParent(_hwnd);
        // CHILD style so layout lives inside Presence; keep visible.
var originalStyle = GetWindowLong(_hwnd, GWL_STYLE);
        var childStyle = (originalStyle | WS_CHILD | WS_VISIBLE) & ~WS_POPUP;
        _ = SetWindowLong(_hwnd, GWL_STYLE, childStyle);

        Marshal.SetLastPInvokeError(0);
        if (SetParent(_hwnd, parent.Handle) == 0 && Marshal.GetLastPInvokeError() != 0)
        {
            _ = SetWindowLong(_hwnd, GWL_STYLE, originalStyle);
            return base.CreateNativeControlCore(parent);
        }

        _attached = true;
        ResizeToHost(parent.Handle);
        return new PlatformHandle(_hwnd, "HWND");
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        DetachIfNeeded();
        // Do not destroy Chromium — only detach. Base would DestroyWindow the handle.
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (_attached && _hwnd != 0 && OperatingSystem.IsWindows())
            ResizeToHost(nint.Zero);
    }

    private void ResizeToHost(nint parentHint)
    {
        if (!OperatingSystem.IsWindows() || _hwnd == 0)
            return;
        var parent = parentHint != 0 ? parentHint : GetParent(_hwnd);
        if (parent == 0)
            return;
        if (!GetClientRect(parent, out var rc))
            return;
        _ = MoveWindow(_hwnd, 0, 0, rc.Right - rc.Left, rc.Bottom - rc.Top, true);
    }

    private void DetachIfNeeded()
    {
        if (!_attached || _hwnd == 0 || !OperatingSystem.IsWindows())
        {
            _attached = false;
            return;
        }

        try
        {
            _ = SetParent(_hwnd, _previousParent);
            var style = GetWindowLong(_hwnd, GWL_STYLE);
            style = (style | WS_POPUP | WS_VISIBLE) & ~WS_CHILD;
            _ = SetWindowLong(_hwnd, GWL_STYLE, style);
            _ = ShowWindow(_hwnd, SW_SHOW);
        }
        catch
        {
            // Best-effort detach — Chromium may already be gone.
        }

        _attached = false;
        _previousParent = 0;
    }

    private const int GWL_STYLE = -16;
    private const int WS_CHILD = 0x40000000;
    private const int WS_POPUP = unchecked((int)0x80000000);
    private const int WS_VISIBLE = 0x10000000;
    private const int SW_SHOW = 5;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetParent(nint hWndChild, nint hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetParent(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(nint hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool MoveWindow(nint hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetClientRect(nint hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);
}
