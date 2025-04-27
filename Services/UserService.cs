namespace db_query_v1._0._0._1.Services
{
    public class UserService : IUserService
    {
        public string GetCurrentPlan(string userId)
        {
            // lookup in your user store…
            return "Free";
        }

        public void UpgradePlan(string userId, string newPlan)
        {
            // update their plan in your database…
        }
    }
}
