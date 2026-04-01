using System.Security.Claims;
using AgenticCompany.Api.Mapping;
using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Enums;
using AgenticCompany.Core.Interfaces;
using AgenticCompany.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgenticCompany.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/nodes/{nodeId:guid}/principles")]
public class PrinciplesController : ControllerBase
{
    private readonly IPrincipleRepository _principleRepo;
    private readonly INodeRepository _nodeRepo;
    private readonly INodeMemberRepository _memberRepo;
    private readonly PrincipleInheritanceService _inheritanceService;

    public PrinciplesController(
        IPrincipleRepository principleRepo,
        INodeRepository nodeRepo,
        INodeMemberRepository memberRepo,
        PrincipleInheritanceService inheritanceService)
    {
        _principleRepo = principleRepo;
        _nodeRepo = nodeRepo;
        _memberRepo = memberRepo;
        _inheritanceService = inheritanceService;
    }

    private async Task<bool> IsNodeMemberAsync(Guid nodeId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var membership = await _memberRepo.GetAsync(nodeId, userId, ct);
        return membership != null && membership.Role != NodeRole.Viewer;
    }

    private async Task<bool> HasReadAccessAsync(Guid nodeId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return false;
        var ancestorIds = node.Path.Split('.').Select(Guid.Parse).ToHashSet();
        var memberships = await _memberRepo.GetByUserIdAsync(userId, ct);
        return memberships.Any(m => ancestorIds.Contains(m.NodeId));
    }

    /// <summary>Get local principles for a node</summary>
    [HttpGet]
    public async Task<ActionResult<List<PrincipleResponse>>> GetByNode(Guid nodeId, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return NotFound("Node not found");

        if (!await HasReadAccessAsync(nodeId, ct))
            return Forbid();

        var principles = await _principleRepo.GetByNodeIdAsync(nodeId, ct);
        return Ok(principles.Select(p => p.ToResponse()).ToList());
    }

    /// <summary>Get effective (inherited + local, resolved) principles for a node</summary>
    [HttpGet("effective")]
    public async Task<ActionResult<List<EffectivePrincipleResponse>>> GetEffective(Guid nodeId, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return NotFound("Node not found");

        if (!await HasReadAccessAsync(nodeId, ct))
            return Forbid();

        // Build the ancestor chain (root first) including the target node
        var ancestors = await _nodeRepo.GetAncestorsAsync(nodeId, ct);
        ancestors.Add(node);

        // Collect principles per ancestor node
        var ancestorPrinciples = new List<(Guid NodeId, List<Principle> Principles)>();
        foreach (var ancestor in ancestors)
        {
            var principles = await _principleRepo.GetByNodeIdAsync(ancestor.Id, ct);
            ancestorPrinciples.Add((ancestor.Id, principles));
        }

        var effective = _inheritanceService.ResolveEffective(ancestorPrinciples, nodeId);
        return Ok(effective.Select(e => e.ToResponse()).ToList());
    }

    /// <summary>Create a principle on a node</summary>
    [HttpPost]
    public async Task<ActionResult<PrincipleResponse>> Create(Guid nodeId, [FromBody] CreatePrincipleRequest request, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return NotFound("Node not found");

        if (!await IsNodeMemberAsync(nodeId, ct))
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required");
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Content is required");

        var principle = new Principle
        {
            NodeId = nodeId,
            Title = request.Title,
            Content = request.Content,
            Order = request.Order,
            IsOverride = request.IsOverride,
        };

        var created = await _principleRepo.CreateAsync(principle, ct);
        return CreatedAtAction(nameof(GetByNode), new { nodeId }, created.ToResponse());
    }

    /// <summary>Update a principle</summary>
    [HttpPut("{principleId:guid}")]
    public async Task<ActionResult<PrincipleResponse>> Update(Guid nodeId, Guid principleId, [FromBody] UpdatePrincipleRequest request, CancellationToken ct)
    {
        if (!await IsNodeMemberAsync(nodeId, ct))
            return Forbid();

        var principles = await _principleRepo.GetByNodeIdAsync(nodeId, ct);
        var principle = principles.FirstOrDefault(p => p.Id == principleId);
        if (principle is null) return NotFound("Principle not found");

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required");
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Content is required");

        principle.Title = request.Title;
        principle.Content = request.Content;
        principle.Order = request.Order;
        principle.IsOverride = request.IsOverride;
        await _principleRepo.UpdateAsync(principle, ct);

        return Ok(principle.ToResponse());
    }

    /// <summary>Delete a principle</summary>
    [HttpDelete("{principleId:guid}")]
    public async Task<IActionResult> Delete(Guid nodeId, Guid principleId, CancellationToken ct)
    {
        if (!await IsNodeMemberAsync(nodeId, ct))
            return Forbid();

        var principles = await _principleRepo.GetByNodeIdAsync(nodeId, ct);
        var principle = principles.FirstOrDefault(p => p.Id == principleId);
        if (principle is null) return NotFound("Principle not found");

        await _principleRepo.DeleteAsync(principleId, ct);
        return NoContent();
    }
}
