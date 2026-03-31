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

    public NodesController(INodeRepository nodeRepo)
    {
        _nodeRepo = nodeRepo;
    }

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
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToResponse());
    }

    /// <summary>Update a node's name and description</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NodeResponse>> Update(Guid id, [FromBody] UpdateNodeRequest request, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(id, ct);
        if (node is null) return NotFound();

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
}
