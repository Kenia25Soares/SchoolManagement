using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Helpers
{
    public class StudentGradeHelper : IStudentGradeHelper
    {
        private readonly IGradesRepository _gradesRepository;
        private readonly IStudentProfileRepository _studentProfileRepository;

        public StudentGradeHelper(IGradesRepository gradesRepository, IStudentProfileRepository studentProfileRepository)
        {
            _gradesRepository = gradesRepository;
            _studentProfileRepository = studentProfileRepository;
        }

        public async Task<StudentGradesDetailsViewModel> GetGradesDetailsAsync(string studentId)
        {
            // Busca todas as notas com disciplinas e os tipos pelo repository
            var grades = await _gradesRepository.GetGradesWithSubjectsAndTypesAsync(studentId);

            // Busca o perfil do estudante para pegar infomação da classe e nome
            var studentProfile = await _studentProfileRepository.GetByUserIdAsync(studentId);

            // Agrupa as notas por disciplina
            var groupedGrades = grades
                .Where(g => g.Subject != null)
                .GroupBy(g => g.Subject)
                .Select(subjectGroup => new SubjectGradesViewModel
                {
                    SubjectName = subjectGroup.Key.Name,
                    GradesByType = subjectGroup
                        .Where(g => g.GradeType != null)
                        .GroupBy(g => g.GradeType)
                        .Select(gt => new GradeTypeGroupViewModel
                        {
                            GradeTypeName = gt.Key?.Name ?? "N/A",
                            Weight = gt.Key?.Weight ?? 0,
                            Grades = gt.Select(x => x.Grade ?? 0).ToList()
                        }).ToList(),
                    AllowedAbsences = subjectGroup.Key.AllowedAbsences,
                    TotalAbsences = grades
                        .Where(a => a.StudentId == studentId && a.SubjectId == subjectGroup.Key.Id && a.GradeTypeId == null)
                        .Sum(a => a.Absences)
                }).ToList();


            // Calcula médias pelo peso e a reprovação por faltas
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

            // Calcula média geral pelas notas e pesos
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
                StudentName = studentProfile?.User?.FullName ?? "Unknown",
                SubjectGrades = groupedGrades,
                TotalAverage = totalWeights > 0 ? totalSum / totalWeights : 0,
                IsClassClosed = studentProfile?.StudentClass?.IsClosed ?? false
            };
            //var grades = await _context.StudentGrades
            //    .Where(g => g.StudentId == studentId && g.GradeTypeId != null && g.Grade.HasValue)
            //    .Include(g => g.Subject)
            //    .Include(g => g.GradeType)
            //    .ToListAsync();

            //var student = await _context.StudentProfiles
            //    .Include(s => s.User)
            //    .Include(s => s.StudentClass)
            //    .FirstOrDefaultAsync(s => s.UserId == studentId);

            //var groupedGrades = grades
            //    .GroupBy(g => g.Subject)
            //    .Select(subjectGroup => new SubjectGradesViewModel
            //    {
            //        SubjectName = subjectGroup.Key.Name,
            //        GradesByType = subjectGroup
            //            .GroupBy(g => g.GradeType)
            //            .Select(gt => new GradeTypeGroupViewModel
            //            {
            //                GradeTypeName = gt.Key.Name,
            //                Weight = gt.Key.Weight,
            //                Grades = gt.Select(x => x.Grade.Value).ToList()
            //            }).ToList(),
            //        AllowedAbsences = subjectGroup.Key.AllowedAbsences,
            //        TotalAbsences = _context.StudentGrades
            //            .Where(a => a.StudentId == studentId && a.SubjectId == subjectGroup.Key.Id && a.GradeTypeId == null)
            //            .Sum(a => a.Absences)
            //    }).ToList();

            //foreach (var subject in groupedGrades)
            //{
            //    double weightedSum = 0;
            //    double totalWeight = 0;

            //    foreach (var gt in subject.GradesByType)
            //    {
            //        if (gt.Weight > 0 && gt.Grades.Any())
            //        {
            //            weightedSum += gt.Grades.Average() * gt.Weight;
            //            totalWeight += gt.Weight;
            //        }
            //    }

            //    subject.WeightedAverage = totalWeight > 0 ? weightedSum / totalWeight : 0;
            //    subject.FailedDueToAbsences = subject.TotalAbsences > subject.AllowedAbsences;
            //}

            //double totalSum = 0;
            //double totalWeights = 0;

            //foreach (var s in groupedGrades)
            //{
            //    foreach (var g in s.GradesByType)
            //    {
            //        if (g.Weight > 0 && g.Grades.Any())
            //        {
            //            foreach (var grade in g.Grades)
            //            {
            //                totalSum += grade * g.Weight;
            //                totalWeights += g.Weight;
            //            }
            //        }
            //    }
            //}

            //return new StudentGradesDetailsViewModel
            //{
            //    StudentId = studentId,
            //    StudentName = student?.User?.FullName ?? "",
            //    SubjectGrades = groupedGrades,
            //    TotalAverage = totalWeights > 0 ? totalSum / totalWeights : 0,
            //    IsClassClosed = student?.StudentClass?.IsClosed ?? false
            //};
        }
    }
}
