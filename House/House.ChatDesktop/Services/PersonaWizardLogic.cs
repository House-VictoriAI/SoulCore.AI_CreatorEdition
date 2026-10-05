using System.Text;
using System.Text.RegularExpressions;

namespace House.ChatDesktop.Services;

/// <summary>
/// Pure helpers for PROP-15.3 create/edit wizard (templates, id slug, save notices).
/// Host trait-band compile / next-turn apply stay server-side.
/// </summary>
public static class PersonaWizardLogic
{
    public const string BlankTemplateId = "blank";
    public const string MentorTemplateId = "mentor";
    public const string AnalystTemplateId = "analyst";

    private static readonly Regex NonIdChars = new("[^a-z0-9_-]+", RegexOptions.Compiled);

    /// <summary>
    /// Gallery: Blank always first, then Host starters (Mentor/Analyst/optional Victoria),
    /// then any other Host packs. Blank path never requires Victoria.
    /// </summary>
    public static IReadOnlyList<PersonaTemplateInfo> BuildTemplates(IReadOnlyList<PersonaPackInfo>? hostPacks)
    {
        var packs = hostPacks ?? Array.Empty<PersonaPackInfo>();
        var byId = packs
            .Where(p => !string.IsNullOrWhiteSpace(p.PersonaId))
            .GroupBy(p => p.PersonaId.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var list = new List<PersonaTemplateInfo>
        {
            ToTemplate(byId.TryGetValue(BlankTemplateId, out var blank) ? blank : BuiltInBlank())
        };

        foreach (var id in new[] { MentorTemplateId, AnalystTemplateId, "victoria" })
        {
            if (byId.TryGetValue(id, out var pack))
                list.Add(ToTemplate(pack));
            else if (id is MentorTemplateId or AnalystTemplateId)
                list.Add(ToTemplate(id == MentorTemplateId ? BuiltInMentor() : BuiltInAnalyst()));
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
        PersonaTraitScalesInfo traits)
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
            Traits = (edits.Traits ?? existing.Traits ?? new PersonaTraitScalesInfo()).Clone(),
            CharterSeedPath = existing.CharterSeedPath,
            PlaywrightProfileDir = existing.PlaywrightProfileDir,
            VmWindowTitle = existing.VmWindowTitle,
            ToolPolicy = existing.ToolPolicy?.Clone() ?? new PersonaToolPolicyInfo()
        };
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

    public static string FormatBandPreview(PersonaTraitScalesInfo traits)
    {
        ArgumentNullException.ThrowIfNull(traits);
        static string Band(double v)
        {
            var c = Math.Clamp(v, 0.0, 1.0);
            if (c < 0.34) return "Low";
            if (c < 0.67) return "Mid";
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

    private static PersonaPackInfo BuiltInBlank() => new()
    {
        PersonaId = BlankTemplateId,
        DisplayName = "Blank",
        HumanAddress = "Friend",
        IdentityBlurb =
            "You are a newly created companion with no fixed biography yet. Stay helpful and clear.",
        Traits = new PersonaTraitScalesInfo
        {
            Warmth = 0.5,
            Directness = 0.5,
            Formality = 0.4,
            Playfulness = 0.4,
            BoundaryStrictness = 0.5,
            RecallBias = 0.5
        }
    };

    private static PersonaPackInfo BuiltInMentor() => new()
    {
        PersonaId = MentorTemplateId,
        DisplayName = "Mentor",
        HumanAddress = "Friend",
        IdentityBlurb = "You are a patient mentor who coaches with clarity and encouragement.",
        Traits = new PersonaTraitScalesInfo
        {
            Warmth = 0.85,
            Directness = 0.55,
            Formality = 0.45,
            Playfulness = 0.35,
            BoundaryStrictness = 0.6,
            RecallBias = 0.7
        }
    };

    private static PersonaPackInfo BuiltInAnalyst() => new()
    {
        PersonaId = AnalystTemplateId,
        DisplayName = "Analyst",
        HumanAddress = "Friend",
        IdentityBlurb =
            "You are a precise analyst who prioritizes evidence, structure, and concise conclusions.",
        Traits = new PersonaTraitScalesInfo
        {
            Warmth = 0.25,
            Directness = 0.9,
            Formality = 0.75,
            Playfulness = 0.15,
            BoundaryStrictness = 0.7,
            RecallBias = 0.55
        }
    };
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
