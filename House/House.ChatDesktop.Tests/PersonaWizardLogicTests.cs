using House.ChatDesktop.Services;
using Xunit;

namespace House.ChatDesktop.Tests;

public sealed class PersonaWizardLogicTests
{
    [Fact]
    public void BuildTemplates_blank_first_without_victoria_required()
    {
        var templates = PersonaWizardLogic.BuildTemplates(Array.Empty<PersonaPackInfo>());
        Assert.NotEmpty(templates);
        Assert.Equal(PersonaWizardLogic.BlankTemplateId, templates[0].TemplateId);
        Assert.False(templates[0].RequiresVictoria);
        Assert.Contains(templates, t => t.TemplateId == PersonaWizardLogic.MentorTemplateId);
        Assert.Contains(templates, t => t.TemplateId == PersonaWizardLogic.AnalystTemplateId);
        Assert.DoesNotContain(templates, t => t.RequiresVictoria);
    }

    [Fact]
    public void BuildTemplates_includes_host_victoria_as_optional_starter()
    {
        var host = new[]
        {
            new PersonaPackInfo
            {
                PersonaId = "blank",
                DisplayName = "Blank",
                IdentityBlurb = "blank seed"
            },
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
    public void BuildCreateWrite_uses_blank_template_without_victoria()
    {
        var template = PersonaWizardLogic.BuildTemplates(null)[0];
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
    public void FormatBandPreview_maps_low_mid_high()
    {
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
}
