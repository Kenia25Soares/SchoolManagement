using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    [Route("EmployeeDashboard/Grades")]
    public class GradesController : Controller
    {
        private readonly DataContext _context;

        public GradesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? classId)
        {
            var classes = await _context.StudentClasses
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();

            if (!classId.HasValue && classes.Any())
                classId = int.Parse(classes.First().Value);

            foreach (var c in classes)
                c.Selected = classId.HasValue && c.Value == classId.Value.ToString();

            var studentsQuery = _context.Users.OfType<StudentUser>();
            if (classId.HasValue)
                studentsQuery = studentsQuery.Where(s => s.StudentClassId == classId.Value);
            else
                studentsQuery = studentsQuery.Where(s => false);

            var students = await studentsQuery.ToListAsync();
            var studentIds = students.Select(s => s.Id).ToList();

            var absences = await _context.StudentGrades
                .Where(g => studentIds.Contains(g.StudentId) && g.Absences > 0)
                .Include(g => g.Subject)
                .ToListAsync();

            var failedByAbsencesDict = new Dictionary<string, bool>();
            foreach (var student in students)
            {
                var absencesBySubject = absences
                    .Where(a => a.StudentId == student.Id)
                    .GroupBy(a => a.Subject)
                    .Select(g => new { Subject = g.Key, TotalAbsences = g.Sum(x => x.Absences) });

                bool failed = absencesBySubject.Any(a => a.TotalAbsences > a.Subject.AllowedAbsences);
                failedByAbsencesDict[student.Id] = failed;
            }

            var grades = await _context.StudentGrades
                .Where(g => studentIds.Contains(g.StudentId) && g.GradeTypeId != null)
                .Include(g => g.GradeType)
                .ToListAsync();

            var averages = grades
                .GroupBy(g => g.StudentId)
                .ToDictionary(g => g.Key, g =>
                {
                    double weightedSum = 0, totalWeight = 0;
                    foreach (var group in g.GroupBy(x => x.GradeType))
                    {
                        var weight = group.Key?.Weight ?? 0;
                        if (weight > 0)
                        {
                            var avg = group.Average(x => x.Grade ?? 0);
                            weightedSum += avg * weight;
                            totalWeight += weight;
                        }
                    }
                    return totalWeight > 0 ? weightedSum / totalWeight : 0;
                });

            var model = new StudentGradesIndexViewModel
            {
                Classes = classes,
                Students = students.Select(s => new UserListViewModel
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    ProfilePictureUrl = s.ProfilePictureUrl,
                    AverageGrade = averages.ContainsKey(s.Id) ? averages[s.Id] : (double?)null,
                    FailedDueToAbsences = failedByAbsencesDict.ContainsKey(s.Id) && failedByAbsencesDict[s.Id]
                }).ToList()
            };

            return View("/Views/EmployeeDashboard/Grades/Index.cshtml", model);
        }

        [HttpGet("AddGrades")]
        public async Task<IActionResult> AddGrades(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
                return BadRequest();

            var student = await _context.Users.OfType<StudentUser>()
                .Include(s => s.StudentClass)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null || student.StudentClass == null)
            {
                TempData["ErrorMessage"] = "Student not found or has no class assigned.";
                return RedirectToAction(nameof(Index));
            }

            var subjects = await _context.CourseSubjects
                .Where(cs => cs.CourseId == student.StudentClass.CourseId)
                .Select(cs => cs.Subject)
                .ToListAsync();

            var gradeTypes = await _context.GradeTypes.ToListAsync();

            var model = new AddGradesViewModel
            {
                StudentId = studentId,
                Grades = new List<GradeInputModel> { new GradeInputModel() },
                Subjects = subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }),
                GradeTypes = gradeTypes.Select(gt => new SelectListItem { Value = gt.Id.ToString(), Text = gt.Name })
            };

            ViewBag.StudentName = student.FullName;
            return View("/Views/EmployeeDashboard/Grades/AddGrades.cshtml", model);
        }

        [HttpPost("AddGrades")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGrades(AddGradesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input. Please check the form.";
                return RedirectToAction(nameof(Index));
            }

            var student = await _context.Users.OfType<StudentUser>()
                .Include(s => s.StudentClass)
                .FirstOrDefaultAsync(s => s.Id == model.StudentId);

            if (student == null || student.StudentClass == null)
            {
                TempData["ErrorMessage"] = "Student not found or not assigned to a class.";
                return RedirectToAction(nameof(Index));
            }

            var courseId = student.StudentClass.CourseId;

            foreach (var input in model.Grades)
            {
                if (input.SubjectId == 0 || input.GradeTypeId == 0)
                    continue;

                _context.StudentGrades.Add(new StudentGrade
                {
                    StudentId = model.StudentId,
                    CourseId = courseId,
                    SubjectId = input.SubjectId,
                    GradeTypeId = input.GradeTypeId,
                    Grade = input.Grade,
                    Absences = 0,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Grades added successfully!";
            return RedirectToAction(nameof(Index), new { classId = student.StudentClassId });
        }

        [HttpGet("AddAbsences")]
        public async Task<IActionResult> AddAbsences(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
                return BadRequest();

            var student = await _context.Users.OfType<StudentUser>()
                .Include(s => s.StudentClass)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null || student.StudentClass == null)
            {
                TempData["ErrorMessage"] = "Student not found or has no class assigned.";
                return RedirectToAction(nameof(Index));
            }

            var subjects = await _context.CourseSubjects
                .Where(cs => cs.CourseId == student.StudentClass.CourseId)
                .Select(cs => cs.Subject)
                .ToListAsync();

            var model = new AddAbsencesViewModel
            {
                StudentId = studentId,
                Absences = new List<AbsenceInputModel> { new AbsenceInputModel() },
                Subjects = subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            };

            ViewBag.StudentName = student.FullName;
            return View("/Views/EmployeeDashboard/Grades/AddAbsences.cshtml", model);
        }

        [HttpPost("AddAbsences")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAbsences(AddAbsencesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input. Please check the form.";
                return RedirectToAction(nameof(Index));
            }

            var student = await _context.Users.OfType<StudentUser>()
                .Include(s => s.StudentClass)
                .FirstOrDefaultAsync(s => s.Id == model.StudentId);

            if (student == null || student.StudentClass == null)
            {
                TempData["ErrorMessage"] = "Student not found or not assigned to a class.";
                return RedirectToAction(nameof(Index));
            }

            var courseId = student.StudentClass.CourseId;

            foreach (var input in model.Absences)
            {
                if (input.SubjectId == 0) continue;

                _context.StudentGrades.Add(new StudentGrade
                {
                    StudentId = model.StudentId,
                    CourseId = courseId,
                    SubjectId = input.SubjectId,
                    GradeTypeId = null,
                    Grade = null,
                    Absences = input.Absences,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Absences added successfully!";
            return RedirectToAction(nameof(Index), new { classId = student.StudentClassId });
        }

        [HttpGet("Details")]
        public async Task<IActionResult> Details(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
                return BadRequest();

            var student = await _context.Users.OfType<StudentUser>()
                .Include(s => s.StudentClass)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                return NotFound();

            if (student.StudentClass == null)
                return BadRequest("Student does not have a class assigned.");

            var grades = await _context.StudentGrades
                .Where(g => g.StudentId == studentId && g.GradeTypeId != null)
                .Include(g => g.Subject)
                .Include(g => g.GradeType)
                .ToListAsync();

            var groupedGrades = grades
                .GroupBy(g => g.Subject)
                .Select(g => new SubjectGradesViewModel
                {
                    SubjectName = g.Key.Name,
                    GradesByType = g
                        .GroupBy(x => x.GradeType)
                        .Select(gtGroup => new GradeTypeGroupViewModel
                        {
                            GradeTypeName = gtGroup.Key.Name,
                            Weight = gtGroup.Key.Weight,
                            Grades = gtGroup.Select(x => x.Grade ?? 0).ToList()
                        })
                        .ToList()
                })
                .ToList();

            foreach (var subject in groupedGrades)
            {
                double subjectWeightedSum = 0;
                double subjectWeightTotal = 0;

                foreach (var gt in subject.GradesByType)
                {
                    if (gt.Weight > 0 && gt.Grades.Any())
                    {
                        var avg = gt.Grades.Average();
                        subjectWeightedSum += avg * gt.Weight;
                        subjectWeightTotal += gt.Weight;
                    }
                }

                subject.WeightedAverage = subjectWeightTotal > 0 ? subjectWeightedSum / subjectWeightTotal : 0;
            }

            double totalWeightedSum = 0;
            double totalOverallWeight = 0;

            foreach (var subject in groupedGrades)
            {
                foreach (var gt in subject.GradesByType)
                {
                    if (gt.Weight > 0 && gt.Grades.Any())
                    {
                        foreach (var grade in gt.Grades)
                        {
                            totalWeightedSum += grade * gt.Weight;
                            totalOverallWeight += gt.Weight;
                        }
                    }
                }
            }

            double totalAverage = totalOverallWeight > 0 ? totalWeightedSum / totalOverallWeight : 0;

            var model = new StudentGradesDetailsViewModel
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                SubjectGrades = groupedGrades,
                TotalAverage = totalAverage
            };

            return View("/Views/EmployeeDashboard/Grades/Details.cshtml", model);
        }

        [HttpGet("ViewAbsences")]
        public async Task<IActionResult> ViewAbsences(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
                return BadRequest();

            var student = await _context.Users.OfType<StudentUser>()
                .Include(s => s.StudentClass)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                return NotFound();

            if (student.StudentClass == null)
                return BadRequest("Student does not have a class assigned.");

            var absences = await _context.StudentGrades
                .Where(g => g.StudentId == studentId && g.Absences > 0)
                .Include(g => g.Subject)
                .ToListAsync();

            var model = new StudentAbsencesViewModel
            {
                StudentId = studentId,
                StudentName = student.FullName,
                Absences = absences
                    .GroupBy(a => a.Subject.Name)
                    .Select(g => new AbsenceSummaryViewModel
                    {
                        SubjectName = g.Key,
                        TotalAbsences = g.Sum(x => x.Absences)
                    })
                    .ToList()
            };

            return View("/Views/EmployeeDashboard/Grades/ViewAbsences.cshtml", model);
        }
    }
}
