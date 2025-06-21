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

        public StudentGradeHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<StudentGradesDetailsViewModel> GetGradesDetailsAsync(string studentId)
        {
            var grades = await _context.StudentGrades
                .Include(g => g.Subject)
                .Include(g => g.GradeType) 
                .Where(g => g.StudentId == studentId)
                .ToListAsync();

            var model = new StudentGradesDetailsViewModel
            {
                StudentName = "",
                SubjectGrades = new List<SubjectGradesViewModel>()
            };

            var groupedBySubject = grades.GroupBy(g => g.Subject.Name);

            foreach (var subjectGroup in groupedBySubject)
            {
                var subjectGradesVm = new SubjectGradesViewModel
                {
                    SubjectName = subjectGroup.Key,
                    GradesByType = new List<GradeTypeGroupViewModel>()
                };

                var gradesGroupedByType = subjectGroup.GroupBy(g => g.GradeType);

                foreach (var gradeTypeGroup in gradesGroupedByType)
                {
                    var gradeType = gradeTypeGroup.Key;

                    var gradesList = gradeTypeGroup
                        .Where(g => g.Grade.HasValue)
                        .Select(g => g.Grade.Value)
                        .ToList();

                    var gradeTypeVm = new GradeTypeGroupViewModel
                    {
                        GradeTypeName = gradeType?.Name ?? "Unknown",
                        Grades = gradesList,
                        Weight = gradeType?.Weight ?? 1.0
                    };

                    subjectGradesVm.GradesByType.Add(gradeTypeVm);
                }

                double weightedSum = subjectGradesVm.GradesByType.Sum(gt =>
                    gt.Grades.Sum() * gt.Weight
                );

                double totalWeight = subjectGradesVm.GradesByType.Sum(gt =>
                    gt.Weight * gt.Grades.Count
                );

                subjectGradesVm.WeightedAverage = totalWeight == 0 ? 0 : weightedSum / totalWeight;

                model.SubjectGrades.Add(subjectGradesVm);
            }

            double totalWeightedSum = model.SubjectGrades.Sum(sg =>
                sg.WeightedAverage
            );

            model.TotalAverage = model.SubjectGrades.Count == 0 ? 0 : totalWeightedSum / model.SubjectGrades.Count;

            return model;
        }
    }
}
