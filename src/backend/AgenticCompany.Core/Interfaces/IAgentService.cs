namespace AgenticCompany.Core.Interfaces;

public interface IAgentService
{
    Task<string> GenerateAsync(string providerName, string prompt, string context, CancellationToken ct = default);
    IReadOnlyList<string> GetAvailableProviders();
}
