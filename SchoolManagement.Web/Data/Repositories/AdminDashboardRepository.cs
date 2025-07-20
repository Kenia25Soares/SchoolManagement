using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Data.Repositories
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly DataContext _context;

        public AdminDashboardRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<int> GetCoursesCountAsync()
        {
            return await _context.Courses.CountAsync();
        }

        public async Task<int> GetSubjectsCountAsync()
        {
            return await _context.Subjects.CountAsync();
        }
    }
}
