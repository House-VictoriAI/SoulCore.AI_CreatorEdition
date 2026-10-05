using System.Text;
using System.Text.RegularExpressions;

namespace House.ChatDesktop.Services;

/// <summary>
/// Pure helpers for PROP-15 create/edit wizard (templates, id slug, save notices).
/// Gallery is Host-only (GET /api/personas); no offline BuiltIn pack mirrors.
/// Host trait-band compile / next-turn apply stay server-side.
/// </summary>
public static class PersonaWizardLogic
{
    /// <summary>HTTP-boundary alias for Host blank pack id (PersonaPack.BlankPersonaId).</summary>
    public const string BlankTemplateId = "blank";

    /// <summary>HTTP-boundary alias for Host mentor pack id (PersonaPack.MentorPersonaId).</summary>
    public const string MentorTemplateId = "mentor";

    /// <summary>HTTP-boundary alias for Host analyst pack id (PersonaPack.AnalystPersonaId).</summary>
    public const string AnalystTemplateId = "analyst";

    /// <summary>
    /// Must match <c>PersonaTraitCompiler.ToBand</c> Low cutover (scale &lt; this → Low).
    /// Local slider preview only — prefer Host <c>compiledDirectives</c> after GET/PUT.
    /// </summary>
    public const double BandLowExclusiveMax = 0.34;

    /// <summary>
    /// Must match <c>PersonaTraitCompiler.ToBand</c> Mid cutover (scale &lt; this → Mid, else High).
    /// </summary>
    public const double BandMidExclusiveMax = 0.67;

    public const string HostGalleryRequiredMessage =
        "Host persona packs required. Start SoulCore.Host on loopback :7700, then retry.";

    /// <summary>Combo label for blank pack InferenceModel (inherit Host Inference:Model).</summary>
    public const string HostDefaultModelLabel = "Host default";

    private static readonly Regex NonIdChars = new("[^a-z0-9_-]+", RegexOptions.Compiled);

    /// <summary>
    /// Gallery from Host packs only: Blank → Mentor → Analyst → Victoria (if present) → others.
    /// Empty when Host list is null/empty — no BuiltIn invent.
    /// </summary>
    public static IReadOnlyList<PersonaTemplateInfo> BuildTemplates(IReadOnlyList<PersonaPackInfo>? hostPacks)
    {
        var packs = hostPacks ?? Array.Empty<PersonaPackInfo>();
        if (packs.Count == 0)
            return Array.Empty<PersonaTemplateInfo>();

        var byId = packs
            .Where(p => !string.IsNullOrWhiteSpace(p.PersonaId))
            .GroupBy(p => p.PersonaId.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        if (byId.Count == 0)
            return Array.Empty<PersonaTemplateInfo>();

        var list = new List<PersonaTemplateInfo>();

        foreach (var id in new[] { BlankTemplateId, MentorTemplateId, AnalystTemplateId, "victoria" })
        {
            if (byId.TryGetValue(id, out var pack))
                list.Add(ToTemplate(pack));
        }

        foreach (var pack in packs.OrderBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase))
        {
            if (list.Any(t => string.Equals(t.TemplateId, pack.PersonaId, StringComparison.OrdinalIgnoreCase)))
                continue;
            list.Add(ToTemplate(pack));
        }

        return list;
    }

    public static string SuggestPersonaId(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return "";

        var slug = displayName.Trim().ToLowerInvariant().Replace(' ', '-');
        slug = NonIdChars.Replace(slug, "-").Trim('-', '_');
        if (slug.Length > 64)
            slug = slug[..64].TrimEnd('-', '_');
        return slug;
    }

    public static bool IsValidPersonaId(string? personaId)
    {
        if (string.IsNullOrWhiteSpace(personaId))
            return false;
        var id = personaId.Trim();
        if (id.Length is 0 or > 64)
            return false;
        foreach (var ch in id)
        {
            if (char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_')
                continue;
            return false;
        }

        // Host normalizes to lowercase; reject mixed case so UI matches persist.
        return string.Equals(id, id.ToLowerInvariant(), StringComparison.Ordinal);
    }

    public static PersonaPackWrite BuildCreateWrite(
        PersonaTemplateInfo template,
        string personaId,
        string displayName,
        string humanAddress,
        string? identityBlurb,
        PersonaTraitScalesInfo traits,
        string? inferenceModel = null)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(traits);

        var id = (personaId ?? "").Trim().ToLowerInvariant();
        var name = string.IsNullOrWhiteSpace(displayName) ? id : displayName.Trim();
        var address = string.IsNullOrWhiteSpace(humanAddress) ? "Friend" : humanAddress.Trim();
        var blurb = string.IsNullOrWhiteSpace(identityBlurb)
            ? template.IdentityBlurb
            : identityBlurb.Trim();

        return new PersonaPackWrite
        {
            PersonaId = id,
            DisplayName = name,
            HumanAddress = address,
            ContactId = id,
            IdentityBlurb = blurb,
            InferenceModel = NormalizeInferenceModel(inferenceModel),
            Traits = traits.Clone()
        };
    }

    public static PersonaPackWrite BuildUpdateWrite(PersonaPackInfo existing, PersonaPackWrite edits)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(edits);

        return new PersonaPackWrite
        {
            PersonaId = existing.PersonaId,
            DisplayName = string.IsNullOrWhiteSpace(edits.DisplayName)
                ? existing.DisplayName
                : edits.DisplayName.Trim(),
            HumanAddress = string.IsNullOrWhiteSpace(edits.HumanAddress)
                ? (existing.HumanAddress ?? "Friend")
                : edits.HumanAddress.Trim(),
            ContactId = string.IsNullOrWhiteSpace(edits.ContactId)
                ? (existing.ContactId ?? existing.PersonaId)
                : edits.ContactId.Trim(),
            IdentityBlurb = edits.IdentityBlurb,
            // Explicit edits.InferenceModel (including null) wins — blank clears pack override.
            InferenceModel = NormalizeInferenceModel(edits.InferenceModel),
            Traits = (edits.Traits ?? existing.Traits ?? new PersonaTraitScalesInfo()).Clone(),
            CharterSeedPath = existing.CharterSeedPath,
            PlaywrightProfileDir = existing.PlaywrightProfileDir,
            VmWindowTitle = existing.VmWindowTitle,
            ToolPolicy = existing.ToolPolicy?.Clone() ?? new PersonaToolPolicyInfo()
        };
    }

    /// <summary>Blank / whitespace → null (Host default inherit).</summary>
    public static string? NormalizeInferenceModel(string? model)
    {
        if (string.IsNullOrWhiteSpace(model))
            return null;
        return model.Trim();
    }

    /// <summary>
    /// Picker rows for persona settings only: Host default first, then Host tags,
    /// plus current pack override if missing from the list.
    /// </summary>
    public static IReadOnlyList<InferenceModelOption> BuildModelPickerOptions(
        IReadOnlyList<string>? hostModels,
        string? currentPackOverride,
        string? hostDefaultModel = null)
    {
        var list = new List<InferenceModelOption>
        {
            new()
            {
                ModelId = null,
                Label = string.IsNullOrWhiteSpace(hostDefaultModel)
                    ? HostDefaultModelLabel
                    : $"{HostDefaultModelLabel} ({hostDefaultModel.Trim()})"
            }
        };

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in hostModels ?? Array.Empty<string>())
        {
            var id = NormalizeInferenceModel(raw);
            if (id is null || !seen.Add(id))
                continue;
            list.Add(new InferenceModelOption { ModelId = id, Label = id });
        }

        var current = NormalizeInferenceModel(currentPackOverride);
        if (current is not null && seen.Add(current))
            list.Add(new InferenceModelOption { ModelId = current, Label = current });

        return list;
    }

    public static InferenceModelOption? FindModelOption(
        IReadOnlyList<InferenceModelOption> options,
        string? packOverride)
    {
        ArgumentNullException.ThrowIfNull(options);
        var current = NormalizeInferenceModel(packOverride);
        if (current is null)
            return options.FirstOrDefault(o => o.ModelId is null);
        return options.FirstOrDefault(o =>
                   string.Equals(o.ModelId, current, StringComparison.OrdinalIgnoreCase))
               ?? options.FirstOrDefault(o => o.ModelId is null);
    }

    /// <summary>
    /// Clarify pack override vs Host default using this pack's resolved model
    /// (not the active-session resolved model from /api/inference/models).
    /// </summary>
    public static string FormatModelResolutionHint(
        string? packInferenceModel,
        string? resolvedForThisPack,
        string? hostModel = null)
    {
        var overrideModel = NormalizeInferenceModel(packInferenceModel);
        var resolved = NormalizeInferenceModel(resolvedForThisPack)
                       ?? NormalizeInferenceModel(hostModel)
                       ?? "—";

        if (overrideModel is null)
        {
            return string.IsNullOrWhiteSpace(hostModel)
                ? $"Resolved: {resolved} (Host default — blank inherits Host Inference:Model)."
                : $"Resolved: {resolved} (Host default).";
        }

        return $"Resolved: {resolved} (persona override for this pack only).";
    }

    public static string BuildCreateQuarantineNotice(string displayName, string personaId)
    {
        var name = string.IsNullOrWhiteSpace(displayName) ? personaId : displayName.Trim();
        var id = string.IsNullOrWhiteSpace(personaId) ? "?" : personaId.Trim();
        return
            $"New persona {name} ({id}) gets its own quarantined memory, charter, and journals. " +
            "Nothing is shared with other personas.";
    }

    public static string BuildSaveNextTurnNotice(string displayName, bool isCreate)
    {
        var name = string.IsNullOrWhiteSpace(displayName) ? "persona" : displayName.Trim();
        var verb = isCreate ? "Created" : "Updated";
        return
            $"{verb} {name}. Trait scales and charter persist on the Host; " +
            "identity and directives apply on the next chat turn.";
    }

    /// <summary>
    /// Prefer Host <c>compiledDirectives</c> (after GET/PUT). Falls back to local band labels
    /// for create-before-save / live slider preview.
    /// </summary>
    public static string PreferHostOrBandPreview(string? hostCompiledDirectives, PersonaTraitScalesInfo traits)
    {
        if (!string.IsNullOrWhiteSpace(hostCompiledDirectives))
            return hostCompiledDirectives.Trim();
        return FormatBandPreview(traits);
    }

    /// <summary>
    /// Local Low/Mid/High labels for slider preview. Cutovers
    /// <see cref="BandLowExclusiveMax"/> / <see cref="BandMidExclusiveMax"/> must match
    /// PersonaTraitCompiler.ToBand — ChatDesktop stays Core-free (PROP-15.10).
    /// </summary>
    public static string FormatBandPreview(PersonaTraitScalesInfo traits)
    {
        ArgumentNullException.ThrowIfNull(traits);
        static string Band(double v)
        {
            var c = Math.Clamp(v, 0.0, 1.0);
            if (c < BandLowExclusiveMax) return "Low";
            if (c < BandMidExclusiveMax) return "Mid";
            return "High";
        }

        var sb = new StringBuilder(128);
        sb.Append("Bands — warmth ").Append(Band(traits.Warmth));
        sb.Append(", directness ").Append(Band(traits.Directness));
        sb.Append(", formality ").Append(Band(traits.Formality));
        sb.Append(", playfulness ").Append(Band(traits.Playfulness));
        sb.Append(", boundaries ").Append(Band(traits.BoundaryStrictness));
        sb.Append(", recall ").Append(Band(traits.RecallBias));
        return sb.ToString();
    }

    private static PersonaTemplateInfo ToTemplate(PersonaPackInfo pack) => new()
    {
        TemplateId = pack.PersonaId,
        DisplayName = pack.DisplayName,
        Description = Describe(pack),
        HumanAddress = pack.HumanAddress ?? "Friend",
        IdentityBlurb = pack.IdentityBlurb,
        Traits = (pack.Traits ?? new PersonaTraitScalesInfo()).Clone(),
        RequiresVictoria = string.Equals(pack.PersonaId, "victoria", StringComparison.OrdinalIgnoreCase)
    };

    private static string Describe(PersonaPackInfo pack)
    {
        if (!string.IsNullOrWhiteSpace(pack.IdentityBlurb))
        {
            var blurb = pack.IdentityBlurb.Trim();
            return blurb.Length <= 120 ? blurb : blurb[..117] + "…";
        }

        return string.Equals(pack.PersonaId, BlankTemplateId, StringComparison.OrdinalIgnoreCase)
            ? "Empty companion — no Victoria biography required."
            : $"Start from {pack.DisplayName}.";
    }
}

public sealed class PersonaTemplateInfo
{
    public string TemplateId { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Description { get; init; } = "";
    public string HumanAddress { get; init; } = "Friend";
    public string? IdentityBlurb { get; init; }
    public PersonaTraitScalesInfo Traits { get; init; } = new();
    public bool RequiresVictoria { get; init; }

    public override string ToString() => DisplayName;
}

/// <summary>PROP-15.12 — combo row for persona-scoped InferenceModel (null = Host default).</summary>
public sealed class InferenceModelOption
{
    public string? ModelId { get; init; }
    public string Label { get; init; } = PersonaWizardLogic.HostDefaultModelLabel;

    public override string ToString() => Label;
}
