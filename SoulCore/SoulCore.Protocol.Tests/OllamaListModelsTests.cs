using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoulCore.Config;
using SoulCore.Inference.Clients;

namespace SoulCore.Protocol.Tests;

public class OllamaListModelsTests
{
    [Fact]
    public async Task ListLocalModelsAsync_ParsesTags()
    {
        var handler = new FixedHandler(
            HttpStatusCode.OK,
            """{"models":[{"name":"gemma4:latest"},{"name":"qwen2.5:0.5b"},{"name":"gemma4:latest"}]}""");
        var client = CreateClient(handler);

        var result = await client.ListLocalModelsAsync();

        Assert.True(result.Available);
        Assert.Null(result.Detail);
        Assert.Equal(new[] { "gemma4:latest", "qwen2.5:0.5b" }, result.Models);
        Assert.Equal("api/tags", handler.LastPath);
    }

    [Fact]
    public async Task ListLocalModelsAsync_Unreachable_ReturnsEmptyFailSoft()
    {
        var handler = new ThrowingHandler();
        var client = CreateClient(handler);

        var result = await client.ListLocalModelsAsync();

        Assert.False(result.Available);
        Assert.Empty(result.Models);
        Assert.False(string.IsNullOrWhiteSpace(result.Detail));
    }

    private static OllamaInferenceClient CreateClient(HttpMessageHandler handler)
    {
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:11434/")
        };
        var opts = Options.Create(new InferenceOptions { Model = "host:latest" });
        var logger = new LoggerFactory().CreateLogger<OllamaInferenceClient>();
        return new OllamaInferenceClient(http, opts, logger, toolRegistry: null);
    }

    private sealed class FixedHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _code;
        private readonly string _body;

        public string? LastPath { get; private set; }

        public FixedHandler(HttpStatusCode code, string body)
        {
            _code = code;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastPath = request.RequestUri?.PathAndQuery.TrimStart('/');
            return Task.FromResult(new HttpResponseMessage(_code)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("connection refused");
    }
}
