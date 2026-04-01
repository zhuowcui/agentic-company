using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticCompany.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AgenticCompany.Infrastructure.Agents;

public class OpenAiAgentProvider : IAgentProvider
{
    public string Name => "openai";

    private readonly HttpClient _http;
    private readonly string? _apiKey;
    private readonly string _model;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenAiAgentProvider(IConfiguration configuration)
    {
        _apiKey = configuration["Agent:OpenAI:ApiKey"];
        _model = configuration["Agent:OpenAI:Model"] ?? "gpt-4o";
        _http = new HttpClient
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };
    }

    public async Task<string> GenerateAsync(string prompt, string context, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new AgentProviderException("OpenAI provider is not configured. Set Agent:OpenAI:ApiKey in configuration.");

        var request = BuildRequest(prompt, context, stream: false);
        var json = JsonSerializer.Serialize(request, JsonOptions);
        using var httpRequest = CreateHttpRequest(json);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(httpRequest, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new AgentProviderException($"OpenAI provider communication error: {ex.GetType().Name}", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new AgentProviderException($"OpenAI API error ({response.StatusCode}): {errorBody}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var messageContent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return messageContent ?? "[Empty response from OpenAI]";
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or IndexOutOfRangeException or InvalidOperationException)
        {
            throw new AgentProviderException($"Unexpected OpenAI response format: {responseJson[..Math.Min(responseJson.Length, 500)]}");
        }
    }

    public Task<IAsyncEnumerable<string>> StreamAsync(string prompt, string context, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new AgentProviderException("OpenAI provider is not configured. Set Agent:OpenAI:ApiKey in configuration.");

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
            yield return $"[OpenAI API error ({response.StatusCode}): {errorBody}]";
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
            if (data == "[DONE]") break;

            using var doc = JsonDocument.Parse(data);
            var choices = doc.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() == 0) continue;

            var delta = choices[0].GetProperty("delta");
            if (delta.TryGetProperty("content", out var contentElement))
            {
                var chunk = contentElement.GetString();
                if (!string.IsNullOrEmpty(chunk))
                    yield return chunk;
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
            messages = new[]
            {
                new { role = "system", content = "You are an expert organizational strategist and spec-driven development assistant." },
                new { role = "user", content = userContent }
            },
            temperature = 0.7,
            max_tokens = 4096,
            stream
        };
    }

    private HttpRequestMessage CreateHttpRequest(string jsonBody)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        return request;
    }

    private static async IAsyncEnumerable<string> SingleChunk(string message)
    {
        yield return message;
        await Task.CompletedTask;
    }
}
