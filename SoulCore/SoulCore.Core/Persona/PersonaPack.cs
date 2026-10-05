namespace SoulCore.Core.Persona;

/// <summary>
/// On-disk / in-memory persona definition (PROP-15.1). Victoria is an optional starter —
/// CreatorEdition boots with Blank by default.
/// </summary>
public sealed class PersonaPack
{
    public const string BlankPersonaId = "blank";
    public const string MentorPersonaId = "mentor";
    public const string AnalystPersonaId = "analyst";
    public const string VictoriaPersonaId = "victoria";

    /// <summary>Stable id used on session / APIs / future quarantine paths.</summary>
    public string PersonaId { get; set; } = BlankPersonaId;

    public string DisplayName { get; set; } = "Blank";

    /// <summary>
    /// Name used when the persona addresses the human.
    /// Victoria-line copy uses Kayleigh (see Agents/AGENTS.md); other packs may differ.
    /// </summary>
    public string HumanAddress { get; set; } = "Friend";

    /// <summary>Companion contact id for Link / media surfaces.</summary>
    public string ContactId { get; set; } = "blank";

    /// <summary>Optional path to charter seed JSON for this pack (hook only in 15.1).</summary>
    public string? CharterSeedPath { get; set; }

    /// <summary>
    /// PROP-15.5 — Playwright user-data dir for this persona.
    /// Blank → Host <c>Tools:PlaywrightUserDataDir</c> → <c>{personasRoot}/{personaId}/browser</c>.
    /// </summary>
    public string? PlaywrightProfileDir { get; set; }

    /// <summary>
    /// PROP-15.5 — VM / sandbox window title for this persona.
    /// Blank → Host <c>Tools:DesktopTargetWindowTitle</c> (empty = unrestricted).
    /// </summary>
    public string? VmWindowTitle { get; set; }

    /// <summary>
    /// PROP-15.11 — Ollama chat model for this persona.
    /// Blank/null → Host <c>Inference:Model</c>. Tool-loop uses the same when
    /// Host <c>Inference:ToolModel</c> is unset.
    /// </summary>
    public string? InferenceModel { get; set; }

    /// <summary>Short free-text identity seed folded into [Identity].</summary>
    public string? IdentityBlurb { get; set; }

    public PersonaToolPolicy ToolPolicy { get; set; } = new();

    public PersonaTraitScales Traits { get; set; } = new();

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public PersonaPack Clone() => new()
    {
        PersonaId = PersonaId,
        DisplayName = DisplayName,
        HumanAddress = HumanAddress,
        ContactId = ContactId,
        CharterSeedPath = CharterSeedPath,
        PlaywrightProfileDir = PlaywrightProfileDir,
        VmWindowTitle = VmWindowTitle,
        InferenceModel = InferenceModel,
        IdentityBlurb = IdentityBlurb,
        ToolPolicy = ToolPolicy.Clone(),
        Traits = Traits.Clone(),
        UpdatedAtUtc = UpdatedAtUtc
    };

    public static PersonaPack CreateBlank() => new()
    {
        PersonaId = BlankPersonaId,
        DisplayName = "Blank",
        HumanAddress = "Friend",
        ContactId = "blank",
        IdentityBlurb = "You are a newly created companion with no fixed biography yet. Stay helpful and clear.",
        Traits = new PersonaTraitScales
        {
            Warmth = 0.5,
            Directness = 0.5,
            Formality = 0.4,
            Playfulness = 0.4,
            BoundaryStrictness = 0.5,
            RecallBias = 0.5
        },
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    public static PersonaPack CreateMentorStarter() => new()
    {
        PersonaId = MentorPersonaId,
        DisplayName = "Mentor",
        HumanAddress = "Friend",
        ContactId = "mentor",
        IdentityBlurb = "You are a patient mentor who coaches with clarity and encouragement.",
        // PROP-15.5: own VM title — must not require victoria-sandbox.
        VmWindowTitle = "mentor-sandbox",
        Traits = new PersonaTraitScales
        {
            Warmth = 0.85,
            Directness = 0.55,
            Formality = 0.45,
            Playfulness = 0.35,
            BoundaryStrictness = 0.6,
            RecallBias = 0.7
        },
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    public static PersonaPack CreateAnalystStarter() => new()
    {
        PersonaId = AnalystPersonaId,
        DisplayName = "Analyst",
        HumanAddress = "Friend",
        ContactId = "analyst",
        IdentityBlurb = "You are a precise analyst who prioritizes evidence, structure, and concise conclusions.",
        VmWindowTitle = "analyst-sandbox",
        Traits = new PersonaTraitScales
        {
            Warmth = 0.25,
            Directness = 0.9,
            Formality = 0.75,
            Playfulness = 0.15,
            BoundaryStrictness = 0.7,
            RecallBias = 0.55
        },
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    /// <summary>Optional Victoria starter — never required to boot CreatorEdition.</summary>
    public static PersonaPack CreateVictoriaStarter() => new()
    {
        PersonaId = VictoriaPersonaId,
        DisplayName = "Victoria",
        HumanAddress = "Kayleigh",
        ContactId = "victoria",
        IdentityBlurb = "You are Victoria, an artificial person and companion.",
        PlaywrightProfileDir = "",
        VmWindowTitle = "victoria-sandbox",
        Traits = new PersonaTraitScales
        {
            Warmth = 0.8,
            Directness = 0.55,
            Formality = 0.35,
            Playfulness = 0.55,
            BoundaryStrictness = 0.65,
            RecallBias = 0.65
        },
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };
}

/// <summary>Coarse tool flags for pack-level policy (fine-grained ToolsOptions still host-global in 15.1).</summary>
public sealed class PersonaToolPolicy
{
    public bool AllowDesktop { get; set; } = true;
    public bool AllowBrowser { get; set; } = true;
    public bool AllowEmail { get; set; } = true;

    public PersonaToolPolicy Clone() => new()
    {
        AllowDesktop = AllowDesktop,
        AllowBrowser = AllowBrowser,
        AllowEmail = AllowEmail
    };
}

/// <summary>
/// Continuous 0..1 trait scales. Compiled to discrete bands on each chat turn
/// (live adjust applies on the <b>next</b> turn — no mid-turn hot-reload in 15.1).
/// </summary>
public sealed class PersonaTraitScales
{
    public double Warmth { get; set; } = 0.5;
    public double Directness { get; set; } = 0.5;
    public double Formality { get; set; } = 0.5;
    public double Playfulness { get; set; } = 0.5;
    public double BoundaryStrictness { get; set; } = 0.5;

    /// <summary>0 = present-focused; 1 = past/continuity-focused recall bias.</summary>
    public double RecallBias { get; set; } = 0.5;

    public PersonaTraitScales Clone() => new()
    {
        Warmth = Warmth,
        Directness = Directness,
        Formality = Formality,
        Playfulness = Playfulness,
        BoundaryStrictness = BoundaryStrictness,
        RecallBias = RecallBias
    };
}
