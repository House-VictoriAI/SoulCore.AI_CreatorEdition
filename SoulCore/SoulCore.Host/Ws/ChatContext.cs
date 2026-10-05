namespace SoulCore.Host.Ws;

/// <summary>
/// Immutable chat context assembled for one turn. Built by <see cref="ChatContextBuilder"/>
/// from parallel independent reads (memory, charter identity, emotion) plus the active persona.
/// </summary>
public sealed record ChatContext(
    string Preamble,
    IReadOnlyList<string> IdentityAnchors,
    IReadOnlyList<string> RecentMemories,
    string EmotionPreamble,
    string PersonaId);
