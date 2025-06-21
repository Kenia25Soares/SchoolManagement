using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Helpers
{
    public interface IStudentAbsenceHelper
    {
        Task<StudentAbsencesViewModel> GetAbsencesAsync(string studentId);
    }
}
