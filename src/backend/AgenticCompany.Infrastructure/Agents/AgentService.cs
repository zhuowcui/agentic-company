using AgenticCompany.Core.Interfaces;

namespace AgenticCompany.Infrastructure.Agents;

public class AgentService : IAgentService
{
    private readonly Dictionary<string, IAgentProvider> _providers;

    public AgentService(IEnumerable<IAgentProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string> GenerateAsync(string providerName, string prompt, string context, CancellationToken ct = default)
    {
        if (!_providers.TryGetValue(providerName, out var provider))
        {
            throw new ArgumentException($"Unknown agent provider: '{providerName}'. Available: {string.Join(", ", _providers.Keys)}");
        }

        return await provider.GenerateAsync(prompt, context, ct);
    }

    public IReadOnlyList<string> GetAvailableProviders()
        => _providers.Keys.ToList().AsReadOnly();
}
