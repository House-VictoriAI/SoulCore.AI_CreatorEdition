namespace SoulCore.Config;

/// <summary>
/// PROP-15.5: resolve VM window title + Playwright user-data dir from the active
/// PersonaPack, falling back to Host <c>Tools</c> options only when the pack leaves
/// the field blank.
/// </summary>
/// <remarks>
/// <para><b>DesktopTargetWindowTitle fallback</b></para>
/// <list type="number">
/// <item>Pack <c>VmWindowTitle</c> when non-whitespace</item>
/// <item>Else <c>Tools:DesktopTargetWindowTitle</c> (may be empty = unrestricted)</item>
/// </list>
/// <para><b>PlaywrightUserDataDir fallback</b></para>
/// <list type="number">
/// <item>Pack <c>PlaywrightProfileDir</c> when non-whitespace</item>
/// <item>Else <c>Tools:PlaywrightUserDataDir</c> when non-whitespace</item>
/// <item>Else persona-scoped default <c>{personasRoot}/{personaId}/browser</c>
/// (no <c>victoria-browser</c> name required for custom packs)</item>
/// </list>
/// Single-active only — no multi-VM simultaneous (Phase 4).
/// </remarks>
public static class PersonaToolPaths
{
    public const string BrowserDirectoryName = "browser";

    /// <summary>
    /// Pack VM title wins; otherwise Host Tools title (empty = unrestricted desktop scope).
    /// </summary>
    public static string ResolveDesktopTargetWindowTitle(string? packVmWindowTitle, string? hostFallback)
    {
        var fromPack = (packVmWindowTitle ?? string.Empty).Trim();
        if (fromPack.Length > 0)
            return fromPack;

        return (hostFallback ?? string.Empty).Trim();
    }

    /// <summary>
    /// Pack profile dir → Host Tools dir → <c>{personasRoot}/{personaId}/browser</c>.
    /// </summary>
    public static string ResolvePlaywrightUserDataDir(
        string? packProfileDir,
        string? hostFallback,
        string personasRoot,
        string personaId)
    {
        var fromPack = (packProfileDir ?? string.Empty).Trim();
        if (fromPack.Length > 0)
            return NormalizePath(fromPack);

        var fromHost = (hostFallback ?? string.Empty).Trim();
        if (fromHost.Length > 0)
            return NormalizePath(fromHost);

        ArgumentException.ThrowIfNullOrWhiteSpace(personasRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(personaId);
        var id = PersonaMemoryPaths.NormalizePersonaId(personaId);
        return Path.GetFullPath(Path.Combine(personasRoot, id, BrowserDirectoryName));
    }

    /// <summary>Legacy Host-only default used when no persona context is available.</summary>
    public static string ResolveLegacyPlaywrightUserDataDir(string? hostFallback)
    {
        var configured = (hostFallback ?? string.Empty).Trim();
        if (configured.Length > 0)
            return NormalizePath(configured);

        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(local))
            local = Path.GetTempPath();
        return Path.Combine(local, "SoulCore", "victoria-browser");
    }

    private static string NormalizePath(string path) =>
        Path.GetFullPath(Environment.ExpandEnvironmentVariables(path.Trim()));
}
