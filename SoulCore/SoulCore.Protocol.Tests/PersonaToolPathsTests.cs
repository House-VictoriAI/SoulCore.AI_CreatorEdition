using Microsoft.Extensions.Options;
using SoulCore.Config;
using SoulCore.Core.Persona;
using SoulCore.Host.Persona;

namespace SoulCore.Protocol.Tests;

/// <summary>PROP-15.5 — pack-first VM title + Playwright profile resolution.</summary>
public class PersonaToolPathsTests
{
    [Fact]
    public void ResolveDesktopTarget_PackWinsOverHost()
    {
        var title = PersonaToolPaths.ResolveDesktopTargetWindowTitle(
            packVmWindowTitle: "mentor-sandbox",
            hostFallback: "victoria-sandbox");
        Assert.Equal("mentor-sandbox", title);
    }

    [Fact]
    public void ResolveDesktopTarget_FallsBackToHostWhenPackBlank()
    {
        var title = PersonaToolPaths.ResolveDesktopTargetWindowTitle(
            packVmWindowTitle: "  ",
            hostFallback: "victoria-sandbox");
        Assert.Equal("victoria-sandbox", title);
    }

    [Fact]
    public void ResolveDesktopTarget_EmptyWhenBothBlank_Unrestricted()
    {
        var title = PersonaToolPaths.ResolveDesktopTargetWindowTitle(null, "");
        Assert.Equal("", title);
    }

    [Fact]
    public void ResolvePlaywright_PackWinsOverHost()
    {
        var root = Path.Combine(Path.GetTempPath(), "soulcore-pt-" + Guid.NewGuid().ToString("N"));
        var packDir = Path.Combine(root, "custom-pack-browser");
        var dir = PersonaToolPaths.ResolvePlaywrightUserDataDir(
            packProfileDir: packDir,
            hostFallback: Path.Combine(root, "host-browser"),
            personasRoot: root,
            personaId: "mentor");
        Assert.Equal(Path.GetFullPath(packDir), dir);
    }

    [Fact]
    public void ResolvePlaywright_FallsBackToHostThenPersonaScoped()
    {
        var root = Path.Combine(Path.GetTempPath(), "soulcore-pt-" + Guid.NewGuid().ToString("N"));
        try
        {
            var hostDir = Path.Combine(root, "host-browser");
            var fromHost = PersonaToolPaths.ResolvePlaywrightUserDataDir(
                packProfileDir: "",
                hostFallback: hostDir,
                personasRoot: root,
                personaId: "analyst");
            Assert.Equal(Path.GetFullPath(hostDir), fromHost);

            var personaDefault = PersonaToolPaths.ResolvePlaywrightUserDataDir(
                packProfileDir: null,
                hostFallback: null,
                personasRoot: root,
                personaId: "analyst");
            Assert.Equal(
                Path.GetFullPath(Path.Combine(root, "analyst", "browser")),
                personaDefault);
            Assert.DoesNotContain("victoria-browser", personaDefault, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void NonVictoriaPack_DoesNotRequireVictoriaSandboxTitle()
    {
        var mentor = PersonaPack.CreateMentorStarter();
        Assert.Equal("mentor-sandbox", mentor.VmWindowTitle);

        var title = PersonaToolPaths.ResolveDesktopTargetWindowTitle(
            mentor.VmWindowTitle,
            hostFallback: "victoria-sandbox");
        Assert.Equal("mentor-sandbox", title);
        Assert.DoesNotContain("victoria-sandbox", title, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolver_TwoPacks_ResolveDistinctTitleAndProfile()
    {
        var root = Path.Combine(Path.GetTempPath(), "soulcore-pt-session-" + Guid.NewGuid().ToString("N"));
        try
        {
            var session = new FixedBlankPersonaSession();
            var tools = Options.Create(new ToolsOptions
            {
                DesktopTargetWindowTitle = "victoria-sandbox",
                PlaywrightUserDataDir = ""
            });
            var persona = Options.Create(new PersonaOptions { RootDirectory = root });
            var resolver = new PersonaToolPathsResolver(session, tools, persona);

            var packA = PersonaPack.CreateBlank();
            packA.PersonaId = "alpha";
            packA.VmWindowTitle = "alpha-sandbox";
            packA.PlaywrightProfileDir = Path.Combine(root, "alpha-pw");
            session.ReplaceActive(packA);

            Assert.Equal("alpha-sandbox", resolver.ResolveDesktopTargetWindowTitle());
            Assert.Equal(Path.GetFullPath(Path.Combine(root, "alpha-pw")), resolver.ResolvePlaywrightUserDataDir());

            var packB = PersonaPack.CreateBlank();
            packB.PersonaId = "beta";
            packB.VmWindowTitle = "beta-sandbox";
            packB.PlaywrightProfileDir = Path.Combine(root, "beta-pw");
            session.ReplaceActive(packB);

            Assert.Equal("beta-sandbox", resolver.ResolveDesktopTargetWindowTitle());
            Assert.Equal(Path.GetFullPath(Path.Combine(root, "beta-pw")), resolver.ResolvePlaywrightUserDataDir());
            Assert.DoesNotContain("victoria-sandbox", resolver.ResolveDesktopTargetWindowTitle(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ScopedDesktopBackend_FuncTitle_SwitchesScopePerCall()
    {
        var title = "alpha-sandbox";
        var scoped = new SoulCore.Inference.Tools.Desktop.ScopedDesktopControlBackend(
            new RecordingDesktopBackend(),
            () => title);

        Assert.True(scoped.IsActive);
        title = "";
        Assert.False(scoped.IsActive);
        title = "beta-sandbox";
        Assert.True(scoped.IsActive);
    }

    private sealed class RecordingDesktopBackend : SoulCore.Inference.Tools.Desktop.IDesktopControlBackend
    {
        public Task<SoulCore.Inference.Tools.Desktop.DesktopOpResult> ScreenshotAsync(int monitor, CancellationToken ct = default)
            => Task.FromResult(new SoulCore.Inference.Tools.Desktop.DesktopOpResult(true, "ok", null));
        public Task<SoulCore.Inference.Tools.Desktop.DesktopOpResult> ClickAsync(int x, int y, string button, int clicks = 1, CancellationToken ct = default)
            => Task.FromResult(new SoulCore.Inference.Tools.Desktop.DesktopOpResult(true, "ok", null));
        public Task<SoulCore.Inference.Tools.Desktop.DesktopOpResult> DragAsync(int x1, int y1, int x2, int y2, string button, CancellationToken ct = default)
            => Task.FromResult(new SoulCore.Inference.Tools.Desktop.DesktopOpResult(true, "ok", null));
        public Task<SoulCore.Inference.Tools.Desktop.DesktopOpResult> TypeAsync(string text, CancellationToken ct = default)
            => Task.FromResult(new SoulCore.Inference.Tools.Desktop.DesktopOpResult(true, "ok", null));
        public Task<SoulCore.Inference.Tools.Desktop.DesktopOpResult> KeyAsync(string key, CancellationToken ct = default)
            => Task.FromResult(new SoulCore.Inference.Tools.Desktop.DesktopOpResult(true, "ok", null));
        public Task<SoulCore.Inference.Tools.Desktop.DesktopOpResult> ScrollAsync(int x, int y, int deltaY, int deltaX = 0, CancellationToken ct = default)
            => Task.FromResult(new SoulCore.Inference.Tools.Desktop.DesktopOpResult(true, "ok", null));
        public Task<SoulCore.Inference.Tools.Desktop.DesktopOpResult> OpenAppAsync(string app, string? args = null, CancellationToken ct = default)
            => Task.FromResult(new SoulCore.Inference.Tools.Desktop.DesktopOpResult(true, "ok", null));
        public Task<SoulCore.Inference.Tools.Desktop.DesktopOpResult> ListWindowsAsync(CancellationToken ct = default)
            => Task.FromResult(new SoulCore.Inference.Tools.Desktop.DesktopOpResult(true, "ok", null));
        public Task<SoulCore.Inference.Tools.Desktop.DesktopOpResult> FocusWindowAsync(string title, CancellationToken ct = default)
            => Task.FromResult(new SoulCore.Inference.Tools.Desktop.DesktopOpResult(true, "ok", null));
    }
}
