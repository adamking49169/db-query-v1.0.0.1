using System.Collections.Generic;

namespace db_query_v1._0._0._1.Services
{
    public class PlanService : IPlanService
    {
        public IEnumerable<string> GetAllPlanNames()
        {
            // pull from database, config, etc.
            return new[] { "Free", "Pro", "Enterprise" };
        }
    }
}
