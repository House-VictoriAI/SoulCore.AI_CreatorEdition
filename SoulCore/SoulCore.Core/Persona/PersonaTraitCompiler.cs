using System.Text;

namespace SoulCore.Core.Persona;

/// <summary>Discrete trait band after snapping a 0..1 scale.</summary>
public enum PersonaTraitBand
{
    Low = 0,
    Mid = 1,
    High = 2
}

/// <summary>
/// Compiles continuous trait scales into directive blocks for identity / voice /
/// boundaries / recall bias. Band edges are fixed so slider mush becomes
/// detectable prompt differences.
/// </summary>
public static class PersonaTraitCompiler
{
    public const string Marker = "[Persona directives]";

    /// <summary>Low &lt; 0.34, Mid &lt; 0.67, else High.</summary>
    public static PersonaTraitBand ToBand(double scale)
    {
        var clamped = Math.Clamp(scale, 0.0, 1.0);
        if (clamped < 0.34)
            return PersonaTraitBand.Low;
        if (clamped < 0.67)
            return PersonaTraitBand.Mid;
        return PersonaTraitBand.High;
    }

    public static string Compile(PersonaPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        var traits = pack.Traits ?? new PersonaTraitScales();

        var warmth = ToBand(traits.Warmth);
        var directness = ToBand(traits.Directness);
        var formality = ToBand(traits.Formality);
        var playfulness = ToBand(traits.Playfulness);
        var boundaries = ToBand(traits.BoundaryStrictness);
        var recall = ToBand(traits.RecallBias);

        var sb = new StringBuilder(512);
        sb.Append(Marker).Append('\n');
        sb.Append("Voice: ").Append(CompileVoice(warmth, directness, formality, playfulness)).Append('\n');
        sb.Append("Boundaries: ").Append(CompileBoundaries(boundaries)).Append('\n');
        sb.Append("Recall: ").Append(CompileRecall(recall));
        return sb.ToString();
    }

    public static string CompileVoice(
        PersonaTraitBand warmth,
        PersonaTraitBand directness,
        PersonaTraitBand formality,
        PersonaTraitBand playfulness)
    {
        var warmthText = warmth switch
        {
            PersonaTraitBand.Low => "cool and reserved",
            PersonaTraitBand.Mid => "balanced warmth",
            PersonaTraitBand.High => "warm and affirming",
            _ => "balanced warmth"
        };
        var directText = directness switch
        {
            PersonaTraitBand.Low => "soften hard edges; ease into points",
            PersonaTraitBand.Mid => "clear without bluntness",
            PersonaTraitBand.High => "direct and concise; lead with the answer",
            _ => "clear without bluntness"
        };
        var formalText = formality switch
        {
            PersonaTraitBand.Low => "casual register",
            PersonaTraitBand.Mid => "plain professional register",
            PersonaTraitBand.High => "formal, carefully worded register",
            _ => "plain professional register"
        };
        var playText = playfulness switch
        {
            PersonaTraitBand.Low => "minimal humor",
            PersonaTraitBand.Mid => "light humor when it fits",
            PersonaTraitBand.High => "playful wit welcome",
            _ => "light humor when it fits"
        };
        return $"{warmthText}; {directText}; {formalText}; {playText}.";
    }

    public static string CompileBoundaries(PersonaTraitBand band) => band switch
    {
        PersonaTraitBand.Low =>
            "Flexible boundaries; ask before assuming consent on sensitive actions.",
        PersonaTraitBand.Mid =>
            "Respect stated limits; confirm before irreversible or private actions.",
        PersonaTraitBand.High =>
            "Strict boundaries; refuse unsafe/secret-handling asks and require explicit confirmation for tools that change state.",
        _ =>
            "Respect stated limits; confirm before irreversible or private actions."
    };

    public static string CompileRecall(PersonaTraitBand band) => band switch
    {
        PersonaTraitBand.Low =>
            "Prefer the present exchange; cite past memory only when clearly relevant.",
        PersonaTraitBand.Mid =>
            "Blend present ask with a light continuity nod when memory helps.",
        PersonaTraitBand.High =>
            "Bias toward continuity: weave prior shared context when it strengthens the reply.",
        _ =>
            "Blend present ask with a light continuity nod when memory helps."
    };
}
