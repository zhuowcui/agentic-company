namespace AgenticCompany.Core.Entities;

public class SpecVersion
{
    public Guid Id { get; set; }
    public Guid SpecId { get; set; }
    public int Version { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Spec Spec { get; set; } = null!;
}
