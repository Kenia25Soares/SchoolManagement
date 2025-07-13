namespace SchoolManagement.Web.Data.Repositories
{
    public interface IAdminDashboardRepository
    {
        Task<int> GetCoursesCountAsync();
        Task<int> GetSubjectsCountAsync();
    }
}
