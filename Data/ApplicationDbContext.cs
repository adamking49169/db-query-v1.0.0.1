using db_query_v1._0._0._1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

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

        // Update the TitanicCsvReader class to fix the issue
        public class TitanicCsvReader
        {
            public static List<Passenger> ReadTitanicCsv(string filePath)
            {
                try
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        // Optional: Handle reading exceptions before creating the CsvReader
                      
                    };

                    using var reader = new StreamReader(filePath);
                    using var csv = new CsvReader(reader, config);
        
            // Register your custom ClassMap here
            csv.Context.RegisterClassMap<PassengerMap>();
        

        var records = csv.GetRecords<Passenger>().ToList();
                    return records;
                }
                catch (CsvHelper.ReaderException e)
                {
                    var b = e.ToString();
                    // Catch any reader exceptions
                    return new List<Passenger>();  // Return an empty list if there's an error
                }
            }


        }

        public DbSet<ChatResponseLog> ChatResponseLogs { get; set; }
    }
}