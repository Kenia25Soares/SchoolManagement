using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Helpers
{
    public class StudentGradeHelper : IStudentGradeHelper
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public StudentGradeHelper(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task<StudentGradesDetailsViewModel> GetGradesDetailsAsync(string studentId)
        {
            var grades = await _context.StudentGrades
                .Where(g => g.StudentId == studentId && g.GradeTypeId != null && g.Grade.HasValue)
                .Include(g => g.Subject)
                .Include(g => g.GradeType)
                .ToListAsync();

            var student = await _context.StudentProfiles
                .Include(s => s.User)
                .Include(s => s.StudentClass)
                .FirstOrDefaultAsync(s => s.UserId == studentId);

            var groupedGrades = grades
                .GroupBy(g => g.Subject)
                .Select(subjectGroup => new SubjectGradesViewModel
                {
                    SubjectName = subjectGroup.Key.Name,
                    GradesByType = subjectGroup
                        .GroupBy(g => g.GradeType)
                        .Select(gt => new GradeTypeGroupViewModel
                        {
                            GradeTypeName = gt.Key.Name,
                            Weight = gt.Key.Weight,
                            Grades = gt.Select(x => x.Grade.Value).ToList()
                        }).ToList(),
                    AllowedAbsences = subjectGroup.Key.AllowedAbsences,
                    TotalAbsences = _context.StudentGrades
                        .Where(a => a.StudentId == studentId && a.SubjectId == subjectGroup.Key.Id && a.GradeTypeId == null)
                        .Sum(a => a.Absences)
                }).ToList();

            foreach (var subject in groupedGrades)
            {
                double weightedSum = 0;
                double totalWeight = 0;

                foreach (var gt in subject.GradesByType)
                {
                    if (gt.Weight > 0 && gt.Grades.Any())
                    {
                        weightedSum += gt.Grades.Average() * gt.Weight;
                        totalWeight += gt.Weight;
                    }
                }

                subject.WeightedAverage = totalWeight > 0 ? weightedSum / totalWeight : 0;
                subject.FailedDueToAbsences = subject.TotalAbsences > subject.AllowedAbsences;
            }

            double totalSum = 0;
            double totalWeights = 0;

            foreach (var s in groupedGrades)
            {
                foreach (var g in s.GradesByType)
                {
                    if (g.Weight > 0 && g.Grades.Any())
                    {
                        foreach (var grade in g.Grades)
                        {
                            totalSum += grade * g.Weight;
                            totalWeights += g.Weight;
                        }
                    }
                }
            }

            return new StudentGradesDetailsViewModel
            {
                StudentId = studentId,
                StudentName = student?.User?.FullName ?? "",
                SubjectGrades = groupedGrades,
                TotalAverage = totalWeights > 0 ? totalSum / totalWeights : 0,
                IsClassClosed = student?.StudentClass?.IsClosed ?? false
            };
        }
    }
}
