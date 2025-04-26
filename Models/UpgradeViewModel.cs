using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace db_query_v1._0._0._1.Models
{
    public class UpgradeViewModel
    {
        [Display(Name = "Current Plan")]
        public string CurrentPlan { get; set; }

        [Display(Name = "Available Plans")]
        public string[] AvailablePlans { get; set; } = Array.Empty<string>();

        [Required]
        [Display(Name = "Select Plan")]
        public string SelectedPlan { get; set; }
    }


}
