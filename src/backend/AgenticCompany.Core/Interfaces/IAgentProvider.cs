namespace AgenticCompany.Core.Interfaces;

public class AgentProviderException : Exception
{
    public AgentProviderException(string message) : base(message) { }
    public AgentProviderException(string message, Exception innerException) : base(message, innerException) { }
}

public interface IAgentProvider
{
    string Name { get; }
    Task<string> GenerateAsync(string prompt, string context, CancellationToken ct = default);
    Task<IAsyncEnumerable<string>> StreamAsync(string prompt, string context, CancellationToken ct = default);
}
