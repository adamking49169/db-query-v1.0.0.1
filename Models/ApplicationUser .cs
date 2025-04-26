using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace db_query_v1._0._0._1.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

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
        public string Specializations { get; set; } // Store like "Law,Tech"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
