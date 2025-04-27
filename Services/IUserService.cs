namespace db_query_v1._0._0._1.Services
{
    public interface IUserService
    {
        string GetCurrentPlan(string userId);
        void UpgradePlan(string userId, string newPlan);
    }
}
