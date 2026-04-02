using System.ComponentModel.DataAnnotations;

namespace AgenticCompany.Api.Models;

// --- Requests ---

public record DraftSpecRequest(Guid NodeId, [Required, MaxLength(2000)] string Prompt, string? Provider = null);
public record DraftPlanRequest(Guid SpecId, [MaxLength(2000)] string? Prompt = null, string? Provider = null);
public record SuggestCascadeRequest(Guid TaskId, string? Provider = null);
public record ReviewSpecRequest(Guid SpecId, string? Provider = null);

// --- Responses ---

public record DraftSpecResponse(string Draft);

public record DraftPlanResponse(string Plan, List<SuggestedTaskItem> SuggestedTasks);
public record SuggestedTaskItem(string Title, string Description);

public record SuggestCascadeResponse(Guid? SuggestedNodeId, string? SuggestedNodeName, string DraftSpec);

public record ReviewSpecResponse(bool Aligned, int Score, string Feedback, List<string> Suggestions);
