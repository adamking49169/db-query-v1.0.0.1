using Microsoft.AspNetCore.Identity;
using Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace db_query_v1._0._0._1.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        public virtual UserProfile Profile { get; set; }
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [StringLength(20)]
        public string Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(250)]
        public string Address { get; set; }

        [Phone]
        public string Phone { get; set; }

        [StringLength(100)]
        public string Specializations { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public ICollection<ChatHistoryItem> ChatHistory { get; set; }

        // Navigation for previous chats
        public ICollection<PreviousChat> PreviousChats { get; set; }
    }
    public class UserProfile
    {
        public int Id { get; set; }

        // this will hold the AspNetUsers.Id
        [ForeignKey(nameof(IdentityUser))]
        public string UserIdentityId { get; set; }

        public virtual ApplicationUser IdentityUser { get; set; }

        // …whatever other fields you need
    }
}
