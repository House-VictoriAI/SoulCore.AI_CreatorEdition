namespace SoulCore.Core.Persona;

/// <summary>
/// PROP-15.5: resolve desktop VM title + Playwright profile for the <b>active</b> PersonaPack.
/// Tool loop / desktop_* / Playwright bridges consume this instead of Victoria hardcodes.
/// </summary>
public interface IPersonaToolPathsResolver
{
    /// <summary>
    /// Active pack <see cref="PersonaPack.VmWindowTitle"/>, else Host
    /// <c>Tools:DesktopTargetWindowTitle</c>, else empty (unrestricted).
    /// </summary>
    string ResolveDesktopTargetWindowTitle();

    /// <summary>
    /// Active pack <see cref="PersonaPack.PlaywrightProfileDir"/>, else Host
    /// <c>Tools:PlaywrightUserDataDir</c>, else persona-scoped
    /// <c>{personasRoot}/{personaId}/browser</c>.
    /// </summary>
    string ResolvePlaywrightUserDataDir();
}
