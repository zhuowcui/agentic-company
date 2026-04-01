using System.Security.Claims;
using AgenticCompany.Api.Mapping;
using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Enums;
using AgenticCompany.Core.Interfaces;
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

    public SpecsController(ISpecRepository specRepo, INodeRepository nodeRepo, INodeMemberRepository memberRepo)
    {
        _specRepo = specRepo;
        _nodeRepo = nodeRepo;
        _memberRepo = memberRepo;
    }

    private async Task<bool> IsNodeMemberAsync(Guid nodeId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var membership = await _memberRepo.GetAsync(nodeId, userId, ct);
        return membership != null;
    }

    [HttpGet("api/nodes/{nodeId:guid}/specs")]
    public async Task<ActionResult<List<SpecResponse>>> GetByNode(Guid nodeId, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return NotFound("Node not found");

        var specs = await _specRepo.GetByNodeIdAsync(nodeId, ct);
        return Ok(specs.Select(s => s.ToResponse()).ToList());
    }

    [HttpGet("api/specs/{id:guid}")]
    public async Task<ActionResult<SpecResponse>> GetById(Guid id, CancellationToken ct)
    {
        var spec = await _specRepo.GetByIdAsync(id, ct);
        if (spec is null) return NotFound();
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

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToResponse(includeVersions: true));
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

        spec.Status = SpecStatus.Approved;
        await _specRepo.UpdateAsync(spec, ct);
        return Ok(spec.ToResponse());
    }
}
