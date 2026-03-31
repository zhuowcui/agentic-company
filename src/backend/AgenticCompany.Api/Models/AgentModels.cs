namespace AgenticCompany.Api.Models;

// --- Requests ---

public record DraftSpecRequest(Guid NodeId, string Prompt, string? Provider = null);
public record DraftPlanRequest(Guid SpecId, string? Prompt = null, string? Provider = null);
public record SuggestCascadeRequest(Guid TaskId, string? Provider = null);
public record ReviewSpecRequest(Guid SpecId, string? Provider = null);

// --- Responses ---

public record DraftSpecResponse(string Draft);

public record DraftPlanResponse(string Plan, List<SuggestedTaskItem> SuggestedTasks);
public record SuggestedTaskItem(string Title, string Description);

public record SuggestCascadeResponse(Guid? SuggestedNodeId, string? SuggestedNodeName, string DraftSpec);

public record ReviewSpecResponse(bool Aligned, int Score, string Feedback, List<string> Suggestions);
