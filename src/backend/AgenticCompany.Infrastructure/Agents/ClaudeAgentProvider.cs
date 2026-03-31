using AgenticCompany.Core.Interfaces;

namespace AgenticCompany.Infrastructure.Agents;

public class ClaudeAgentProvider : IAgentProvider
{
    public string Name => "claude";

    public Task<string> GenerateAsync(string prompt, string context, CancellationToken ct = default)
    {
        // TODO: Implement with Anthropic API client
        throw new NotImplementedException("Claude provider not yet configured. Set the Anthropic API key in configuration.");
    }

    public Task<IAsyncEnumerable<string>> StreamAsync(string prompt, string context, CancellationToken ct = default)
    {
        throw new NotImplementedException("Claude streaming not yet configured.");
    }
}
