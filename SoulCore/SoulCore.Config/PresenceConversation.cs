namespace SoulCore.Config;

/// <summary>
/// The One Thread conversation id — the single operator &lt;-&gt; Victoria conversation
/// spanning desk, SMS and phone.
/// </summary>
/// <remarks>
/// This string was previously duplicated as a literal in <c>SmsOptions</c>,
/// <c>SmsInboundService</c>, <c>CompanionApiEndpoints</c>, ChatDesktop and the runbooks,
/// and the Android client used a different value entirely (<c>companion-android</c>),
/// which is why phone and desk were two conversations. New code should reference this
/// constant rather than re-typing it.
/// </remarks>
public static class PresenceConversation
{
    public const string Id = "presence-local";
}
