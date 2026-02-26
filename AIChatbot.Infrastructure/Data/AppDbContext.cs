using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<ResponseMetadata> ResponseMetadata => Set<ResponseMetadata>();

    public DbSet<ChatSessionNaming> ChatSessionNaming => Set<ChatSessionNaming>();
    public DbSet<ConnectionString> ConnectionStrings => Set<ConnectionString>();
    public DbSet<PromptSet> PromptSets => Set<PromptSet>();
    public DbSet<PromptFunction> PromptFunctions => Set<PromptFunction>();

    public DbSet<RoleDbPermission> RoleDbPermissions => Set<RoleDbPermission>();
    public DbSet<RoleTablePermission> RoleTablePermissions => Set<RoleTablePermission>();
    public DbSet<RoleColumnPermission> RoleColumnPermissions => Set<RoleColumnPermission>();




    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).IsRequired().HasMaxLength(450);
            entity.Property(x => x.CreatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.UserId);

            entity.HasMany(x => x.Messages)
                  .WithOne(x => x.ChatSession)
                  .HasForeignKey(x => x.ChatSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Message>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Role).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Content).IsRequired();
            entity.Property(x => x.CreatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.ChatSessionId);
        });
        // -------------------- ConnectionStrings --------------------
        builder.Entity<ConnectionString>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ServerName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.DatabaseName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.AuthMode).IsRequired().HasMaxLength(20);
            entity.Property(x => x.DbIdentifier).IsRequired().HasMaxLength(100);

            entity.HasMany(x => x.PromptSets)
                  .WithOne(x => x.ConnectionString)
                  .HasForeignKey(x => x.ConnectionStringId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------- PromptSets --------------------
        builder.Entity<PromptSet>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.VersionType)
                  .IsRequired()
                  .HasMaxLength(20);

            entity.HasMany(x => x.Functions)
                  .WithOne(x => x.PromptSet)
                  .HasForeignKey(x => x.PromptSetId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------- PromptFunctions --------------------
        builder.Entity<PromptFunction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FunctionName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(x => x.SystemPrompt)
                  .IsRequired();
        });

        // -------------------- RoleDbPermissions --------------------
        builder.Entity<RoleDbPermission>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RoleName)
                  .IsRequired()
                  .HasMaxLength(100);
        });

        // -------------------- RoleTablePermissions --------------------
        builder.Entity<RoleTablePermission>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RoleName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(x => x.TableName)
                  .IsRequired()
                  .HasMaxLength(200);
        });

        // -------------------- RoleColumnPermissions --------------------
        builder.Entity<RoleColumnPermission>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RoleName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(x => x.TableName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(x => x.ColumnName)
                  .IsRequired()
                  .HasMaxLength(200);
        });
    }
}
