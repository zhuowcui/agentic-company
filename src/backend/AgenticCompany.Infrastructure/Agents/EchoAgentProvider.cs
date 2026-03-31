using AgenticCompany.Core.Interfaces;

namespace AgenticCompany.Infrastructure.Agents;

/// <summary>
/// A simple echo provider for testing that returns the prompt back.
/// </summary>
public class EchoAgentProvider : IAgentProvider
{
    public string Name => "echo";

    public Task<string> GenerateAsync(string prompt, string context, CancellationToken ct = default)
    {
        var response = $"[Echo Provider]\nPrompt: {prompt}\nContext length: {context.Length} chars";
        return Task.FromResult(response);
    }

    public Task<IAsyncEnumerable<string>> StreamAsync(string prompt, string context, CancellationToken ct = default)
    {
        return Task.FromResult(StreamInternal(prompt, context));
    }

    private static async IAsyncEnumerable<string> StreamInternal(string prompt, string context)
    {
        yield return $"[Echo] {prompt}";
        await Task.CompletedTask;
    }
}
