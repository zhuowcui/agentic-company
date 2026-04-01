using System.Security.Claims;
using AgenticCompany.Api.Mapping;
using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Enums;
using AgenticCompany.Core.Interfaces;
using AgenticCompany.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgenticCompany.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NodesController : ControllerBase
{
    private readonly INodeRepository _nodeRepo;
    private readonly INodeMemberRepository _memberRepo;
    private readonly IUserRepository _userRepo;
    private readonly AppDbContext _db;

    public NodesController(INodeRepository nodeRepo, INodeMemberRepository memberRepo, IUserRepository userRepo, AppDbContext db)
    {
        _nodeRepo = nodeRepo;
        _memberRepo = memberRepo;
        _userRepo = userRepo;
        _db = db;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Check read access with bidirectional inheritance: user can read a node if they
    /// have membership on that node, any ancestor, OR any descendant (for tree navigation).
    /// </summary>
    private async Task<bool> HasReadAccessAsync(Guid nodeId, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return false;

        return await HasReadAccessByPathAsync(node.Path, ct);
    }

    private async Task<bool> HasReadAccessByPathAsync(string nodePath, CancellationToken ct)
    {
        var userId = GetUserId();
        var memberships = await _memberRepo.GetByUserIdAsync(userId, ct);

        // Check 1: membership on self or any ancestor (downward inheritance)
        var ancestorIds = nodePath.Split('.').Select(Guid.Parse).ToHashSet();
        if (memberships.Any(m => ancestorIds.Contains(m.NodeId)))
            return true;

        // Check 2: membership on any descendant (upward navigation — allows viewing ancestor tree)
        var nodePathPrefix = nodePath + ".";
        return memberships.Any(m => m.Node.Path.StartsWith(nodePathPrefix));
    }

    /// <summary>List root nodes accessible to the caller</summary>
    [HttpGet]
    public async Task<ActionResult<List<NodeResponse>>> GetRoots(CancellationToken ct)
    {
        var userId = GetUserId();
        var memberships = await _memberRepo.GetByUserIdAsync(userId, ct);

        // Extract unique root IDs from membership node paths (first segment = root)
        var accessibleRootIds = new HashSet<Guid>();
        foreach (var membership in memberships)
        {
            var rootIdStr = membership.Node.Path.Split('.')[0];
            if (Guid.TryParse(rootIdStr, out var rootId))
                accessibleRootIds.Add(rootId);
        }

        // Only fetch the roots the user can access (not all roots in the system)
        var accessibleRoots = await _nodeRepo.GetByIdsAsync(
            accessibleRootIds.Where(id => true), ct);
        // Filter to actual roots (ParentId == null) in case a membership path is corrupt
        accessibleRoots = accessibleRoots.Where(n => n.ParentId == null).ToList();

        return Ok(accessibleRoots.Select(n => n.ToResponse()).ToList());
    }

    /// <summary>Get a single node by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NodeResponse>> GetById(Guid id, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(id, ct);
        if (node is null) return NotFound();

        if (!await HasReadAccessByPathAsync(node.Path, ct))
            return Forbid();

        return Ok(node.ToResponse());
    }

    /// <summary>Get node with full subtree</summary>
    [HttpGet("{id:guid}/tree")]
    public async Task<ActionResult<NodeResponse>> GetTree(Guid id, CancellationToken ct)
    {
        var node = await _nodeRepo.GetWithChildrenAsync(id, ct);
        if (node is null) return NotFound();

        if (!await HasReadAccessByPathAsync(node.Path, ct))
            return Forbid();

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

            var parentMembership = await _memberRepo.GetAsync(request.ParentId.Value, GetUserId(), ct);
            if (parentMembership is null || parentMembership.Role == NodeRole.Viewer)
                return Forbid();
        }

        var node = new Node
        {
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            ParentId = request.ParentId,
        };

        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var created = await _nodeRepo.CreateAsync(node, ct);

            // Auto-create the caller as Owner of the new node
            await _memberRepo.CreateAsync(new NodeMember
            {
                NodeId = created.Id,
                UserId = GetUserId(),
                Role = NodeRole.Owner,
            }, ct);

            await transaction.CommitAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToResponse());
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
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
        if (id == request.NewParentId)
            return BadRequest("Cannot move a node under itself.");

        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var node = await _nodeRepo.GetByIdAsync(id, ct);
            if (node is null) return NotFound();

            var membership = await _memberRepo.GetAsync(id, GetUserId(), ct);
            if (membership is null || (membership.Role != NodeRole.Owner && membership.Role != NodeRole.Admin))
                return Forbid();

            var newParent = await _nodeRepo.GetByIdAsync(request.NewParentId, ct);
            if (newParent is null) return BadRequest("New parent node not found");

            var destMembership = await _memberRepo.GetAsync(request.NewParentId, GetUserId(), ct);
            if (destMembership is null || destMembership.Role == NodeRole.Viewer)
                return Forbid();

            // Prevent moving a node under its own descendant
            var descendants = await _nodeRepo.GetDescendantsAsync(id, ct);
            if (descendants.Any(d => d.Id == request.NewParentId))
                return BadRequest("Cannot move a node under its own descendant");

            var oldPath = node.Path;
            var oldDepth = node.Depth;

            var descendantNodes = await _nodeRepo.GetDescendantsByPathPrefixAsync(oldPath, ct);

            node.ParentId = request.NewParentId;
            node.Path = $"{newParent.Path}.{node.Id}";
            node.Depth = newParent.Depth + 1;

            var depthDelta = node.Depth - oldDepth;

            foreach (var desc in descendantNodes)
            {
                desc.Path = node.Path + desc.Path[oldPath.Length..];
                desc.Depth += depthDelta;
            }

            await _nodeRepo.UpdateRangeAsync(descendantNodes.Prepend(node), ct);
            await transaction.CommitAsync(ct);

            return Ok(node.ToResponse());
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>List members of a node</summary>
    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<List<NodeMemberResponse>>> GetMembers(Guid id, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(id, ct);
        if (node is null) return NotFound();

        if (!await HasReadAccessByPathAsync(node.Path, ct))
            return Forbid();

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

        // Only Owners can grant Owner role
        if (role == NodeRole.Owner && callerMembership.Role != NodeRole.Owner)
            return BadRequest("Only Owners can grant the Owner role.");

        var user = await _userRepo.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return BadRequest("User not found.");

        var existing = await _memberRepo.GetAsync(id, request.UserId, ct);
        if (existing is not null)
            return Conflict("User is already a member of this node.");

        NodeMember member;
        try
        {
            member = await _memberRepo.CreateAsync(new NodeMember
            {
                NodeId = id,
                UserId = request.UserId,
                Role = role,
            }, ct);
        }
        catch (DbUpdateException)
        {
            return Conflict("User is already a member of this node.");
        }

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

        // Admins cannot remove Owners
        if (callerMembership.Role == NodeRole.Admin && target.Role == NodeRole.Owner)
            return BadRequest("Admins cannot remove Owners.");

        // Prevent removing the last Owner — use FOR UPDATE lock to prevent write skew
        if (target.Role == NodeRole.Owner)
        {
            using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // Lock all owner membership rows for this node to prevent concurrent removal
                var ownerCount = await _db.Database
                    .SqlQuery<int>($"""
                        SELECT COUNT(*)::int AS "Value" FROM "NodeMembers"
                        WHERE "NodeId" = {nodeId} AND "Role" = 'Owner'
                        FOR UPDATE
                        """)
                    .FirstAsync(ct);

                if (ownerCount <= 1)
                    return BadRequest("Cannot remove the last Owner of a node.");

                await _memberRepo.DeleteAsync(nodeId, userId, ct);
                await transaction.CommitAsync(ct);
                return NoContent();
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        await _memberRepo.DeleteAsync(nodeId, userId, ct);
        return NoContent();
    }
}
