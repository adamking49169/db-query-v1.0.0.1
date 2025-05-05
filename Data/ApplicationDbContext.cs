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
        { }

        public DbSet<ChatHistoryItem> ChatHistoryItems { get; set; }
        public DbSet<PreviousChat> PreviousChats { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<ChatResponseLog> ChatResponseLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1) User ↔ Profile (1:1)
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.IdentityUser)
                .HasForeignKey<UserProfile>(p => p.UserIdentityId);

            // 2) User ↔ ChatHistoryItem (1:N)
            builder.Entity<ChatHistoryItem>()
                .HasOne(c => c.User)
                .WithMany(u => u.ChatHistory)
                .HasForeignKey(c => c.UserIdentityId);

            // 3) User ↔ PreviousChat (1:N)
            builder.Entity<PreviousChat>()
                .HasOne(p => p.User)
                .WithMany(u => u.PreviousChats)
                .HasForeignKey(p => p.UserIdentityId);

            // 4) PK generation
            builder.Entity<PreviousChat>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            // 5) PreviousChat ↔ ChatHistoryItem (1:N) — break cascade here
            builder.Entity<ChatHistoryItem>()
                .HasOne(c => c.PreviousChat)
                .WithMany(p => p.ChatHistoryItems)
                .HasForeignKey(c => c.ChatId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6) Default SQL timestamp (optional)
            builder.Entity<ChatHistoryItem>()
                .Property(c => c.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
