using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace AgenticCompany.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Node> Nodes => Set<Node>();
    public DbSet<Principle> Principles => Set<Principle>();
    public DbSet<Spec> Specs => Set<Spec>();
    public DbSet<SpecVersion> SpecVersions => Set<SpecVersion>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<TaskDependency> TaskDependencies => Set<TaskDependency>();
    public DbSet<NodeMember> NodeMembers => Set<NodeMember>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Node - self-referential hierarchy
        modelBuilder.Entity<Node>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(e => e.ParentId);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Path);

            entity.HasOne(e => e.Parent)
                  .WithMany(e => e.Children)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Principle
        modelBuilder.Entity<Principle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => e.NodeId);

            entity.HasOne(e => e.Node)
                  .WithMany(e => e.Principles)
                  .HasForeignKey(e => e.NodeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Spec
        modelBuilder.Entity<Spec>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(e => e.NodeId);

            entity.HasOne(e => e.Node)
                  .WithMany(e => e.Specs)
                  .HasForeignKey(e => e.NodeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SourceTask)
                  .WithOne(t => t.SpawnedSpec)
                  .HasForeignKey<Spec>(e => e.SourceTaskId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // SpecVersion
        modelBuilder.Entity<SpecVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => new { e.SpecId, e.Version }).IsUnique();

            entity.HasOne(e => e.Spec)
                  .WithMany(e => e.Versions)
                  .HasForeignKey(e => e.SpecId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Plan
        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.PlanType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);

            entity.HasOne(e => e.Spec)
                  .WithMany(e => e.Plans)
                  .HasForeignKey(e => e.SpecId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskItem
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(e => e.PlanId);
            entity.HasIndex(e => e.TargetNodeId);

            entity.HasOne(e => e.Plan)
                  .WithMany(e => e.Tasks)
                  .HasForeignKey(e => e.PlanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TargetNode)
                  .WithMany()
                  .HasForeignKey(e => e.TargetNodeId)
                  .OnDelete(DeleteBehavior.SetNull);

            // SpawnedSpec relationship configured on Spec side via SourceTaskId FK.
            // Ignore SpawnedSpecId so EF doesn't create a second FK relationship.
            entity.Ignore(e => e.SpawnedSpecId);
        });

        // TaskDependency
        modelBuilder.Entity<TaskDependency>(entity =>
        {
            entity.HasKey(e => new { e.TaskId, e.DependsOnTaskId });

            entity.HasOne(e => e.Task)
                  .WithMany(e => e.Dependencies)
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.DependsOnTask)
                  .WithMany(e => e.Dependents)
                  .HasForeignKey(e => e.DependsOnTaskId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // NodeMember
        modelBuilder.Entity<NodeMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(e => new { e.NodeId, e.UserId }).IsUnique();

            entity.HasOne(e => e.Node)
                  .WithMany(e => e.Members)
                  .HasForeignKey(e => e.NodeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
