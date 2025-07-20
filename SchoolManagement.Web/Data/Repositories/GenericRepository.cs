using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;

namespace SchoolManagement.Web.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
    {
        private readonly DataContext _context;
        //protected readonly DbSet<T> _dbSet;

        public GenericRepository(DataContext context)
        {
            _context = context;
            //_dbSet = _context.Set<T>();
        }

        public IQueryable<T> GetAll()
        {
            return _context.Set<T>().AsNoTracking();  //Vai buscar todos os dados da tabela e depois desativa o tracking para melhorar a performance
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>()
                .AsNoTracking()  //Desativa o tracking para melhorar a performance
                .FirstOrDefaultAsync(e => e.Id == id);
                //.FindAsync(id);
        }

        public async Task CreateAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await SaveAllAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity); // Update não é acincrono, mas é usado para marcar a entidade como modificada
            await SaveAllAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await SaveAllAsync();
        }

        public async Task<bool> ExistAsync(int id)
        {
            return await _context.Set<T>().AnyAsync(e => e.Id == id);
            //return await _context.Set<T>().FindAsync(id) != null;
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task CreateRangeAsync(IEnumerable<T> entities)
        {
            //await _dbSet.AddRangeAsync(entities);
            await _context.Set<T>().AddRangeAsync(entities);
            await SaveAllAsync();
        }
    }
}
