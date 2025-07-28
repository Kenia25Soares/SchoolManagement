using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Data.Repositories
{
    public class GradesRepository : GenericRepository<StudentGrade>, IGradesRepository
    {
        private readonly DataContext _context;

        public GradesRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<SelectListItem>> GetClassSelectListAsync(int? selectedId)
        {
            return await _context.StudentClasses
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = selectedId.HasValue && selectedId.Value == c.Id
                }).ToListAsync();
        }

        public async Task<List<StudentProfile>> GetStudentsByClassAsync(int classId)
        {
            return await _context.StudentProfiles
                .Include(p => p.User)
                .Where(p => p.StudentClassId == classId)
                .ToListAsync();
        }

        public async Task<List<StudentGrade>> GetAbsencesByStudentIdsAsync(IEnumerable<string> studentIds)
        {
            return await _context.StudentGrades
                .Where(g => studentIds.Contains(g.StudentId) && g.Absences > 0)
                .Include(g => g.Subject)
                .ToListAsync();
        }

        public async Task<List<StudentGrade>> GetGradesByStudentIdsAsync(IEnumerable<string> studentIds)
        {
            return await _context.StudentGrades
                .Where(g => studentIds.Contains(g.StudentId) && g.GradeTypeId != null)
                .Include(g => g.GradeType)
                .ToListAsync();
        }

        public async Task<bool> IsClassClosedAsync(int classId)
        {
            return await _context.StudentClasses
                .Where(c => c.Id == classId)
                .Select(c => c.IsClosed)
                .FirstOrDefaultAsync();
        }

        public async Task<StudentProfile?> GetStudentWithClassAsync(string studentId)
        {
            return await _context.StudentProfiles
                .Include(p => p.StudentClass)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == studentId);
        }

        public async Task<List<Subject>> GetSubjectsByCourseAsync(int courseId)
        {
            return await _context.CourseSubjects
                .Where(cs => cs.CourseId == courseId)
                .Select(cs => cs.Subject)
                .ToListAsync();
        }

        public async Task<List<GradeType>> GetGradeTypesAsync()
        {
            return await _context.GradeTypes.ToListAsync();
        }

        public async Task<List<StudentGrade>> GetGradesWithSubjectsAndTypesAsync(string studentId)
        {
            return await _context.StudentGrades
                .Where(g => g.StudentId == studentId)
                .Include(g => g.Subject)
                .Include(g => g.GradeType)
                .ToListAsync();
        }

        public async Task<List<StudentGrade>> GetAbsencesByStudentAsync(string studentId)
        {
            return await _context.StudentGrades
                .Where(g => g.StudentId == studentId && g.Absences > 0)
                .Include(g => g.Subject)
                .ToListAsync();
        }

        public async Task<StudentClass?> GetClassByIdAsync(int classId)
        {
            return await _context.StudentClasses
                .FirstOrDefaultAsync(c => c.Id == classId);
        }

        public async Task AddGradesAsync(IEnumerable<StudentGrade> grades)
        {
            await CreateRangeAsync(grades); 
        }

        public async Task AddAbsencesAsync(IEnumerable<StudentGrade> absences)
        {
            await CreateRangeAsync(absences); 
        }

        public async Task<StudentProfile?> GetStudentProfileByUserIdAsync(string userId)
        {
            return await _context.StudentProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task UpdateStudentProfileAsync(StudentProfile studentProfile)
        {
            _context.StudentProfiles.Update(studentProfile);
            await _context.SaveChangesAsync();
        }


    }
}
