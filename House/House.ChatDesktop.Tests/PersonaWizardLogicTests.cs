using House.ChatDesktop.Services;
using Xunit;

namespace House.ChatDesktop.Tests;

public sealed class PersonaWizardLogicTests
{
    private static PersonaPackInfo HostBlank() => new()
    {
        PersonaId = PersonaWizardLogic.BlankTemplateId,
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

    private static PersonaPackInfo HostMentor() => new()
    {
        PersonaId = PersonaWizardLogic.MentorTemplateId,
        DisplayName = "Mentor",
        IdentityBlurb = "You are a patient mentor who coaches with clarity and encouragement."
    };

    private static PersonaPackInfo HostAnalyst() => new()
    {
        PersonaId = PersonaWizardLogic.AnalystTemplateId,
        DisplayName = "Analyst",
        IdentityBlurb = "You are a precise analyst who prioritizes evidence, structure, and concise conclusions."
    };

    [Fact]
    public void BuildTemplates_empty_when_host_unavailable()
    {
        Assert.Empty(PersonaWizardLogic.BuildTemplates(null));
        Assert.Empty(PersonaWizardLogic.BuildTemplates(Array.Empty<PersonaPackInfo>()));
    }

    [Fact]
    public void BuildTemplates_host_only_blank_first_no_builtin_invent()
    {
        var host = new[] { HostAnalyst(), HostMentor(), HostBlank() };
        var templates = PersonaWizardLogic.BuildTemplates(host);

        Assert.Equal(3, templates.Count);
        Assert.Equal(PersonaWizardLogic.BlankTemplateId, templates[0].TemplateId);
        Assert.Equal(PersonaWizardLogic.MentorTemplateId, templates[1].TemplateId);
        Assert.Equal(PersonaWizardLogic.AnalystTemplateId, templates[2].TemplateId);
        Assert.False(templates[0].RequiresVictoria);
        Assert.DoesNotContain(templates, t => t.RequiresVictoria);
    }

    [Fact]
    public void BuildTemplates_includes_host_victoria_as_optional_starter()
    {
        var host = new[]
        {
            HostBlank(),
            new PersonaPackInfo
            {
                PersonaId = "victoria",
                DisplayName = "Victoria",
                HumanAddress = "Kayleigh",
                IdentityBlurb = "optional starter"
            }
        };

        var templates = PersonaWizardLogic.BuildTemplates(host);
        Assert.Equal("blank", templates[0].TemplateId);
        var victoria = Assert.Single(templates, t => t.TemplateId == "victoria");
        Assert.True(victoria.RequiresVictoria);
        Assert.Equal("Kayleigh", victoria.HumanAddress);
        Assert.DoesNotContain(templates, t => t.TemplateId == PersonaWizardLogic.MentorTemplateId);
    }

    [Fact]
    public void BuildTemplates_omits_missing_starters_without_inventing()
    {
        var templates = PersonaWizardLogic.BuildTemplates(new[] { HostBlank() });
        Assert.Single(templates);
        Assert.Equal(PersonaWizardLogic.BlankTemplateId, templates[0].TemplateId);
    }

    [Theory]
    [InlineData("My Mentor", "my-mentor")]
    [InlineData("Analyst 2!", "analyst-2")]
    [InlineData("  Cool_Bot  ", "cool_bot")]
    public void SuggestPersonaId_slugs_display_name(string display, string expected) =>
        Assert.Equal(expected, PersonaWizardLogic.SuggestPersonaId(display));

    [Theory]
    [InlineData("ok-id", true)]
    [InlineData("Ok-Id", false)]
    [InlineData("bad id", false)]
    [InlineData("", false)]
    public void IsValidPersonaId_enforces_host_rules(string id, bool expected) =>
        Assert.Equal(expected, PersonaWizardLogic.IsValidPersonaId(id));

    [Fact]
    public void BuildCreateWrite_uses_host_blank_template_without_victoria()
    {
        var template = PersonaWizardLogic.BuildTemplates(new[] { HostBlank() })[0];
        var write = PersonaWizardLogic.BuildCreateWrite(
            template,
            "nova",
            "Nova",
            "Friend",
            "Fresh charter",
            new PersonaTraitScalesInfo { Warmth = 0.9, Directness = 0.2 });

        Assert.Equal("nova", write.PersonaId);
        Assert.Equal("Nova", write.DisplayName);
        Assert.Equal("Friend", write.HumanAddress);
        Assert.Equal("Fresh charter", write.IdentityBlurb);
        Assert.Equal(0.9, write.Traits.Warmth);
        Assert.DoesNotContain("Victoria", write.IdentityBlurb ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildCreateQuarantineNotice_mentions_isolation()
    {
        var msg = PersonaWizardLogic.BuildCreateQuarantineNotice("Nova", "nova");
        Assert.Contains("Nova", msg, StringComparison.Ordinal);
        Assert.Contains("nova", msg, StringComparison.Ordinal);
        Assert.Contains("quarantined", msg, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Nothing is shared", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildSaveNextTurnNotice_mentions_next_turn()
    {
        var msg = PersonaWizardLogic.BuildSaveNextTurnNotice("Nova", isCreate: true);
        Assert.Contains("Created", msg, StringComparison.Ordinal);
        Assert.Contains("next chat turn", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildEditQuarantineNotice_mentions_do_not_follow()
    {
        var msg = PersonaQuarantineConfirm.BuildEditQuarantineNotice("Mentor", "mentor");
        Assert.Contains("Mentor", msg, StringComparison.Ordinal);
        Assert.Contains("quarantined", msg, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("do not follow", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatBandPreview_maps_low_mid_high_matching_compiler_cutovers()
    {
        Assert.Equal(0.34, PersonaWizardLogic.BandLowExclusiveMax);
        Assert.Equal(0.67, PersonaWizardLogic.BandMidExclusiveMax);

        var preview = PersonaWizardLogic.FormatBandPreview(new PersonaTraitScalesInfo
        {
            Warmth = 0.1,
            Directness = 0.5,
            Formality = 0.9,
            Playfulness = 0.5,
            BoundaryStrictness = 0.2,
            RecallBias = 0.8
        });
        Assert.Contains("warmth Low", preview, StringComparison.Ordinal);
        Assert.Contains("directness Mid", preview, StringComparison.Ordinal);
        Assert.Contains("formality High", preview, StringComparison.Ordinal);
    }

    [Fact]
    public void PreferHostOrBandPreview_uses_compiled_directives_when_present()
    {
        var traits = new PersonaTraitScalesInfo { Warmth = 0.1 };
        var host = "[Persona directives]\nVoice: cool";
        Assert.Equal(host, PersonaWizardLogic.PreferHostOrBandPreview(host, traits));
        Assert.StartsWith(
            "Bands —",
            PersonaWizardLogic.PreferHostOrBandPreview(null, traits),
            StringComparison.Ordinal);
    }

    [Fact]
    public void NormalizeInferenceModel_blank_means_host_default()
    {
        Assert.Null(PersonaWizardLogic.NormalizeInferenceModel(null));
        Assert.Null(PersonaWizardLogic.NormalizeInferenceModel(""));
        Assert.Null(PersonaWizardLogic.NormalizeInferenceModel("   "));
        Assert.Equal("qwen2.5:0.5b", PersonaWizardLogic.NormalizeInferenceModel(" qwen2.5:0.5b "));
    }

    [Fact]
    public void BuildModelPickerOptions_host_default_first_and_keeps_orphan_override()
    {
        var options = PersonaWizardLogic.BuildModelPickerOptions(
            new[] { "gemma4:latest", "qwen2.5:0.5b" },
            currentPackOverride: "custom:7b",
            hostDefaultModel: "gemma4:latest");

        Assert.Null(options[0].ModelId);
        Assert.Contains("Host default", options[0].Label, StringComparison.Ordinal);
        Assert.Contains("gemma4:latest", options[0].Label, StringComparison.Ordinal);
        Assert.Contains(options, o => o.ModelId == "qwen2.5:0.5b");
        Assert.Contains(options, o => o.ModelId == "custom:7b");
    }

    [Fact]
    public void FindModelOption_selects_override_or_host_default()
    {
        var options = PersonaWizardLogic.BuildModelPickerOptions(
            new[] { "a:1", "b:2" },
            currentPackOverride: "b:2");

        Assert.Equal("b:2", PersonaWizardLogic.FindModelOption(options, "b:2")!.ModelId);
        Assert.Null(PersonaWizardLogic.FindModelOption(options, null)!.ModelId);
        Assert.Null(PersonaWizardLogic.FindModelOption(options, "   ")!.ModelId);
    }

    [Fact]
    public void FormatModelResolutionHint_distinguishes_override_vs_host_default()
    {
        var inherit = PersonaWizardLogic.FormatModelResolutionHint(
            packInferenceModel: null,
            resolvedForThisPack: "gemma4:latest",
            hostModel: "gemma4:latest");
        Assert.Contains("Host default", inherit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("gemma4:latest", inherit, StringComparison.Ordinal);

        var overrideHint = PersonaWizardLogic.FormatModelResolutionHint(
            packInferenceModel: "qwen2.5:0.5b",
            resolvedForThisPack: "qwen2.5:0.5b",
            hostModel: "gemma4:latest");
        Assert.Contains("persona override", overrideHint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("this pack only", overrideHint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildCreateWrite_and_update_persist_inference_model()
    {
        var template = PersonaWizardLogic.BuildTemplates(new[] { HostBlank() })[0];
        var create = PersonaWizardLogic.BuildCreateWrite(
            template,
            "nova",
            "Nova",
            "Friend",
            "Fresh",
            new PersonaTraitScalesInfo(),
            inferenceModel: "qwen2.5:0.5b");
        Assert.Equal("qwen2.5:0.5b", create.InferenceModel);

        var blankCreate = PersonaWizardLogic.BuildCreateWrite(
            template,
            "nova2",
            "Nova2",
            "Friend",
            null,
            new PersonaTraitScalesInfo(),
            inferenceModel: null);
        Assert.Null(blankCreate.InferenceModel);

        var existing = new PersonaPackInfo
        {
            PersonaId = "nova",
            DisplayName = "Nova",
            InferenceModel = "qwen2.5:0.5b",
            Traits = new PersonaTraitScalesInfo()
        };
        var cleared = PersonaWizardLogic.BuildUpdateWrite(
            existing,
            new PersonaPackWrite
            {
                DisplayName = "Nova",
                HumanAddress = "Friend",
                InferenceModel = null,
                Traits = new PersonaTraitScalesInfo()
            });
        Assert.Null(cleared.InferenceModel);

        var kept = PersonaWizardLogic.BuildUpdateWrite(
            existing,
            new PersonaPackWrite
            {
                DisplayName = "Nova",
                HumanAddress = "Friend",
                InferenceModel = "gemma4:latest",
                Traits = new PersonaTraitScalesInfo()
            });
        Assert.Equal("gemma4:latest", kept.InferenceModel);
    }
}
