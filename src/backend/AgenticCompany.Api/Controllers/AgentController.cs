using System.Text.Json;
using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Interfaces;
using AgenticCompany.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgenticCompany.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly string _defaultProvider;

    private readonly IAgentService _agentService;
    private readonly INodeRepository _nodeRepo;
    private readonly ISpecRepository _specRepo;
    private readonly IPlanRepository _planRepo;
    private readonly ITaskItemRepository _taskRepo;
    private readonly IPrincipleRepository _principleRepo;
    private readonly PrincipleInheritanceService _principleService;

    public AgentController(
        IAgentService agentService,
        INodeRepository nodeRepo,
        ISpecRepository specRepo,
        IPlanRepository planRepo,
        ITaskItemRepository taskRepo,
        IPrincipleRepository principleRepo,
        PrincipleInheritanceService principleService,
        IConfiguration configuration)
    {
        _agentService = agentService;
        _nodeRepo = nodeRepo;
        _specRepo = specRepo;
        _planRepo = planRepo;
        _taskRepo = taskRepo;
        _principleRepo = principleRepo;
        _principleService = principleService;
        _defaultProvider = configuration["Agent:DefaultProvider"] ?? "echo";
    }

    /// <summary>Generate a draft spec body using AI</summary>
    [HttpPost("draft-spec")]
    public async Task<ActionResult<DraftSpecResponse>> DraftSpec(
        [FromBody] DraftSpecRequest request, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(request.NodeId, ct);
        if (node is null) return NotFound("Node not found");

        var effectivePrinciples = await GetEffectivePrinciplesAsync(request.NodeId, ct);
        var principlesText = FormatPrinciples(effectivePrinciples);

        var prompt = $"""
            You are a spec author for an organizational hierarchy.
            Generate a detailed specification in markdown format.

            Node: {node.Name} (Type: {node.Type}, Depth: {node.Depth})
            User request: {request.Prompt}

            The spec should be well-structured with sections for Overview, Goals, Requirements, and Success Criteria.
            """;

        var context = $"""
            Effective principles for this node:
            {principlesText}
            """;

        var provider = request.Provider ?? _defaultProvider;
        var draft = await _agentService.GenerateAsync(provider, prompt, context, ct);

        return Ok(new DraftSpecResponse(draft));
    }

    /// <summary>Generate a plan with tasks from a spec</summary>
    [HttpPost("draft-plan")]
    public async Task<ActionResult<DraftPlanResponse>> DraftPlan(
        [FromBody] DraftPlanRequest request, CancellationToken ct)
    {
        var spec = await _specRepo.GetByIdAsync(request.SpecId, ct);
        if (spec is null) return NotFound("Spec not found");

        var node = await _nodeRepo.GetByIdAsync(spec.NodeId, ct);
        if (node is null) return NotFound("Node not found");

        var effectivePrinciples = await GetEffectivePrinciplesAsync(spec.NodeId, ct);
        var principlesText = FormatPrinciples(effectivePrinciples);

        var latestContent = spec.Versions?.MaxBy(v => v.Version)?.Content ?? "(no content)";

        var additionalGuidance = request.Prompt is not null ? $"Additional guidance: {request.Prompt}\n" : "";
        var jsonExample = """[{"title": "Task title", "description": "Task description"}]""";

        var prompt = $"""
            You are a planning agent for an organizational hierarchy.
            Break the following spec into a plan with discrete, actionable tasks.
            {additionalGuidance}
            Spec title: {spec.Title}
            Spec content:
            {latestContent}

            Return a plan overview followed by a JSON array of tasks.
            Format the tasks section as a JSON code block with this structure:
            ```json
            {jsonExample}
            ```
            """;

        var context = $"""
            Node: {node.Name} (Type: {node.Type}, Depth: {node.Depth})
            Effective principles:
            {principlesText}
            """;

        var provider = request.Provider ?? _defaultProvider;
        var result = await _agentService.GenerateAsync(provider, prompt, context, ct);

        var (planText, tasks) = ParsePlanResponse(result);

        return Ok(new DraftPlanResponse(planText, tasks));
    }

    /// <summary>Suggest which child node a task should cascade to</summary>
    [HttpPost("suggest-cascade")]
    public async Task<ActionResult<SuggestCascadeResponse>> SuggestCascade(
        [FromBody] SuggestCascadeRequest request, CancellationToken ct)
    {
        var task = await _taskRepo.GetByIdAsync(request.TaskId, ct);
        if (task is null) return NotFound("Task not found");

        var plan = await _planRepo.GetByIdAsync(task.PlanId, ct);
        if (plan is null) return NotFound("Plan not found");

        var spec = await _specRepo.GetByIdAsync(plan.SpecId, ct);
        if (spec is null) return NotFound("Spec not found");

        var node = await _nodeRepo.GetWithChildrenAsync(spec.NodeId, ct);
        if (node is null) return NotFound("Node not found");

        var children = node.Children?.ToList() ?? [];
        var childrenText = children.Count > 0
            ? string.Join("\n", children.Select(c => $"- {c.Id}: {c.Name} (Type: {c.Type})"))
            : "(no child nodes)";

        var latestContent = spec.Versions?.MaxBy(v => v.Version)?.Content ?? "(no content)";

        var prompt = $"""
            You are a cascade advisor for an organizational hierarchy.
            Given a task, recommend which child node it should be delegated to
            and draft a child spec for that delegation.

            Task: {task.Title}
            Task description: {task.Description ?? "(none)"}

            Parent spec: {spec.Title}
            Parent spec content:
            {latestContent}

            Available child nodes:
            {childrenText}

            Respond with:
            1. The ID and name of the recommended child node (or "none" if no suitable child exists)
            2. A draft spec for the child node in markdown

            Format your response as:
            TARGET_NODE_ID: <guid or "none">
            TARGET_NODE_NAME: <name or "none">
            DRAFT_SPEC:
            <markdown spec content>
            """;

        var context = $"Parent node: {node.Name} (Type: {node.Type}, Depth: {node.Depth})";

        var provider = request.Provider ?? _defaultProvider;
        var result = await _agentService.GenerateAsync(provider, prompt, context, ct);

        var (suggestedNodeId, suggestedNodeName, draftSpec) = ParseCascadeResponse(result, children);

        return Ok(new SuggestCascadeResponse(suggestedNodeId, suggestedNodeName, draftSpec));
    }

    /// <summary>Review a spec against its node's principles</summary>
    [HttpPost("review-spec")]
    public async Task<ActionResult<ReviewSpecResponse>> ReviewSpec(
        [FromBody] ReviewSpecRequest request, CancellationToken ct)
    {
        var spec = await _specRepo.GetByIdAsync(request.SpecId, ct);
        if (spec is null) return NotFound("Spec not found");

        var node = await _nodeRepo.GetByIdAsync(spec.NodeId, ct);
        if (node is null) return NotFound("Node not found");

        var effectivePrinciples = await GetEffectivePrinciplesAsync(spec.NodeId, ct);
        var principlesText = FormatPrinciples(effectivePrinciples);

        var latestContent = spec.Versions?.MaxBy(v => v.Version)?.Content ?? "(no content)";

        var prompt = $"""
            You are a spec reviewer for an organizational hierarchy.
            Evaluate the following spec against the node's effective principles.

            Spec title: {spec.Title}
            Spec content:
            {latestContent}

            Rate alignment on a scale of 1-100 and provide feedback.

            Respond in this exact format:
            ALIGNED: <true or false>
            SCORE: <1-100>
            FEEDBACK: <overall assessment>
            SUGGESTIONS:
            - <suggestion 1>
            - <suggestion 2>
            """;

        var context = $"""
            Node: {node.Name} (Type: {node.Type}, Depth: {node.Depth})
            Effective principles:
            {principlesText}
            """;

        var provider = request.Provider ?? _defaultProvider;
        var result = await _agentService.GenerateAsync(provider, prompt, context, ct);

        var review = ParseReviewResponse(result);

        return Ok(review);
    }

    // --- Private helpers ---

    private async Task<List<PrincipleInheritanceService.EffectivePrinciple>> GetEffectivePrinciplesAsync(
        Guid nodeId, CancellationToken ct)
    {
        var ancestors = await _nodeRepo.GetAncestorsAsync(nodeId, ct);
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);

        var allNodes = new List<Node>(ancestors);
        if (node is not null && !allNodes.Any(n => n.Id == nodeId))
            allNodes.Add(node);

        var ancestorPrinciples = new List<(Guid NodeId, List<Principle> Principles)>();
        foreach (var n in allNodes)
        {
            var principles = await _principleRepo.GetByNodeIdAsync(n.Id, ct);
            ancestorPrinciples.Add((n.Id, principles));
        }

        return _principleService.ResolveEffective(ancestorPrinciples, nodeId);
    }

    private static string FormatPrinciples(List<PrincipleInheritanceService.EffectivePrinciple> principles)
    {
        if (principles.Count == 0)
            return "(no principles defined)";

        return string.Join("\n", principles
            .Where(p => !p.IsOverridden)
            .Select(p =>
            {
                var source = p.IsInherited ? " (inherited)" : " (local)";
                return $"- {p.Principle.Title}{source}: {p.Principle.Content}";
            }));
    }

    private static (string PlanText, List<SuggestedTaskItem> Tasks) ParsePlanResponse(string response)
    {
        var tasks = new List<SuggestedTaskItem>();
        var planText = response;

        var jsonStart = response.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
        var jsonEnd = jsonStart >= 0 ? response.IndexOf("```", jsonStart + 7, StringComparison.Ordinal) : -1;

        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            var jsonContent = response[(jsonStart + 7)..jsonEnd].Trim();
            planText = response[..jsonStart].Trim();

            try
            {
                var parsed = JsonSerializer.Deserialize<List<JsonTaskItem>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
                if (parsed is not null)
                {
                    tasks = parsed.Select(t => new SuggestedTaskItem(t.Title ?? "", t.Description ?? "")).ToList();
                }
            }
            catch (JsonException)
            {
                // If parsing fails, return the full text as the plan with no structured tasks
                planText = response;
            }
        }

        return (planText, tasks);
    }

    private record JsonTaskItem(string? Title, string? Description);

    private static (Guid? NodeId, string? NodeName, string DraftSpec) ParseCascadeResponse(
        string response, List<Node> children)
    {
        Guid? suggestedNodeId = null;
        string? suggestedNodeName = null;
        var draftSpec = response;

        var lines = response.Split('\n');
        var draftStartIndex = -1;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            if (line.StartsWith("TARGET_NODE_ID:", StringComparison.OrdinalIgnoreCase))
            {
                var idStr = line["TARGET_NODE_ID:".Length..].Trim();
                if (Guid.TryParse(idStr, out var parsedId) && children.Any(c => c.Id == parsedId))
                {
                    suggestedNodeId = parsedId;
                }
            }
            else if (line.StartsWith("TARGET_NODE_NAME:", StringComparison.OrdinalIgnoreCase))
            {
                var name = line["TARGET_NODE_NAME:".Length..].Trim();
                if (!string.Equals(name, "none", StringComparison.OrdinalIgnoreCase))
                {
                    suggestedNodeName = name;
                }
            }
            else if (line.StartsWith("DRAFT_SPEC:", StringComparison.OrdinalIgnoreCase))
            {
                draftStartIndex = i + 1;
                break;
            }
        }

        if (draftStartIndex >= 0 && draftStartIndex < lines.Length)
        {
            draftSpec = string.Join("\n", lines[draftStartIndex..]).Trim();
        }

        return (suggestedNodeId, suggestedNodeName, draftSpec);
    }

    private static ReviewSpecResponse ParseReviewResponse(string response)
    {
        var aligned = false;
        var score = 50;
        var feedback = response;
        var suggestions = new List<string>();

        var lines = response.Split('\n');
        var inSuggestions = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (line.StartsWith("ALIGNED:", StringComparison.OrdinalIgnoreCase))
            {
                var val = line["ALIGNED:".Length..].Trim();
                aligned = string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
            }
            else if (line.StartsWith("SCORE:", StringComparison.OrdinalIgnoreCase))
            {
                var val = line["SCORE:".Length..].Trim();
                if (int.TryParse(val, out var parsedScore))
                    score = Math.Clamp(parsedScore, 1, 100);
            }
            else if (line.StartsWith("FEEDBACK:", StringComparison.OrdinalIgnoreCase))
            {
                feedback = line["FEEDBACK:".Length..].Trim();
                inSuggestions = false;
            }
            else if (line.StartsWith("SUGGESTIONS:", StringComparison.OrdinalIgnoreCase))
            {
                inSuggestions = true;
            }
            else if (inSuggestions && line.StartsWith("- "))
            {
                suggestions.Add(line[2..].Trim());
            }
        }

        return new ReviewSpecResponse(aligned, score, feedback, suggestions);
    }
}
