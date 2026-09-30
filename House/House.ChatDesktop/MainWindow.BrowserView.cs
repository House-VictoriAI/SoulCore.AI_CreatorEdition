using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using House.ChatDesktop.Services;

namespace House.ChatDesktop;

public partial class MainWindow
{
    private int _browserImagePixelWidth;
    private int _browserImagePixelHeight;
    private string? _lastHoverClickHint;

    private async Task RefreshVictoriaBrowserViewAsync()
    {
        if (_browserViewBusy) return;
        if (PresenceView is { IsVisible: false }) return;

        _browserViewBusy = true;
        try
        {
            var snap = await _browserView.GetAsync(includeImage: true).ConfigureAwait(true);
            ApplyVictoriaBrowserView(snap);
        }
        finally
        {
            _browserViewBusy = false;
        }
    }

    private void ApplyVictoriaBrowserView(BrowserViewSnapshot snap)
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
            return;
        }

        if (VictoriaBrowserUrlText is not null)
            VictoriaBrowserUrlText.Text = string.IsNullOrWhiteSpace(snap.Url) ? "—" : snap.Url!;
        if (VictoriaBrowserTitleText is not null)
            VictoriaBrowserTitleText.Text = snap.Title ?? "";

        var when = snap.UpdatedAt?.ToLocalTime().ToString("h:mm:ss tt") ?? "-";
        VictoriaBrowserActionText.Text = string.IsNullOrWhiteSpace(snap.LastAction)
            ? $"No frame yet · {snap.Backend ?? "vbox-guest"} · {when}"
            : $"{snap.LastAction} · {snap.Backend ?? "vbox-guest"} · {when}";

        if (VictoriaBrowserWaitingText is not null)
        {
            var waiting = snap.WaitingOnYou?.Trim();
            if (!string.IsNullOrWhiteSpace(waiting))
            {
                VictoriaBrowserWaitingText.Text = "Waiting on you: " + waiting;
                VictoriaBrowserWaitingText.IsVisible = true;
            }
            else
            {
                VictoriaBrowserWaitingText.Text = "";
                VictoriaBrowserWaitingText.IsVisible = false;
            }
        }

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
