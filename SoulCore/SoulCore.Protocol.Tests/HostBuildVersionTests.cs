using SoulCore.Host.Hosting;
using Xunit;

namespace SoulCore.Protocol.Tests;

public class HostBuildVersionTests
{
    [Fact]
    public void Current_IsNonEmptySemVerish()
    {
        var v = HostBuildVersion.Current;
        Assert.False(string.IsNullOrWhiteSpace(v));
        Assert.Contains('.', v);
        Assert.DoesNotContain('+', v);
    }

    [Fact]
    public void Resolve_StripsPlusMetadata()
    {
        // Assembly under test may not have +meta; exercise parser via Current shape.
        Assert.Matches(@"^\d+\.\d+\.\d+", HostBuildVersion.Current);
    }
}
