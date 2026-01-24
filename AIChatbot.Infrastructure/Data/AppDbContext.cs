using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Data;
using AIChatbot.Domain.Entities;


public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatSession> ChatSessions { get; set; } = default!;
    public DbSet<Message> Messages { get; set; } = default!;
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();




    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ===============================
        // ChatSession configuration
        // ===============================
        builder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(450); // Identity UserId length

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(x => x.UserId);

            entity.HasMany(x => x.Messages)
                .WithOne(x => x.ChatSession)
                .HasForeignKey(x => x.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===============================
        // Message configuration
        // ===============================
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
    }
}
