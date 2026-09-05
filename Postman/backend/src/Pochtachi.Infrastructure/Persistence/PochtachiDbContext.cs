using Microsoft.EntityFrameworkCore;
using Pochtachi.Domain.Entities;

namespace Pochtachi.Infrastructure.Persistence;

public class PochtachiDbContext(DbContextOptions<PochtachiDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<ApiEnvironment> Environments => Set<ApiEnvironment>();
    public DbSet<SwitchDimension> SwitchDimensions => Set<SwitchDimension>();
    public DbSet<SwitchOption> SwitchOptions => Set<SwitchOption>();
    public DbSet<Variable> Variables => Set<Variable>();
    public DbSet<VariableValue> VariableValues => Set<VariableValue>();
    public DbSet<AuthConfig> AuthConfigs => Set<AuthConfig>();
    public DbSet<RequestHistory> RequestHistories => Set<RequestHistory>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<WorkspaceMember>().HasKey(m => new { m.WorkspaceId, m.UserId });

        builder.Entity<SwitchDimension>()
            .HasOne(d => d.ActiveOption)
            .WithMany()
            .HasForeignKey(d => d.ActiveOptionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<SwitchOption>()
            .HasOne(o => o.SwitchDimension)
            .WithMany(d => d.Options)
            .HasForeignKey(o => o.SwitchDimensionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<VariableValue>()
            .HasOne(v => v.Variable)
            .WithMany(v => v.Values)
            .HasForeignKey(v => v.VariableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<VariableValue>()
            .HasOne(v => v.SwitchOption)
            .WithMany()
            .HasForeignKey(v => v.SwitchOptionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Folder>()
            .HasOne(f => f.ParentFolder)
            .WithMany(f => f.ChildFolders)
            .HasForeignKey(f => f.ParentFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Request>()
            .HasOne(r => r.Folder)
            .WithMany(f => f.Requests)
            .HasForeignKey(r => r.FolderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
