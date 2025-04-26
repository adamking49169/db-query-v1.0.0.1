using db_query_v1._0._0._1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatHistoryItem> ChatHistoryItems { get; set; }
    public DbSet<PreviousChat> PreviousChats { get; set; }
    public DbSet<ChatResponseLog> ChatResponseLogs { get; set; }
}