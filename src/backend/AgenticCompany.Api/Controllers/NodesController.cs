using System.Security.Claims;
using AgenticCompany.Api.Mapping;
using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Enums;
using AgenticCompany.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgenticCompany.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NodesController : ControllerBase
{
    private readonly INodeRepository _nodeRepo;
    private readonly INodeMemberRepository _memberRepo;

    public NodesController(INodeRepository nodeRepo, INodeMemberRepository memberRepo)
    {
        _nodeRepo = nodeRepo;
        _memberRepo = memberRepo;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>List root nodes</summary>
    [HttpGet]
    public async Task<ActionResult<List<NodeResponse>>> GetRoots(CancellationToken ct)
    {
        var nodes = await _nodeRepo.GetRootsAsync(ct);
        return Ok(nodes.Select(n => n.ToResponse()).ToList());
    }

    /// <summary>Get a single node by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NodeResponse>> GetById(Guid id, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(id, ct);
        if (node is null) return NotFound();
        return Ok(node.ToResponse());
    }

    /// <summary>Get node with full subtree</summary>
    [HttpGet("{id:guid}/tree")]
    public async Task<ActionResult<NodeResponse>> GetTree(Guid id, CancellationToken ct)
    {
        var node = await _nodeRepo.GetWithChildrenAsync(id, ct);
        if (node is null) return NotFound();
        return Ok(node.ToResponse(includeChildren: true));
    }

    /// <summary>Create a new node</summary>
    [HttpPost]
    public async Task<ActionResult<NodeResponse>> Create([FromBody] CreateNodeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required");

        if (request.ParentId.HasValue)
        {
            var parent = await _nodeRepo.GetByIdAsync(request.ParentId.Value, ct);
            if (parent is null)
                return BadRequest(new { error = $"Parent node {request.ParentId.Value} does not exist." });
        }

        var node = new Node
        {
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            ParentId = request.ParentId,
        };

        var created = await _nodeRepo.CreateAsync(node, ct);

        // Auto-create the caller as Owner of the new node
        await _memberRepo.CreateAsync(new NodeMember
        {
            NodeId = created.Id,
            UserId = GetUserId(),
            Role = NodeRole.Owner,
        }, ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToResponse());
    }

    /// <summary>Update a node's name and description</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NodeResponse>> Update(Guid id, [FromBody] UpdateNodeRequest request, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(id, ct);
        if (node is null) return NotFound();

        var membership = await _memberRepo.GetAsync(id, GetUserId(), ct);
        if (membership is null || (membership.Role != NodeRole.Owner && membership.Role != NodeRole.Admin))
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required");

        node.Name = request.Name;
        node.Description = request.Description;
        await _nodeRepo.UpdateAsync(node, ct);

        return Ok(node.ToResponse());
    }

    /// <summary>Delete a node</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var node = await _nodeRepo.GetWithChildrenAsync(id, ct);
        if (node is null) return NotFound();

        var membership = await _memberRepo.GetAsync(id, GetUserId(), ct);
        if (membership is null || membership.Role != NodeRole.Owner)
            return Forbid();

        if (node.Children.Any())
            return Conflict("Cannot delete a node that has children. Move or delete children first.");

        await _nodeRepo.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Move a node to a new parent</summary>
    [HttpPatch("{id:guid}/move")]
    public async Task<ActionResult<NodeResponse>> Move(Guid id, [FromBody] MoveNodeRequest request, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(id, ct);
        if (node is null) return NotFound();

        var membership = await _memberRepo.GetAsync(id, GetUserId(), ct);
        if (membership is null || (membership.Role != NodeRole.Owner && membership.Role != NodeRole.Admin))
            return Forbid();

        var newParent = await _nodeRepo.GetByIdAsync(request.NewParentId, ct);
        if (newParent is null) return BadRequest("New parent node not found");

        // Prevent moving a node under its own descendant
        var descendants = await _nodeRepo.GetDescendantsAsync(id, ct);
        if (descendants.Any(d => d.Id == request.NewParentId))
            return BadRequest("Cannot move a node under its own descendant");

        var oldPath = node.Path;
        var oldDepth = node.Depth;

        // Fetch descendants by old path prefix before changing the node
        var descendantNodes = await _nodeRepo.GetDescendantsByPathPrefixAsync(oldPath, ct);

        // Update the moved node
        node.ParentId = request.NewParentId;
        node.Path = $"{newParent.Path}.{node.Id}";
        node.Depth = newParent.Depth + 1;

        var depthDelta = node.Depth - oldDepth;

        // Update all descendants' paths and depths
        foreach (var desc in descendantNodes)
        {
            desc.Path = node.Path + desc.Path[oldPath.Length..];
            desc.Depth += depthDelta;
        }

        await _nodeRepo.UpdateRangeAsync(descendantNodes.Prepend(node), ct);

        return Ok(node.ToResponse());
    }

    /// <summary>List members of a node</summary>
    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<List<NodeMemberResponse>>> GetMembers(Guid id, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(id, ct);
        if (node is null) return NotFound();

        var members = await _memberRepo.GetByNodeIdAsync(id, ct);
        return Ok(members.Select(m => new NodeMemberResponse(
            m.UserId, m.User.Email, m.User.DisplayName, m.Role.ToString(), m.JoinedAt)).ToList());
    }

    /// <summary>Add a member to a node</summary>
    [HttpPost("{id:guid}/members")]
    public async Task<ActionResult<NodeMemberResponse>> AddMember(Guid id, [FromBody] AddNodeMemberRequest request, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(id, ct);
        if (node is null) return NotFound();

        var callerMembership = await _memberRepo.GetAsync(id, GetUserId(), ct);
        if (callerMembership is null || (callerMembership.Role != NodeRole.Owner && callerMembership.Role != NodeRole.Admin))
            return Forbid();

        if (!Enum.TryParse<NodeRole>(request.Role, true, out var role))
            return BadRequest($"Invalid role '{request.Role}'. Valid values: {string.Join(", ", Enum.GetNames<NodeRole>())}");

        var existing = await _memberRepo.GetAsync(id, request.UserId, ct);
        if (existing is not null)
            return Conflict("User is already a member of this node.");

        var member = await _memberRepo.CreateAsync(new NodeMember
        {
            NodeId = id,
            UserId = request.UserId,
            Role = role,
        }, ct);

        return Created($"/api/nodes/{id}/members", new NodeMemberResponse(
            member.UserId, member.User?.Email ?? "", member.User?.DisplayName ?? "", member.Role.ToString(), member.JoinedAt));
    }

    /// <summary>Remove a member from a node</summary>
    [HttpDelete("{nodeId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid nodeId, Guid userId, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return NotFound();

        var callerMembership = await _memberRepo.GetAsync(nodeId, GetUserId(), ct);
        if (callerMembership is null || (callerMembership.Role != NodeRole.Owner && callerMembership.Role != NodeRole.Admin))
            return Forbid();

        var target = await _memberRepo.GetAsync(nodeId, userId, ct);
        if (target is null) return NotFound("Member not found.");

        await _memberRepo.DeleteAsync(nodeId, userId, ct);
        return NoContent();
    }
}
