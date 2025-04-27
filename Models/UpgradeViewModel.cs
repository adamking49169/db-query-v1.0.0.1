using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace db_query_v1._0._0._1.Models
{
    public class UpgradeViewModel
    {
        [Display(Name = "Current Plan")]
        public string CurrentPlan { get; set; }

        [Required(ErrorMessage = "Please select a plan to upgrade to.")]
        [Display(Name = "New Plan")]
        public string SelectedPlan { get; set; }

        public IEnumerable<string> AvailablePlans { get; set; }
    }
}
