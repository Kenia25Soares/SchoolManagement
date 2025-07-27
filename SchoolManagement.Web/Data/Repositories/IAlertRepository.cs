using SchoolManagement.Web.Data.Entities;

namespace SchoolManagement.Web.Data.Repositories
{
    public interface IAlertRepository : IGenericRepository<Alert>
    {
        Task<IEnumerable<Alert?>> GetAllWithCreatorAsync();
        Task<Alert?> GetByIdWithCreatorAsync(int id);
    }
}
