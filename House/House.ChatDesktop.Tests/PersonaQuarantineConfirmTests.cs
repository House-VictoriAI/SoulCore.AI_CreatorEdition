using House.ChatDesktop.Models;
using House.ChatDesktop.Services;
using Xunit;

namespace House.ChatDesktop.Tests;

public sealed class PersonaQuarantineConfirmTests
{
    [Fact]
    public void BuildMessage_mentions_quarantine_and_persona()
    {
        var msg = PersonaQuarantineConfirm.BuildMessage("Mentor", "mentor");
        Assert.Contains("Mentor", msg, StringComparison.Ordinal);
        Assert.Contains("mentor", msg, StringComparison.Ordinal);
        Assert.Contains("quarantined", msg, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("do not follow", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AssistantDisplayName_drives_DisplayRole()
    {
        var prior = ChatMessage.AssistantDisplayName;
        try
        {
            ChatMessage.AssistantDisplayName = "Analyst";
            var msg = new ChatMessage { Role = "assistant", Text = "hi" };
            Assert.Equal("Analyst", msg.DisplayRole);
        }
        finally
        {
            ChatMessage.AssistantDisplayName = prior;
        }
    }
}
