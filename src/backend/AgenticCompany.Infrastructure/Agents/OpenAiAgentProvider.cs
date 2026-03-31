using AgenticCompany.Core.Interfaces;

namespace AgenticCompany.Infrastructure.Agents;

public class OpenAiAgentProvider : IAgentProvider
{
    public string Name => "openai";

    public Task<string> GenerateAsync(string prompt, string context, CancellationToken ct = default)
    {
        // TODO: Implement with OpenAI API client
        throw new NotImplementedException("OpenAI provider not yet configured. Set the OpenAI API key in configuration.");
    }

    public Task<IAsyncEnumerable<string>> StreamAsync(string prompt, string context, CancellationToken ct = default)
    {
        throw new NotImplementedException("OpenAI streaming not yet configured.");
    }
}
