using AgenticCompany.Core.Interfaces;

namespace AgenticCompany.Infrastructure.Agents;

public class ClaudeAgentProvider : IAgentProvider
{
    public string Name => "claude";

    public Task<string> GenerateAsync(string prompt, string context, CancellationToken ct = default)
        => Task.FromResult("[Claude provider is not configured. Set the ANTHROPIC_API_KEY environment variable and restart the server.]");

    public Task<IAsyncEnumerable<string>> StreamAsync(string prompt, string context, CancellationToken ct = default)
        => Task.FromResult(ToAsyncEnumerable("[Claude provider is not configured. Set the ANTHROPIC_API_KEY environment variable and restart the server.]"));

    private static async IAsyncEnumerable<string> ToAsyncEnumerable(string message)
    {
        yield return message;
        await Task.CompletedTask;
    }
}
