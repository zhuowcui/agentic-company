using AgenticCompany.Core.Interfaces;

namespace AgenticCompany.Infrastructure.Agents;

public class OpenAiAgentProvider : IAgentProvider
{
    public string Name => "openai";

    public Task<string> GenerateAsync(string prompt, string context, CancellationToken ct = default)
        => Task.FromResult("[OpenAI provider is not configured. Set the OPENAI_API_KEY environment variable and restart the server.]");

    public Task<IAsyncEnumerable<string>> StreamAsync(string prompt, string context, CancellationToken ct = default)
        => Task.FromResult(ToAsyncEnumerable("[OpenAI provider is not configured. Set the OPENAI_API_KEY environment variable and restart the server.]"));

    private static async IAsyncEnumerable<string> ToAsyncEnumerable(string message)
    {
        yield return message;
        await Task.CompletedTask;
    }
}
