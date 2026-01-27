using Microsoft.EntityFrameworkCore;
using Agent.Domain.Entities.UserManagement;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Agent.Infrastructure.DependencyInjection;
//this is a comment
var a = 1;
namespace Agent.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : base(options) 
        { 
        }
        //public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<UserMessage> UserMessages { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            // Your refresh token / user session relations go here later if needed
        }

    }
}
