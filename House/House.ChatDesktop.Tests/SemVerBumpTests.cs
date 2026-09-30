using House.ChatDesktop.Services;
using Xunit;

namespace House.ChatDesktop.Tests;

public class SemVerBumpTests
{
    [Theory]
    [InlineData("0.1.0", "patch", "0.1.1")]
    [InlineData("0.1.1", "minor", "0.2.0")]
    [InlineData("0.2.0", "major", "1.0.0")]
    [InlineData("1.2.3+git", "patch", "1.2.4")]
    [InlineData("2.0.0-beta", "patch", "2.0.1")]
    public void Bump_IncrementsRequestedPart(string input, string part, string expected)
    {
        Assert.Equal(expected, SemVerBump.Bump(input, part));
    }
}
