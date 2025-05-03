using db_query_v1._0._0._1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
namespace db_query_v1._0._0._1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ChatHistoryItem> ChatHistoryItems { get; set; }
        public DbSet<PreviousChat> PreviousChats { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
               .HasOne(u => u.Profile)
               .WithOne(p => p.IdentityUser)
               .HasForeignKey<UserProfile>(p => p.UserIdentityId);

            builder.Entity<ChatHistoryItem>()
                   .HasOne<ApplicationUser>()
                   .WithMany(u => u.ChatHistory)
                   .HasForeignKey(c => c.UserIdentityId);

            builder.Entity<PreviousChat>()
                   .HasOne<ApplicationUser>()
                   .WithMany(u => u.PreviousChats)
                   .HasForeignKey(c => c.UserIdentityId);
        }

        public DbSet<ChatResponseLog> ChatResponseLogs { get; set; }
    }
}