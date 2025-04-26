using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace db_query_v1._0._0._1.Models
{
    public class SettingsViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; init; }  // Read-only

        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        public string Address { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        public string[] Specializations { get; set; } = Array.Empty<string>();
    }
}
