using AgenticCompany.Core.Interfaces;
using Microsoft.Extensions.Hosting;

namespace AgenticCompany.Infrastructure.Agents;

public class AgentService : IAgentService
{
    private readonly Dictionary<string, IAgentProvider> _providers;
    private readonly IHostEnvironment _env;

    public AgentService(IEnumerable<IAgentProvider> providers, IHostEnvironment env)
    {
        _providers = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        _env = env;
    }

    public async Task<string> GenerateAsync(string providerName, string prompt, string context, CancellationToken ct = default)
    {
        if ((string.IsNullOrWhiteSpace(providerName) || providerName.Equals("echo", StringComparison.OrdinalIgnoreCase))
            && !_env.IsDevelopment())
        {
            throw new AgentProviderException(
                "No AI provider configured. Set Agent:DefaultProvider to 'openai' or 'claude'.");
        }

        if (!_providers.TryGetValue(providerName, out var provider))
        {
            throw new AgentProviderException(
                $"Unknown agent provider: '{providerName}'. Available providers: {string.Join(", ", _providers.Keys)}");
        }

        return await provider.GenerateAsync(prompt, context, ct);
    }

    public IReadOnlyList<string> GetAvailableProviders()
        => _providers.Keys.ToList().AsReadOnly();
}
