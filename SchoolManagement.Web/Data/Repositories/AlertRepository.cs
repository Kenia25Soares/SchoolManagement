using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;

namespace SchoolManagement.Data.Repositories
{
    public class AlertRepository : GenericRepository<Alert>, IAlertRepository
    {
        private readonly DataContext _context;
        public AlertRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Alert>> GetAllWithCreatorAsync()
        {
            return await _context.Alerts
                .Include(a => a.CreatedBy)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Alert?> GetByIdWithCreatorAsync(int id)
        {
            return await _context.Alerts
                .Include(a => a.CreatedBy)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}
