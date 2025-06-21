using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Helpers
{
    public class StudentAbsenceHelper : IStudentAbsenceHelper
    {
        private readonly DataContext _context;

        public StudentAbsenceHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<StudentAbsencesViewModel> GetAbsencesAsync(string studentId)
        {
            var absencesData = await _context.StudentGrades
                .Include(sg => sg.Subject)
                .Where(sg => sg.StudentId == studentId)
                .GroupBy(sg => sg.Subject)
                .Select(g => new AbsenceSummaryViewModel
                {
                    SubjectName = g.Key.Name,
                    TotalAbsences = g.Sum(x => x.Absences)
                })
                .ToListAsync();

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == studentId);

            var model = new StudentAbsencesViewModel
            {
                StudentId = studentId,
                StudentName = student?.FullName ?? "Unknown",
                Absences = absencesData
            };

            return model;
        }
    }
}
