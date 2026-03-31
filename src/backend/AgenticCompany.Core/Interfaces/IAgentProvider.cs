namespace AgenticCompany.Core.Interfaces;

public interface IAgentProvider
{
    string Name { get; }
    Task<string> GenerateAsync(string prompt, string context, CancellationToken ct = default);
    Task<IAsyncEnumerable<string>> StreamAsync(string prompt, string context, CancellationToken ct = default);
}
