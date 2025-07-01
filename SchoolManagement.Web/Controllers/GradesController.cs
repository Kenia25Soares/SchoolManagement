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

        // GET: /EmployeeDashboard/Grades
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
            {
                classId = int.Parse(classes.First().Value);
            }

            foreach (var c in classes)
            {
                c.Selected = classId.HasValue && c.Value == classId.Value.ToString();
            }

            IQueryable<StudentUser> studentsQuery = _context.Users.OfType<StudentUser>();

            if (classId.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.StudentClassId == classId.Value);
            }
            else
            {
                studentsQuery = studentsQuery.Where(s => false);
            }

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
                    .Select(g => new
                    {
                        Subject = g.Key,
                        TotalAbsences = g.Sum(x => x.Absences)
                    });

                bool failedByAbsences = absencesBySubject.Any(a => a.TotalAbsences > a.Subject.AllowedAbsences);
                failedByAbsencesDict[student.Id] = failedByAbsences;
            }

            var grades = await _context.StudentGrades
                .Where(g => studentIds.Contains(g.StudentId) && g.GradeTypeId != null)
                .Include(g => g.GradeType)
                .ToListAsync();

            var averages = grades
                .GroupBy(g => g.StudentId)
                .ToDictionary(g => g.Key, g =>
                {
                    var groupedByType = g.GroupBy(x => x.GradeType);
                    double weightedSum = 0;
                    double totalWeight = 0;

                    foreach (var group in groupedByType)
                    {
                        var weight = group.Key?.Weight ?? 0;
                        if (weight > 0)
                        {
                            var avgGrade = group.Average(x => x.Grade ?? 0);
                            weightedSum += avgGrade * weight;
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

        // GET: /EmployeeDashboard/Grades/AddGrades?studentId=xyz
        [HttpGet("AddGrades")]
        public async Task<IActionResult> AddGrades(string studentId)
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

        // POST: /EmployeeDashboard/Grades/AddGrades
        [HttpPost("AddGrades")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGrades(AddGradesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var student = await _context.Users.OfType<StudentUser>()
                    .Include(s => s.StudentClass)
                    .FirstOrDefaultAsync(s => s.Id == model.StudentId);

                if (student == null)
                    return NotFound();

                if (student.StudentClass == null)
                {
                    ModelState.AddModelError("", "Student does not have a class assigned.");
                    return View("/Views/EmployeeDashboard/Grades/AddGrades.cshtml", model);
                }

                var subjects = await _context.CourseSubjects
                    .Where(cs => cs.CourseId == student.StudentClass.CourseId)
                    .Select(cs => cs.Subject)
                    .ToListAsync();

                var gradeTypes = await _context.GradeTypes.ToListAsync();

                model.Subjects = subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
                model.GradeTypes = gradeTypes.Select(gt => new SelectListItem { Value = gt.Id.ToString(), Text = gt.Name });

                ViewBag.StudentName = student.FullName;

                return View("/Views/EmployeeDashboard/Grades/AddGrades.cshtml", model);
            }

            var studentEntity = await _context.Users.OfType<StudentUser>()
                                    .Include(s => s.StudentClass)
                                    .FirstOrDefaultAsync(s => s.Id == model.StudentId);

            if (studentEntity == null)
                return NotFound();

            if (!studentEntity.StudentClassId.HasValue)
                return BadRequest("Student does not have a class assigned.");

            var courseId = studentEntity.StudentClass.CourseId;

            foreach (var gradeInput in model.Grades)
            {
                if (gradeInput.SubjectId == 0 || gradeInput.GradeTypeId == 0)
                    continue;

                var grade = new StudentGrade
                {
                    StudentId = model.StudentId,
                    CourseId = courseId,
                    SubjectId = gradeInput.SubjectId,
                    GradeTypeId = gradeInput.GradeTypeId,
                    Grade = gradeInput.Grade,
                    Absences = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.StudentGrades.Add(grade);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Grades added successfully!";
            return RedirectToAction(nameof(Index), new { classId = studentEntity.StudentClassId });
        }

        // GET: /EmployeeDashboard/Grades/AddAbsences?studentId=xyz
        [HttpGet("AddAbsences")]
        public async Task<IActionResult> AddAbsences(string studentId)
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

        // POST: /EmployeeDashboard/Grades/AddAbsences
        [HttpPost("AddAbsences")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAbsences(AddAbsencesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var student = await _context.Users.OfType<StudentUser>()
                    .Include(s => s.StudentClass)
                    .FirstOrDefaultAsync(s => s.Id == model.StudentId);

                if (student == null)
                    return NotFound();

                if (student.StudentClass == null)
                {
                    ModelState.AddModelError("", "Student does not have a class assigned.");
                    return View("/Views/EmployeeDashboard/Grades/AddAbsences.cshtml", model);
                }

                var subjects = await _context.CourseSubjects
                    .Where(cs => cs.CourseId == student.StudentClass.CourseId)
                    .Select(cs => cs.Subject)
                    .ToListAsync();

                model.Subjects = subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });

                ViewBag.StudentName = student.FullName;

                return View("/Views/EmployeeDashboard/Grades/AddAbsences.cshtml", model);
            }

            var studentEntity = await _context.Users.OfType<StudentUser>()
                .Include(s => s.StudentClass)
                .FirstOrDefaultAsync(s => s.Id == model.StudentId);

            if (studentEntity == null || !studentEntity.StudentClassId.HasValue)
                return BadRequest();

            var courseId = studentEntity.StudentClass.CourseId;

            foreach (var absenceInput in model.Absences)
            {
                if (absenceInput.SubjectId == 0)
                    continue;

                var grade = new StudentGrade
                {
                    StudentId = model.StudentId,
                    CourseId = courseId,
                    SubjectId = absenceInput.SubjectId,
                    GradeTypeId = null,
                    Grade = null,
                    Absences = absenceInput.Absences,
                    CreatedAt = DateTime.UtcNow
                };

                _context.StudentGrades.Add(grade);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Absences added successfully!";
            return RedirectToAction(nameof(Index), new { classId = studentEntity.StudentClassId });
        }

        // GET: /EmployeeDashboard/Grades/Details?studentId=xyz
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
                double weightedSum = 0;
                double totalWeight = 0;

                foreach (var gradeTypeGroup in subject.GradesByType)
                {
                    if (gradeTypeGroup.Weight > 0 && gradeTypeGroup.Grades.Any())
                    {
                        double avg = gradeTypeGroup.Grades.Average();
                        weightedSum += avg * gradeTypeGroup.Weight;
                        totalWeight += gradeTypeGroup.Weight;
                    }
                }

                subject.WeightedAverage = totalWeight > 0 ? weightedSum / totalWeight : 0;
            }

            double totalWeightedSum = 0;
            double totalWeightsCount = 0;

            foreach (var subj in groupedGrades)
            {
                var countGradesWithWeight = subj.GradesByType.Where(gt => gt.Weight > 0).Sum(gt => gt.Grades.Count);
                totalWeightedSum += subj.WeightedAverage * countGradesWithWeight;
                totalWeightsCount += countGradesWithWeight;
            }

            double totalAverage = totalWeightsCount > 0 ? totalWeightedSum / totalWeightsCount : 0;

            var model = new StudentGradesDetailsViewModel
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                SubjectGrades = groupedGrades,
                TotalAverage = totalAverage
            };

            return View("/Views/EmployeeDashboard/Grades/Details.cshtml", model);
        }

        // GET: /EmployeeDashboard/Grades/ViewAbsences?studentId=xyz
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
