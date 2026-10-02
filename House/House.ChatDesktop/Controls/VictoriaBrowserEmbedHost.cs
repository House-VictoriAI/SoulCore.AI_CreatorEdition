using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using House.ChatDesktop.Services;

namespace House.ChatDesktop.Controls;

/// <summary>
/// PROP-14.2: hosts an existing Win32 HWND (Victoria Playwright Chromium) inside Presence.
/// Non-Windows / zero hwnd → no native child (JPEG fallback stays visible).
/// Requires Windows app.manifest with supportedOS (see House.ChatDesktop/app.manifest).
/// </summary>
public sealed class VictoriaBrowserEmbedHost : NativeControlHost
{
    private nint _hwnd;
    private nint _previousParent;
    private bool _attached;
    private bool _nativeHostUnavailable;
    private nint _placeholderHwnd;

    public nint Hwnd
    {
        get => _hwnd;
        set => Bind(value);
    }

    /// <summary>True when Win32 child-host creation failed (missing manifest / OS). JPEG fallback only.</summary>
    public bool NativeHostUnavailable => _nativeHostUnavailable;

    public void Bind(nint hwnd)
    {
        if (_nativeHostUnavailable)
        {
            IsVisible = false;
            return;
        }

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        try
        {
            base.OnAttachedToVisualTree(e);
        }
        catch (InvalidOperationException ex)
        {
            // Avalonia Win32NativeControlHost.DumbWindow without supportedOS manifest.
            MarkUnavailable(ex);
        }
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (_nativeHostUnavailable || !OperatingSystem.IsWindows() || parent.Handle == 0)
            return CreatePlaceholderHandle();

        if (_hwnd == 0)
            return CreatePlaceholderOrBase(parent);

        try
        {
            _previousParent = GetParent(_hwnd);
            // CHILD style so layout lives inside Presence; keep visible.
            var originalStyle = GetWindowLong(_hwnd, GWL_STYLE);
            var childStyle = (originalStyle | WS_CHILD | WS_VISIBLE) & ~WS_POPUP;
            _ = SetWindowLong(_hwnd, GWL_STYLE, childStyle);

            Marshal.SetLastPInvokeError(0);
            if (SetParent(_hwnd, parent.Handle) == 0 && Marshal.GetLastPInvokeError() != 0)
            {
                _ = SetWindowLong(_hwnd, GWL_STYLE, originalStyle);
                return CreatePlaceholderOrBase(parent);
            }

            _attached = true;
            ResizeToHost(parent.Handle);
            return new PlatformHandle(_hwnd, "HWND");
        }
        catch (Exception ex)
        {
            PresenceStartupLog.WriteException("VictoriaBrowserEmbedHost.CreateNativeControlCore", ex);
            _attached = false;
            return CreatePlaceholderOrBase(parent);
        }
    }

    private IPlatformHandle CreatePlaceholderOrBase(IPlatformHandle parent)
    {
        try
        {
            return base.CreateNativeControlCore(parent);
        }
        catch (InvalidOperationException ex)
        {
            MarkUnavailable(ex);
            return CreatePlaceholderHandle();
        }
    }

    private IPlatformHandle CreatePlaceholderHandle()
    {
        // Zero handle: DestroyWindow is a no-op; never return the Avalonia parent HWND.
        DestroyPlaceholder();
        if (OperatingSystem.IsWindows())
        {
            // Message-only window — avoids destroying any real UI if Avalonia tears us down.
            _placeholderHwnd = CreateWindowExW(
                0,
                "Static",
                "",
                WS_POPUP,
                0, 0, 0, 0,
                HWND_MESSAGE,
                nint.Zero,
                nint.Zero,
                nint.Zero);
        }

        return new PlatformHandle(_placeholderHwnd, "HWND");
    }

    private void MarkUnavailable(Exception ex)
    {
        _nativeHostUnavailable = true;
        _attached = false;
        IsVisible = false;
        PresenceStartupLog.WriteException("VictoriaBrowserEmbedHost unavailable — JPEG fallback only", ex);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        DetachIfNeeded();
        // Do not destroy Chromium — only detach.
        if (control.Handle == _hwnd && _hwnd != 0)
            return;

        if (control.Handle == _placeholderHwnd && _placeholderHwnd != 0)
        {
            DestroyPlaceholder();
            return;
        }

        if (control.Handle != 0 && control.Handle != _hwnd)
        {
            try { base.DestroyNativeControlCore(control); }
            catch { /* ignore */ }
        }
    }

    private void DestroyPlaceholder()
    {
        if (_placeholderHwnd == 0 || !OperatingSystem.IsWindows())
            return;
        _ = DestroyWindow(_placeholderHwnd);
        _placeholderHwnd = 0;
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
    private static readonly nint HWND_MESSAGE = new(-3);

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

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint CreateWindowExW(
        int dwExStyle,
        string lpClassName,
        string lpWindowName,
        int dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        nint hWndParent,
        nint hMenu,
        nint hInstance,
        nint lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyWindow(nint hWnd);
}
