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
public class SpecsController : ControllerBase
{
    private readonly ISpecRepository _specRepo;
    private readonly INodeRepository _nodeRepo;
    private readonly INodeMemberRepository _memberRepo;
    private readonly AppDbContext _db;

    public SpecsController(ISpecRepository specRepo, INodeRepository nodeRepo, INodeMemberRepository memberRepo, AppDbContext db)
    {
        _specRepo = specRepo;
        _nodeRepo = nodeRepo;
        _memberRepo = memberRepo;
        _db = db;
    }

    private async Task<bool> IsNodeMemberAsync(Guid nodeId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return false;
        var pathSegments = node.Path.Split('.');
        var memberships = await _memberRepo.GetByUserIdAsync(userId, ct);
        var byNodeId = memberships.ToDictionary(m => m.NodeId);
        // Most specific (deepest) membership wins
        for (int i = pathSegments.Length - 1; i >= 0; i--)
        {
            if (Guid.TryParse(pathSegments[i], out var segId) && byNodeId.TryGetValue(segId, out var m))
                return m.Role != NodeRole.Viewer;
        }
        return false;
    }

    private async Task<bool> HasReadAccessAsync(Guid nodeId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return false;
        var ancestorIds = node.Path.Split('.').Select(Guid.Parse).ToHashSet();
        var memberships = await _memberRepo.GetByUserIdAsync(userId, ct);
        if (memberships.Any(m => ancestorIds.Contains(m.NodeId)))
            return true;
        var nodePathPrefix = node.Path + ".";
        return memberships.Any(m => m.Node.Path.StartsWith(nodePathPrefix));
    }

    [HttpGet("api/nodes/{nodeId:guid}/specs")]
    public async Task<ActionResult<List<SpecResponse>>> GetByNode(Guid nodeId, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return NotFound("Node not found");

        if (!await HasReadAccessAsync(nodeId, ct))
            return Forbid();

        var specs = await _specRepo.GetByNodeIdAsync(nodeId, ct);
        return Ok(specs.Select(s => s.ToResponse()).ToList());
    }

    [HttpGet("api/specs/{id:guid}")]
    public async Task<ActionResult<SpecResponse>> GetById(Guid id, CancellationToken ct)
    {
        var spec = await _specRepo.GetByIdAsync(id, ct);
        if (spec is null) return NotFound();

        if (!await HasReadAccessAsync(spec.NodeId, ct))
            return Forbid();

        return Ok(spec.ToResponse(includeVersions: true));
    }

    [HttpPost("api/nodes/{nodeId:guid}/specs")]
    public async Task<ActionResult<SpecResponse>> Create(Guid nodeId, [FromBody] CreateSpecRequest request, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return NotFound("Node not found");

        if (!await IsNodeMemberAsync(nodeId, ct))
            return Forbid();

        var spec = new Spec
        {
            NodeId = nodeId,
            Title = request.Title,
            Status = SpecStatus.Draft,
        };

        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var created = await _specRepo.CreateAsync(spec, ct);

            // Create initial version
            created.Versions.Add(new SpecVersion
            {
                Id = Guid.NewGuid(),
                SpecId = created.Id,
                Version = 1,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
            });
            await _specRepo.UpdateAsync(created, ct);

            await transaction.CommitAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToResponse(includeVersions: true));
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    [HttpPut("api/specs/{id:guid}")]
    public async Task<ActionResult<SpecResponse>> Update(Guid id, [FromBody] UpdateSpecRequest request, CancellationToken ct)
    {
        var spec = await _specRepo.GetByIdAsync(id, ct);
        if (spec is null) return NotFound();

        if (!await IsNodeMemberAsync(spec.NodeId, ct))
            return Forbid();

        if (request.Title is not null) spec.Title = request.Title;

        var latestVersion = spec.Versions?.MaxBy(v => v.Version)?.Version ?? 0;
        spec.Versions ??= new List<SpecVersion>();
        spec.Versions.Add(new SpecVersion
        {
            Id = Guid.NewGuid(),
            SpecId = id,
            Version = latestVersion + 1,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
        });

        try
        {
            await _specRepo.UpdateAsync(spec, ct);
            return Ok(spec.ToResponse(includeVersions: true));
        }
        catch (DbUpdateException)
        {
            return Conflict(new { error = "Spec was modified concurrently. Please reload and try again." });
        }
    }

    [HttpPost("api/specs/{id:guid}/approve")]
    public async Task<ActionResult<SpecResponse>> Approve(Guid id, CancellationToken ct)
    {
        var spec = await _specRepo.GetByIdAsync(id, ct);
        if (spec is null) return NotFound();

        if (!await IsNodeMemberAsync(spec.NodeId, ct))
            return Forbid();

        spec.Status = SpecStatus.Approved;
        await _specRepo.UpdateAsync(spec, ct);
        return Ok(spec.ToResponse());
    }
}
