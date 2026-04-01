using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticCompany.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AgenticCompany.Infrastructure.Agents;

public class ClaudeAgentProvider : IAgentProvider
{
    public string Name => "claude";

    private readonly HttpClient _http;
    private readonly string? _apiKey;
    private readonly string _model;
    private const string AnthropicVersion = "2023-06-01";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ClaudeAgentProvider(IConfiguration configuration)
    {
        _apiKey = configuration["Agent:Claude:ApiKey"];
        _model = configuration["Agent:Claude:Model"] ?? "claude-sonnet-4-20250514";
        _http = new HttpClient
        {
            BaseAddress = new Uri("https://api.anthropic.com/v1/")
        };
    }

    public async Task<string> GenerateAsync(string prompt, string context, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new AgentProviderException("Claude provider is not configured. Set Agent:Claude:ApiKey in configuration.");

        var request = BuildRequest(prompt, context, stream: false);
        var json = JsonSerializer.Serialize(request, JsonOptions);
        using var httpRequest = CreateHttpRequest(json);

        var response = await _http.SendAsync(httpRequest, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new AgentProviderException($"Claude API error ({response.StatusCode}): {errorBody}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var textContent = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            return textContent ?? "[Empty response from Claude]";
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or IndexOutOfRangeException or InvalidOperationException)
        {
            throw new AgentProviderException($"Unexpected Claude response format: {responseJson[..Math.Min(responseJson.Length, 500)]}");
        }
    }

    public Task<IAsyncEnumerable<string>> StreamAsync(string prompt, string context, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new AgentProviderException("Claude provider is not configured. Set Agent:Claude:ApiKey in configuration.");

        return Task.FromResult(StreamInternal(prompt, context, ct));
    }

    private async IAsyncEnumerable<string> StreamInternal(string prompt, string context, [EnumeratorCancellation] CancellationToken ct)
    {
        var request = BuildRequest(prompt, context, stream: true);
        var json = JsonSerializer.Serialize(request, JsonOptions);
        using var httpRequest = CreateHttpRequest(json);

        var response = await _http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            yield return $"[Claude API error ({response.StatusCode}): {errorBody}]";
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null)
        {
            if (string.IsNullOrEmpty(line)) continue;
            if (!line.StartsWith("data: ")) continue;

            var data = line["data: ".Length..];

            using var doc = JsonDocument.Parse(data);
            var type = doc.RootElement.GetProperty("type").GetString();

            if (type == "content_block_delta")
            {
                var delta = doc.RootElement.GetProperty("delta");
                if (delta.TryGetProperty("text", out var textElement))
                {
                    var chunk = textElement.GetString();
                    if (!string.IsNullOrEmpty(chunk))
                        yield return chunk;
                }
            }
            else if (type == "message_stop")
            {
                break;
            }
        }
    }

    private object BuildRequest(string prompt, string context, bool stream)
    {
        var userContent = string.IsNullOrEmpty(context)
            ? prompt
            : $"{prompt}\n\n---\nContext:\n{context}";

        return new
        {
            model = _model,
            max_tokens = 4096,
            messages = new[]
            {
                new { role = "user", content = userContent }
            },
            system = "You are an expert organizational strategist and spec-driven development assistant.",
            stream
        };
    }

    private HttpRequestMessage CreateHttpRequest(string jsonBody)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "messages")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", AnthropicVersion);
        return request;
    }

    private static async IAsyncEnumerable<string> SingleChunk(string message)
    {
        yield return message;
        await Task.CompletedTask;
    }
}
