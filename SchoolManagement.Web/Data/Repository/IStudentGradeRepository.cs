using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IStudentGradeRepository : IGenericRepository<StudentGrade>
{
    Task<List<StudentGrade>> GetGradesByStudentIdsAsync(List<string> studentIds);
}

