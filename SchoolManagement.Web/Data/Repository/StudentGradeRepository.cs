using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class StudentGradeRepository : GenericRepository<StudentGrade>, IStudentGradeRepository
{
    public StudentGradeRepository(DataContext context) : base(context) { }

    public async Task<List<StudentGrade>> GetGradesByStudentIdsAsync(List<string> studentIds)
    {
        return await _dbSet
            .Where(g => studentIds.Contains(g.StudentId) && g.Grade.HasValue && g.GradeTypeId != null)
            .Include(g => g.GradeType)
            .ToListAsync();
    }
}

