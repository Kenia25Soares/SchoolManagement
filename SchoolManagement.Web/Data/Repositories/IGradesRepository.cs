using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Data.Repositories
{
    public interface IGradesRepository : IGenericRepository<StudentGrade>
    {
        Task<List<SelectListItem>> GetClassSelectListAsync(int? selectedId);
        Task<List<StudentProfile>> GetStudentsByClassAsync(int classId);
        Task<List<StudentGrade>> GetAbsencesByStudentIdsAsync(IEnumerable<string> studentIds);
        Task<List<StudentGrade>> GetGradesByStudentIdsAsync(IEnumerable<string> studentIds);
        Task<bool> IsClassClosedAsync(int classId);
        Task<StudentProfile?> GetStudentWithClassAsync(string studentId); 
        Task<List<Subject>> GetSubjectsByCourseAsync(int courseId);
        Task<List<GradeType>> GetGradeTypesAsync();
        Task<List<StudentGrade>> GetGradesWithSubjectsAndTypesAsync(string studentId); // 
        Task<List<StudentGrade>> GetAbsencesByStudentAsync(string studentId);
        Task<StudentClass?> GetClassByIdAsync(int classId); 
        Task AddGradesAsync(IEnumerable<StudentGrade> grades);
        Task AddAbsencesAsync(IEnumerable<StudentGrade> absences);

    }
}