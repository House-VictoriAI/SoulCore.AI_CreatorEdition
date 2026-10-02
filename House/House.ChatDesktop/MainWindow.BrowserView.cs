using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using House.ChatDesktop.Controls;
using House.ChatDesktop.Services;

namespace House.ChatDesktop;

public partial class MainWindow
{
    private int _browserImagePixelWidth;
    private int _browserImagePixelHeight;
    private string? _lastHoverClickHint;
    private long _lastEmbedHwnd;
    private string? _lastEmbedMode;
    private VictoriaBrowserEmbedHost? _victoriaBrowserEmbedHost;

    private async Task RefreshVictoriaBrowserViewAsync()
    {
        if (_browserViewBusy) return;
        if (PresenceView is { IsVisible: false }) return;

        _browserViewBusy = true;
        try
        {
            var snap = await _browserView.GetAsync(includeImage: true).ConfigureAwait(true);
            BrowserEmbedSnapshot? embed = null;
            if (snap.EmbedPane && OperatingSystem.IsWindows())
                embed = await _browserView.GetEmbedAsync().ConfigureAwait(true);
            ApplyVictoriaBrowserView(snap, embed);
        }
        finally
        {
            _browserViewBusy = false;
        }
    }

    private void ApplyVictoriaBrowserView(BrowserViewSnapshot snap, BrowserEmbedSnapshot? embed = null)
    {
        if (VictoriaBrowserActionText is null) return;

        if (!snap.Reachable)
        {
            VictoriaBrowserActionText.Text = snap.Detail ?? "Host unreachable";
            if (VictoriaBrowserUrlText is not null) VictoriaBrowserUrlText.Text = "—";
            if (VictoriaBrowserTitleText is not null) VictoriaBrowserTitleText.Text = "";
            if (VictoriaBrowserWaitingText is not null)
            {
                VictoriaBrowserWaitingText.Text = "";
                VictoriaBrowserWaitingText.IsVisible = false;
            }

            ClearVictoriaBrowserImage();
            ClearVictoriaBrowserEmbed("Host unreachable");
            return;
        }

        if (VictoriaBrowserUrlText is not null)
            VictoriaBrowserUrlText.Text = string.IsNullOrWhiteSpace(snap.Url) ? "—" : snap.Url!;
        if (VictoriaBrowserTitleText is not null)
            VictoriaBrowserTitleText.Text = snap.Title ?? "";

        var when = snap.UpdatedAt?.ToLocalTime().ToString("h:mm:ss tt") ?? "-";
        var embedded = embed is { Mode: "embedded", Hwnd: > 0 } && OperatingSystem.IsWindows();
        var modeLabel = embedded
            ? "embedded"
            : embed?.Mode is { Length: > 0 } m && m != "disabled"
                ? m
                : (snap.Backend ?? "playwright");

        VictoriaBrowserActionText.Text = string.IsNullOrWhiteSpace(snap.LastAction)
            ? $"No frame yet · {modeLabel} · {when}"
            : $"{snap.LastAction} · {modeLabel} · {when}";

        if (VictoriaBrowserWaitingText is not null)
        {
            var waiting = snap.WaitingOnYou?.Trim();
            var embedNote = embed?.Mode is "fallback" or "capture_off"
                ? embed.Detail
                : null;
            if (!string.IsNullOrWhiteSpace(waiting) || !string.IsNullOrWhiteSpace(embedNote))
            {
                VictoriaBrowserWaitingText.Text = !string.IsNullOrWhiteSpace(waiting)
                    ? "Waiting on you: " + waiting
                    : "Embed: " + embedNote;
                VictoriaBrowserWaitingText.IsVisible = true;
            }
            else
            {
                VictoriaBrowserWaitingText.Text = "";
                VictoriaBrowserWaitingText.IsVisible = false;
            }
        }

        if (embedded)
        {
            ApplyVictoriaBrowserEmbed(embed!);
            // Hide JPEG while HWND is live — coord hover is for JPEG fallback only.
            if (VictoriaBrowserImage is not null)
                VictoriaBrowserImage.IsVisible = false;
            if (VictoriaBrowserEmptyText is not null)
                VictoriaBrowserEmptyText.IsVisible = false;
            HideVictoriaBrowserCoords();
            return;
        }

        ClearVictoriaBrowserEmbed(embed?.Detail);
        if (snap.ImageBytes is { Length: > 0 })
        {
            var hash = $"{snap.ImageBytes.Length}:{snap.UpdatedAt:O}:{snap.Url}";
            if (!string.Equals(hash, _lastBrowserImageHash, StringComparison.Ordinal))
            {
                _lastBrowserImageHash = hash;
                ShowVictoriaBrowserBitmap(snap.ImageBytes);
            }
        }
        else
        {
            ClearVictoriaBrowserImage();
        }
    }

    private void ApplyVictoriaBrowserEmbed(BrowserEmbedSnapshot embed)
    {
        var host = EnsureVictoriaBrowserEmbedHost();
        if (host is null)
            return;

        if (host.NativeHostUnavailable)
        {
            if (VictoriaBrowserWaitingText is not null && string.IsNullOrWhiteSpace(VictoriaBrowserWaitingText.Text))
            {
                VictoriaBrowserWaitingText.Text = "Embed: native host unavailable — JPEG fallback";
                VictoriaBrowserWaitingText.IsVisible = true;
            }

            return;
        }

        if (embed.Hwnd == _lastEmbedHwnd && string.Equals(_lastEmbedMode, embed.Mode, StringComparison.Ordinal))
            return;

        _lastEmbedHwnd = embed.Hwnd;
        _lastEmbedMode = embed.Mode;
        if (VictoriaBrowserEmbedSlot is not null)
            VictoriaBrowserEmbedSlot.IsVisible = true;
        host.Bind((nint)embed.Hwnd);
    }

    private VictoriaBrowserEmbedHost? EnsureVictoriaBrowserEmbedHost()
    {
        if (_victoriaBrowserEmbedHost is not null)
            return _victoriaBrowserEmbedHost;

        if (VictoriaBrowserEmbedSlot is null || !OperatingSystem.IsWindows())
            return null;

        try
        {
            _victoriaBrowserEmbedHost = new VictoriaBrowserEmbedHost
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                IsVisible = false
            };
            VictoriaBrowserEmbedSlot.Children.Add(_victoriaBrowserEmbedHost);
            PresenceStartupLog.Write("VictoriaBrowserEmbedHost created (deferred)");
            return _victoriaBrowserEmbedHost;
        }
        catch (Exception ex)
        {
            PresenceStartupLog.WriteException("EnsureVictoriaBrowserEmbedHost", ex);
            return null;
        }
    }

    private void ClearVictoriaBrowserEmbed(string? detail)
    {
        _lastEmbedHwnd = 0;
        _lastEmbedMode = detail;
        _victoriaBrowserEmbedHost?.Bind(0);
        if (VictoriaBrowserEmbedSlot is not null)
            VictoriaBrowserEmbedSlot.IsVisible = false;
    }

    private void ShowVictoriaBrowserBitmap(byte[] imageBytes)
    {
        try
        {
            using var ms = new System.IO.MemoryStream(imageBytes);
            var bmp = new Bitmap(ms);
            _browserImagePixelWidth = bmp.PixelSize.Width;
            _browserImagePixelHeight = bmp.PixelSize.Height;
            if (VictoriaBrowserImage is not null)
            {
                VictoriaBrowserImage.Source = bmp;
                VictoriaBrowserImage.IsVisible = true;
            }

            if (VictoriaBrowserEmptyText is not null)
                VictoriaBrowserEmptyText.IsVisible = false;
        }
        catch (Exception ex)
        {
            if (VictoriaBrowserActionText is not null)
                VictoriaBrowserActionText.Text = $"Image decode failed: {ex.Message}";
        }
    }

    private void ClearVictoriaBrowserImage()
    {
        if (VictoriaBrowserImage is not null)
        {
            VictoriaBrowserImage.Source = null;
            VictoriaBrowserImage.IsVisible = false;
        }

        if (VictoriaBrowserEmptyText is not null)
            VictoriaBrowserEmptyText.IsVisible = true;
        _lastBrowserImageHash = null;
        _browserImagePixelWidth = 0;
        _browserImagePixelHeight = 0;
        HideVictoriaBrowserCoords();
    }

    private void VictoriaBrowserSurface_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (VictoriaBrowserSurface is null || VictoriaBrowserImage is null || !VictoriaBrowserImage.IsVisible)
        {
            HideVictoriaBrowserCoords();
            return;
        }

        var pos = e.GetPosition(VictoriaBrowserSurface);
        var mapped = VictoriaBrowserCoordMap.TryMapPointerToPage(
            pos.X,
            pos.Y,
            VictoriaBrowserSurface.Bounds.Width,
            VictoriaBrowserSurface.Bounds.Height,
            _browserImagePixelWidth,
            _browserImagePixelHeight);

        if (mapped is null)
        {
            HideVictoriaBrowserCoords();
            return;
        }

        var (x, y) = mapped.Value;
        _lastHoverClickHint = VictoriaBrowserCoordMap.FormatClickHint(x, y);
        if (VictoriaBrowserCoordText is not null)
            VictoriaBrowserCoordText.Text = _lastHoverClickHint;
        if (VictoriaBrowserCoordBadge is not null)
            VictoriaBrowserCoordBadge.IsVisible = true;
    }

    private void VictoriaBrowserSurface_PointerExited(object? sender, PointerEventArgs e) =>
        HideVictoriaBrowserCoords();

    private async void VictoriaBrowserSurface_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_lastHoverClickHint))
            return;
        if (!e.GetCurrentPoint(VictoriaBrowserSurface).Properties.IsLeftButtonPressed)
            return;

        var text = _lastHoverClickHint!;
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is not null)
                await clipboard.SetTextAsync(text).ConfigureAwait(true);

            if (VictoriaBrowserActionText is not null)
                VictoriaBrowserActionText.Text = $"Copied — tell her: {text}";
        }
        catch
        {
            if (VictoriaBrowserActionText is not null)
                VictoriaBrowserActionText.Text = $"Tell her: {text}";
        }
    }

    private void HideVictoriaBrowserCoords()
    {
        if (VictoriaBrowserCoordBadge is not null)
            VictoriaBrowserCoordBadge.IsVisible = false;
        _lastHoverClickHint = null;
    }
}
