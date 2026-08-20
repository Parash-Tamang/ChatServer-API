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
    public DbSet<LocalFunctionBlock> LocalFunctionBlocks => Set<LocalFunctionBlock>();
    public DbSet<PromptFunction> PromptFunctions => Set<PromptFunction>();
    public DbSet<ConnectionExclusion> ConnectionExclusions => Set<ConnectionExclusion>();
    public DbSet<RoleInstruction> RoleInstructions => Set<RoleInstruction>();
    public DbSet<RoleConnectionMapping> RoleConnectionMappings => Set<RoleConnectionMapping>();


    public DbSet<ExternalUserMapping> ExternalUserMappings => Set<ExternalUserMapping>();

    public DbSet<RoleRuntimePermission>
    RoleRuntimePermissions
    { get; set; }
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<RoleConnectionUserLookupConfiguration>
    RoleConnectionUserLookupConfigurations
    { get; set; }
    public DbSet<RuntimePermissionView>
    RuntimePermissionViews
    { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // -------------------- ChatSession --------------------
        builder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                  .IsRequired()
                  .HasMaxLength(450);

            entity.Property(x => x.CreatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(x => x.UserId);

            entity.HasMany(x => x.Messages)
                  .WithOne(x => x.ChatSession)
                  .HasForeignKey(x => x.ChatSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------- Message --------------------
        builder.Entity<Message>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Role)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(x => x.Content)
                  .IsRequired();

            entity.Property(x => x.CreatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(x => x.ChatSessionId);
        });

        // -------------------- ConnectionStrings --------------------
        builder.Entity<ConnectionString>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ServerName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(x => x.DatabaseName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(x => x.AuthMode)
                  .IsRequired()
                  .HasMaxLength(20);

            entity.Property(x => x.UpdatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasMany(x => x.Functions)
                  .WithOne(x => x.ConnectionString)
                  .HasForeignKey(x => x.ConnectionStringId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------- LocalFunctionBlock --------------------
        builder.Entity<LocalFunctionBlock>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FunctionName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.HasIndex(x => new { x.ConnectionId, x.FunctionName })
                  .IsUnique();
        });

        // -------------------- PromptFunction --------------------
        builder.Entity<PromptFunction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FunctionName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(x => x.SystemPrompt)
                  .IsRequired();

            entity.HasOne(x => x.ConnectionString)
                  .WithMany(x => x.Functions)
                  .HasForeignKey(x => x.ConnectionStringId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired(false); // allows global functions
        });
        // -------------------- ConnectionExclusion --------------------
        builder.Entity<ConnectionExclusion>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ExclusionPath)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(x => x.CreatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne<ConnectionString>()
                  .WithMany()
                  .HasForeignKey(x => x.ConnectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        // -------------------- RoleConnectionMapping --------------------
        builder.Entity<RoleConnectionMapping>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CreatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(x => x.Role)
                  .WithMany()
                  .HasForeignKey(x => x.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Connection)
                  .WithMany()
                  .HasForeignKey(x => x.ConnectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        // -------------------- ExternalUserMapping --------------------
        builder.Entity<ExternalUserMapping>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ExternalTable)
                  .HasMaxLength(200);

            entity.Property(x => x.ExternalUserId)
                  .HasMaxLength(200);

            entity.Property(x => x.MatchColumn)
                  .HasMaxLength(100);

            entity.Property(x => x.MatchValue)
                  .HasMaxLength(300);

            entity.Property(x => x.CreatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(x => x.ApplicationUser)
                  .WithMany()
                  .HasForeignKey(x => x.ApplicationUserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Connection)
                  .WithMany()
                  .HasForeignKey(x => x.ConnectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        // -------------------- RoleInstruction --------------------
        builder.Entity<RoleInstruction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RoleId)
                  .IsRequired()
                  .HasMaxLength(450);

            entity.Property(x => x.InstructionSet)
                  .IsRequired();

            entity.Property(x => x.CreatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()");
        });
    }
}