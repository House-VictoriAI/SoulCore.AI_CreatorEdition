using SoulCore.Core.Persona;

namespace SoulCore.Protocol.Tests;

public class PersonaTraitCompilerTests
{
    [Theory]
    [InlineData(0.0, PersonaTraitBand.Low)]
    [InlineData(0.33, PersonaTraitBand.Low)]
    [InlineData(0.34, PersonaTraitBand.Mid)]
    [InlineData(0.66, PersonaTraitBand.Mid)]
    [InlineData(0.67, PersonaTraitBand.High)]
    [InlineData(1.0, PersonaTraitBand.High)]
    public void ToBand_SnapsContinuousScale(double scale, PersonaTraitBand expected)
    {
        Assert.Equal(expected, PersonaTraitCompiler.ToBand(scale));
    }

    [Fact]
    public void Compile_DifferentBands_ProduceDifferentDirectiveStrings()
    {
        var low = PersonaPack.CreateBlank();
        low.Traits.Warmth = 0.1;
        low.Traits.Directness = 0.1;
        low.Traits.Formality = 0.1;
        low.Traits.Playfulness = 0.1;
        low.Traits.BoundaryStrictness = 0.1;
        low.Traits.RecallBias = 0.1;

        var high = PersonaPack.CreateBlank();
        high.Traits.Warmth = 0.95;
        high.Traits.Directness = 0.95;
        high.Traits.Formality = 0.95;
        high.Traits.Playfulness = 0.95;
        high.Traits.BoundaryStrictness = 0.95;
        high.Traits.RecallBias = 0.95;

        var lowText = PersonaTraitCompiler.Compile(low);
        var highText = PersonaTraitCompiler.Compile(high);

        Assert.StartsWith(PersonaTraitCompiler.Marker, lowText, StringComparison.Ordinal);
        Assert.StartsWith(PersonaTraitCompiler.Marker, highText, StringComparison.Ordinal);
        Assert.NotEqual(lowText, highText);
        Assert.Contains("cool and reserved", lowText, StringComparison.Ordinal);
        Assert.Contains("warm and affirming", highText, StringComparison.Ordinal);
        Assert.Contains("Flexible boundaries", lowText, StringComparison.Ordinal);
        Assert.Contains("Strict boundaries", highText, StringComparison.Ordinal);
        Assert.Contains("Prefer the present exchange", lowText, StringComparison.Ordinal);
        Assert.Contains("Bias toward continuity", highText, StringComparison.Ordinal);
    }
}
