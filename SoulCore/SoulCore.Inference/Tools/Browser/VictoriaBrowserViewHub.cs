namespace SoulCore.Inference.Tools.Browser;

/// <summary>
/// In-memory near-live view for Presence "Her screen" (FED-196).
/// Playwright Chromium or VirtualBox guest framebuffer — not persisted to the desktop gallery.
/// </summary>
public interface IVictoriaBrowserViewHub
{
    void Publish(
        byte[] jpegOrPng,
        string? url,
        string? title,
        string? lastAction,
        string? waitingOnYou = null,
        string? backend = null);

    VictoriaBrowserViewSnapshot GetSnapshot();
    bool TryGetImageBytes(out byte[]? bytes, out string contentType);
}

public sealed class VictoriaBrowserViewSnapshot
{
    public bool HasImage { get; init; }
    public string? Url { get; init; }
    public string? Title { get; init; }
    public string? LastAction { get; init; }
    public string? WaitingOnYou { get; init; }
    public string Backend { get; init; } = "playwright";
    public DateTimeOffset? UpdatedUtc { get; init; }
}

public sealed class VictoriaBrowserViewHub : IVictoriaBrowserViewHub
{
    public const string BackendPlaywright = "playwright";
    public const string BackendVboxGuest = "vbox-guest";

    private readonly object _gate = new();
    private byte[]? _bytes;
    private string _contentType = "image/jpeg";
    private string? _url;
    private string? _title;
    private string? _lastAction;
    private string? _waiting;
    private string _backend = BackendPlaywright;
    private DateTimeOffset? _updated;

    public void Publish(
        byte[] jpegOrPng,
        string? url,
        string? title,
        string? lastAction,
        string? waitingOnYou = null,
        string? backend = null)
    {
        if (jpegOrPng is null || jpegOrPng.Length == 0)
            return;
        lock (_gate)
        {
            _bytes = jpegOrPng;
            _contentType = jpegOrPng.Length >= 3 && jpegOrPng[0] == 0xFF && jpegOrPng[1] == 0xD8
                ? "image/jpeg"
                : "image/png";
            if (url is not null) _url = url;
            if (title is not null) _title = title;
            if (lastAction is not null) _lastAction = lastAction;
            if (waitingOnYou is not null) _waiting = waitingOnYou;
            if (!string.IsNullOrWhiteSpace(backend))
                _backend = backend.Trim();
            _updated = DateTimeOffset.UtcNow;
        }
    }

    public VictoriaBrowserViewSnapshot GetSnapshot()
    {
        lock (_gate)
        {
            return new VictoriaBrowserViewSnapshot
            {
                HasImage = _bytes is { Length: > 0 },
                Url = _url,
                Title = _title,
                LastAction = _lastAction,
                WaitingOnYou = _waiting,
                Backend = _backend,
                UpdatedUtc = _updated
            };
        }
    }

    public bool TryGetImageBytes(out byte[]? bytes, out string contentType)
    {
        lock (_gate)
        {
            bytes = _bytes;
            contentType = _contentType;
            return bytes is { Length: > 0 };
        }
    }

    /// <summary>
    /// Publish guest / desktop tool image bytes into Her screen (same pixels as desktop_screenshot).
    /// Hover coords on Presence then match guest framebuffer origin 0,0.
    /// </summary>
    public static bool TryPublishFromToolData(
        IVictoriaBrowserViewHub? hub,
        object? data,
        string lastAction,
        string backend = BackendVboxGuest,
        string? url = null,
        string? title = null)
    {
        if (hub is null || data is null)
            return false;

        if (!Desktop.DesktopViewHub.TryGetImageBytesFromToolData(data, out var bytes))
            return false;

        hub.Publish(bytes, url, title ?? "victoria-sandbox", lastAction, waitingOnYou: null, backend);
        return true;
    }
}
