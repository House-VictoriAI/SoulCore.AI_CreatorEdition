using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SoulCore.Config;
using SoulCore.Core.Persona;
using SoulCore.Host.Persona;

namespace SoulCore.Protocol.Tests;

public class PersonaPackStoreTests
{
    [Fact]
    public async Task EnsureSeeded_DefaultsActiveToBlank_NotVictoria()
    {
        var root = CreateTempRoot();
        try
        {
            var store = new PersonaPackStore(root, NullLogger<PersonaPackStore>.Instance);
            await store.EnsureSeededAsync();

            var active = await store.GetActivePersonaIdAsync();
            Assert.Equal(PersonaPack.BlankPersonaId, active);

            var packs = await store.ListAsync();
            Assert.Contains(packs, p => p.PersonaId == PersonaPack.BlankPersonaId);
            Assert.Contains(packs, p => p.PersonaId == PersonaPack.MentorPersonaId);
            Assert.Contains(packs, p => p.PersonaId == PersonaPack.AnalystPersonaId);
            Assert.Contains(packs, p => p.PersonaId == PersonaPack.VictoriaPersonaId);

            var blank = await store.GetAsync(PersonaPack.BlankPersonaId);
            Assert.NotNull(blank);
            Assert.DoesNotContain("Victoria", blank!.IdentityBlurb ?? "", StringComparison.OrdinalIgnoreCase);

            // PROP-15.8: fresh seed writes starter VmWindowTitle for mentor/analyst.
            var mentor = await store.GetAsync(PersonaPack.MentorPersonaId);
            var analyst = await store.GetAsync(PersonaPack.AnalystPersonaId);
            Assert.Equal("mentor-sandbox", mentor!.VmWindowTitle);
            Assert.Equal("analyst-sandbox", analyst!.VmWindowTitle);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task EnsureSeeded_MergesBlankVmWindowTitle_OnStaleStarterPacks()
    {
        var root = CreateTempRoot();
        try
        {
            // Simulate QA 15.6 on-disk seeds that omit vmWindowTitle (pre-PROP-15.5 JSON).
            WriteStalePack(root, "mentor", """
                {
                  "personaId": "mentor",
                  "displayName": "Mentor",
                  "humanAddress": "Friend",
                  "contactId": "mentor",
                  "identityBlurb": "stale mentor"
                }
                """);
            WriteStalePack(root, "analyst", """
                {
                  "personaId": "analyst",
                  "displayName": "Analyst",
                  "humanAddress": "Friend",
                  "contactId": "analyst",
                  "identityBlurb": "stale analyst",
                  "vmWindowTitle": "   "
                }
                """);
            // Operator-customized title must not be clobbered.
            WriteStalePack(root, "victoria", """
                {
                  "personaId": "victoria",
                  "displayName": "Victoria",
                  "humanAddress": "Kayleigh",
                  "contactId": "victoria",
                  "vmWindowTitle": "custom-victoria-vm"
                }
                """);

            var store = new PersonaPackStore(root, NullLogger<PersonaPackStore>.Instance);
            await store.EnsureSeededAsync();

            var mentor = await store.GetAsync(PersonaPack.MentorPersonaId);
            var analyst = await store.GetAsync(PersonaPack.AnalystPersonaId);
            var victoria = await store.GetAsync(PersonaPack.VictoriaPersonaId);
            Assert.Equal("mentor-sandbox", mentor!.VmWindowTitle);
            Assert.Equal("analyst-sandbox", analyst!.VmWindowTitle);
            Assert.Equal("custom-victoria-vm", victoria!.VmWindowTitle);
            Assert.Equal("stale mentor", mentor.IdentityBlurb);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task EnsureSeeded_ActivateMentorAnalyst_PackTitleWinsOverHostVictoriaFallback()
    {
        var root = CreateTempRoot();
        try
        {
            WriteStalePack(root, "mentor", """
                { "personaId": "mentor", "displayName": "Mentor", "contactId": "mentor" }
                """);
            WriteStalePack(root, "analyst", """
                { "personaId": "analyst", "displayName": "Analyst", "contactId": "analyst" }
                """);

            var store = new PersonaPackStore(root, NullLogger<PersonaPackStore>.Instance);
            var hub = new PersonaStoreHub(root, NullLogger<PersonaStoreHub>.Instance);
            try
            {
                var session = new ActivePersonaSession(store, hub, NullLogger<ActivePersonaSession>.Instance);
                await session.InitializeAsync();

                var tools = Options.Create(new ToolsOptions
                {
                    DesktopTargetWindowTitle = "victoria-sandbox"
                });
                var persona = Options.Create(new PersonaOptions { RootDirectory = root });
                var resolver = new PersonaToolPathsResolver(session, tools, persona);

                await session.SetActiveAsync(PersonaPack.MentorPersonaId);
                Assert.Equal("mentor-sandbox", session.GetActive().VmWindowTitle);
                Assert.Equal("mentor-sandbox", resolver.ResolveDesktopTargetWindowTitle());
                Assert.DoesNotContain("victoria-sandbox", resolver.ResolveDesktopTargetWindowTitle(), StringComparison.OrdinalIgnoreCase);

                await session.SetActiveAsync(PersonaPack.AnalystPersonaId);
                Assert.Equal("analyst-sandbox", session.GetActive().VmWindowTitle);
                Assert.Equal("analyst-sandbox", resolver.ResolveDesktopTargetWindowTitle());
            }
            finally
            {
                await hub.DisposeAsync();
            }
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void TryMergeMissingToolPaths_FillsBlankOnly()
    {
        var existing = new PersonaPack
        {
            PersonaId = "mentor",
            VmWindowTitle = null,
            PlaywrightProfileDir = "  "
        };
        var starter = PersonaPack.CreateMentorStarter();
        starter.PlaywrightProfileDir = "mentor-browser";

        Assert.True(PersonaPackStore.TryMergeMissingToolPaths(existing, starter));
        Assert.Equal("mentor-sandbox", existing.VmWindowTitle);
        Assert.Equal("mentor-browser", existing.PlaywrightProfileDir);

        Assert.False(PersonaPackStore.TryMergeMissingToolPaths(existing, starter));
    }

    [Fact]
    public async Task UpsertAndSetActive_PersistsAndSwitches()
    {
        var root = CreateTempRoot();
        try
        {
            var store = new PersonaPackStore(root, NullLogger<PersonaPackStore>.Instance);
            await store.EnsureSeededAsync();

            var custom = PersonaPack.CreateBlank();
            custom.PersonaId = "custom-a";
            custom.DisplayName = "Custom A";
            custom.HumanAddress = "Friend";
            custom.Traits.Warmth = 0.9;
            await store.UpsertAsync(custom);

            await store.SetActivePersonaIdAsync("custom-a");
            Assert.Equal("custom-a", await store.GetActivePersonaIdAsync());

            var loaded = await store.GetAsync("custom-a");
            Assert.NotNull(loaded);
            Assert.Equal("Custom A", loaded!.DisplayName);
            Assert.Equal(0.9, loaded.Traits.Warmth);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task UpsertAndGet_UsesSharedPersonaMemoryPathsNormalizer()
    {
        var root = CreateTempRoot();
        try
        {
            var store = new PersonaPackStore(root, NullLogger<PersonaPackStore>.Instance);
            await store.EnsureSeededAsync();

            var custom = PersonaPack.CreateBlank();
            custom.PersonaId = "Custom_B";
            custom.DisplayName = "Custom B";
            var saved = await store.UpsertAsync(custom);
            Assert.Equal("custom_b", saved.PersonaId);
            Assert.Equal("custom_b", PersonaMemoryPaths.NormalizePersonaId("Custom_B"));

            var byMixedCase = await store.GetAsync("CUSTOM_B");
            Assert.NotNull(byMixedCase);
            Assert.Equal("custom_b", byMixedCase!.PersonaId);

            var bad = PersonaPack.CreateBlank();
            bad.PersonaId = "bad id!";
            await Assert.ThrowsAsync<ArgumentException>(() => store.UpsertAsync(bad));
            Assert.Throws<ArgumentException>(() => PersonaMemoryPaths.NormalizePersonaId("bad id!"));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task ActivePersonaSession_SetActive_UpdatesNextTurnPack()
    {
        var root = CreateTempRoot();
        try
        {
            var store = new PersonaPackStore(root, NullLogger<PersonaPackStore>.Instance);
            var hub = new PersonaStoreHub(root, NullLogger<PersonaStoreHub>.Instance);
            try
            {
                var session = new ActivePersonaSession(store, hub, NullLogger<ActivePersonaSession>.Instance);
                await session.InitializeAsync();

                Assert.Equal(PersonaPack.BlankPersonaId, session.ActivePersonaId);
                Assert.Equal(PersonaPack.BlankPersonaId, session.GetActive().PersonaId);

                await session.SetActiveAsync(PersonaPack.AnalystPersonaId);
                Assert.Equal(PersonaPack.AnalystPersonaId, session.ActivePersonaId);
                Assert.Equal("Analyst", session.GetActive().DisplayName);
                Assert.True(session.TryGet(PersonaPack.AnalystPersonaId, out var warm));
                Assert.Equal("Analyst", warm.DisplayName);
                Assert.False(session.TryGet(PersonaPack.BlankPersonaId, out _));
            }
            finally
            {
                await hub.DisposeAsync();
            }
        }
        finally
        {
            TryDelete(root);
        }
    }

    private static string CreateTempRoot() =>
        Path.Combine(Path.GetTempPath(), "soulcore-persona-" + Guid.NewGuid().ToString("N"));

    private static void WriteStalePack(string root, string personaId, string json)
    {
        var dir = Path.Combine(root, personaId);
        Directory.CreateDirectory(dir);
        // Validate JSON shape early so typos fail the test, not the store.
        _ = JsonDocument.Parse(json);
        File.WriteAllText(Path.Combine(dir, "pack.json"), json);
    }

    private static void TryDelete(string root)
    {
        try
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
