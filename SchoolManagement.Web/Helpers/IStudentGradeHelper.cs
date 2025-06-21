using SchoolManagement.Web.Models;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Helpers
{
    public interface IStudentGradeHelper
    {
        Task<StudentGradesDetailsViewModel> GetGradesDetailsAsync(string studentId);
    }
}
