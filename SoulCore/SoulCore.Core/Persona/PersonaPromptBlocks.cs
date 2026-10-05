using System.Text;

namespace SoulCore.Core.Persona;

/// <summary>
/// Prompt fragments derived from the active <see cref="PersonaPack"/>
/// (identity address + tool guidance). Live pack edits apply next turn.
/// </summary>
public static class PersonaPromptBlocks
{
    public const string ToolMarker = "[Persona tools]";

    /// <summary>
    /// Standing address rule for the human, using the pack's <see cref="PersonaPack.HumanAddress"/>.
    /// </summary>
    public static string BuildHumanAddressRule(string humanAddress)
    {
        var name = string.IsNullOrWhiteSpace(humanAddress) ? "Friend" : humanAddress.Trim();
        return
            $"The human you are with is {name}. Address them as {name} only; " +
            "if memory or history uses any other personal name for them, ignore it.";
    }

    public static string BuildIdentityHeader(PersonaPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        var display = string.IsNullOrWhiteSpace(pack.DisplayName) ? pack.PersonaId : pack.DisplayName.Trim();
        var id = string.IsNullOrWhiteSpace(pack.PersonaId) ? PersonaPack.BlankPersonaId : pack.PersonaId.Trim();
        var sb = new StringBuilder(256);
        sb.Append("You are ").Append(display).Append(" (personaId=").Append(id).Append(").");
        if (!string.IsNullOrWhiteSpace(pack.IdentityBlurb))
            sb.Append('\n').Append(pack.IdentityBlurb.Trim());
        return sb.ToString();
    }

    /// <summary>Compact tool-loop guidance keyed by active pack (policy flags + naming).</summary>
    public static string BuildToolGuidance(PersonaPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        var display = string.IsNullOrWhiteSpace(pack.DisplayName) ? pack.PersonaId : pack.DisplayName.Trim();
        var human = string.IsNullOrWhiteSpace(pack.HumanAddress) ? "Friend" : pack.HumanAddress.Trim();
        var policy = pack.ToolPolicy ?? new PersonaToolPolicy();

        var sb = new StringBuilder(256);
        sb.Append(ToolMarker).Append('\n');
        sb.Append("Active persona: ").Append(display)
            .Append(" (personaId=").Append(pack.PersonaId.Trim()).Append("). ");
        sb.Append("When confirming tools with the human, address them as ").Append(human).Append(". ");
        sb.Append("Pack tool policy: desktop=")
            .Append(policy.AllowDesktop ? "on" : "off")
            .Append(", browser=")
            .Append(policy.AllowBrowser ? "on" : "off")
            .Append(", email=")
            .Append(policy.AllowEmail ? "on" : "off")
            .Append('.');
        // PROP-15.5: surface pack VM title / Playwright profile when set (host Tools is fallback).
        if (!string.IsNullOrWhiteSpace(pack.VmWindowTitle))
            sb.Append(" VM window title: ").Append(pack.VmWindowTitle.Trim()).Append('.');
        if (!string.IsNullOrWhiteSpace(pack.PlaywrightProfileDir))
            sb.Append(" Playwright profile: ").Append(pack.PlaywrightProfileDir.Trim()).Append('.');
        return sb.ToString();
    }

    public static string AppendToolGuidance(string? preamble, PersonaPack pack)
    {
        var baseText = string.IsNullOrWhiteSpace(preamble) ? string.Empty : preamble.TrimEnd();
        if (baseText.Contains(ToolMarker, StringComparison.Ordinal))
            return baseText;

        var block = BuildToolGuidance(pack);
        return baseText.Length == 0 ? block : baseText + "\n\n" + block;
    }
}
