using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Helpers
{
    public class StudentAbsenceHelper : IStudentAbsenceHelper
    {
        private readonly IGradesRepository _gradesRepository;
        private readonly IUserHelper _userHelper;

        public StudentAbsenceHelper(IGradesRepository gradesRepository, IUserHelper userHelper)
        {
            _gradesRepository = gradesRepository;
            _userHelper = userHelper;
        }

        public async Task<StudentAbsencesViewModel> GetAbsencesAsync(string studentId)
        {

            // Busca faltas agrupadas por disciplina via repositório
            var absences = await _gradesRepository.GetAbsencesByStudentAsync(studentId);

            var groupedAbsences = absences
                .GroupBy(a => a.Subject.Name)
                .Select(g => new AbsenceSummaryViewModel
                {
                    SubjectName = g.Key,
                    TotalAbsences = g.Sum(x => x.Absences)
                })
                .ToList();

            // Pega info do aluno
            var student = await _userHelper.GetUserByIdAsync(studentId);

            return new StudentAbsencesViewModel
            {
                StudentId = studentId,
                StudentName = student?.FullName ?? "Unknown",
                Absences = groupedAbsences
            };
            //var absencesData = await _context.StudentGrades
            //    .Include(sg => sg.Subject)
            //    .Where(sg => sg.StudentId == studentId)
            //    .GroupBy(sg => sg.Subject)
            //    .Select(g => new AbsenceSummaryViewModel
            //    {
            //        SubjectName = g.Key.Name,
            //        TotalAbsences = g.Sum(x => x.Absences)
            //    })
            //    .ToListAsync();

            //var student = await _context.Users
            //    .FirstOrDefaultAsync(u => u.Id == studentId);

            //var model = new StudentAbsencesViewModel
            //{
            //    StudentId = studentId,
            //    StudentName = student?.FullName ?? "Unknown",
            //    Absences = absencesData
            //};

            //return model;
        }
    }
}
