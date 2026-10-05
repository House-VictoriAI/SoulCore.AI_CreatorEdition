using System.Text.Json;
using SoulCore.Host.Inference;

namespace SoulCore.Protocol.Tests;

/// <summary>
/// PROP-15.15: models endpoint payload uses one shape and canonical JSON keys.
/// </summary>
public class InferenceModelsPayloadTests
{
    [Fact]
    public void BuildModelsPayload_UsesCanonicalHostAndResolvedKeys()
    {
        var payload = InferenceApiEndpoints.BuildModelsPayload(
            models: new[] { "gemma4:latest", "mommy:latest" },
            available: true,
            detail: null,
            hostModel: "gemma4:latest",
            personaInferenceModel: "mommy:latest",
            resolvedInferenceModel: "mommy:latest",
            baseUrl: "loopback");

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(payload));
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.Array, root.GetProperty("models").ValueKind);
        Assert.Equal(2, root.GetProperty("models").GetArrayLength());
        Assert.True(root.GetProperty("available").GetBoolean());
        Assert.Equal("gemma4:latest", root.GetProperty("hostModel").GetString());
        Assert.Equal("mommy:latest", root.GetProperty("personaInferenceModel").GetString());
        Assert.Equal("mommy:latest", root.GetProperty("resolvedInferenceModel").GetString());
        Assert.Equal("loopback", root.GetProperty("baseUrl").GetString());
        Assert.False(root.TryGetProperty("resolvedModel", out _));
        Assert.False(root.TryGetProperty("hostInferenceModel", out _));
    }

    [Fact]
    public void BuildModelsPayload_FailSoftBranch_SameKeys()
    {
        var payload = InferenceApiEndpoints.BuildModelsPayload(
            models: Array.Empty<string>(),
            available: false,
            detail: "Inference is disabled on Host.",
            hostModel: "gemma4:latest",
            personaInferenceModel: null,
            resolvedInferenceModel: "gemma4:latest",
            baseUrl: "loopback");

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(payload));
        var root = doc.RootElement;

        Assert.Empty(root.GetProperty("models").EnumerateArray());
        Assert.False(root.GetProperty("available").GetBoolean());
        Assert.Equal("Inference is disabled on Host.", root.GetProperty("detail").GetString());
        Assert.Equal("gemma4:latest", root.GetProperty("hostModel").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("personaInferenceModel").ValueKind);
        Assert.Equal("gemma4:latest", root.GetProperty("resolvedInferenceModel").GetString());
        Assert.False(root.TryGetProperty("resolvedModel", out _));
    }
}
