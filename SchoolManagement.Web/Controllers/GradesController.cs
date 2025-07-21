using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    [Route("EmployeeDashboard/Grades")]
    public class GradesController : Controller
    {
        private readonly IGradesRepository _gradesRepository;
        private readonly IStudentClassRepository _studentClassRepository;

        public GradesController(IGradesRepository gradesRepository,
            IStudentClassRepository studentClassRepository)
        {
            _gradesRepository = gradesRepository;
            _studentClassRepository = studentClassRepository;
        }

        
        /// <summary>
        /// Displays a list of students, their average grades, and absence status for a selected class.
        /// </summary>
        /// <param name="classId">Optional class ID to filter students.</param>
        /// <returns>Index view with student grade summaries.</returns>
        [HttpGet]
        public async Task<IActionResult> Index(int? classId)
        {
            var classes = await _gradesRepository.GetClassSelectListAsync(classId);

            if (!classId.HasValue && classes.Count > 0/*Any()*/)
                classId = int.Parse(classes.First().Value);

            var students = await _gradesRepository.GetStudentsByClassAsync(classId ?? 0);
            var studentIds = students.Select(s => s.User.Id).ToList();

            var absences = await _gradesRepository.GetAbsencesByStudentIdsAsync(studentIds);
            var failedByAbsencesDict = students.ToDictionary(
                s => s.User.Id,
                s => absences
                        .Where(a => a.StudentId == s.User.Id)
                        .GroupBy(a => a.Subject)
                        .Any(g => g.Sum(x => x.Absences) > g.Key.AllowedAbsences)
            );

            var grades = await _gradesRepository.GetGradesByStudentIdsAsync(studentIds);
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
                                         foreach (var grade in group)
                                         {
                                             if (grade.Grade.HasValue)
                                             {
                                                 weightedSum += grade.Grade.Value * weight;
                                                 totalWeight += weight;
                                             }
                                         }
                                     }
                                 }
                                 return totalWeight > 0 ? weightedSum / totalWeight : 0;
                             });

            bool isClassClosed = classId.HasValue && await _gradesRepository.IsClassClosedAsync(classId.Value);

            var model = new StudentGradesIndexViewModel
            {
                Classes = classes,
                Students = students.Select(s => new UserListViewModel
                {
                    Id = s.User.Id,
                    FullName = s.User.FullName,
                    Email = s.User?.Email ?? string.Empty,
                    ProfilePictureUrl = s.User?.ProfilePictureUrl ?? string.Empty,
                    //AverageGrade = (s.User != null && averages.ContainsKey(s.User.Id))
                    //    ? averages[s.User.Id]
                    //    : (double?)null,
                    AverageGrade = s.User != null && averages.TryGetValue(s.User.Id, out var avg) ? avg : (double?)null,
                    FailedDueToAbsences = s.User != null &&
                        failedByAbsencesDict.ContainsKey(s.User.Id) &&
                        failedByAbsencesDict[s.User.Id],
                }).ToList(),
                IsClassClosed = isClassClosed
            };

            return View("/Views/EmployeeDashboard/Grades/Index.cshtml", model);
        }


        /// <summary>
        /// Displays the form to add grades for a student.
        /// </summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <returns>Grade input form view.</returns>
        [HttpGet("AddGrades")]
        public async Task<IActionResult> AddGrades(string studentId)
        {
            var student = await _gradesRepository.GetStudentWithClassAsync(studentId);
            if (student?.StudentClass == null || student.StudentClass.IsClosed)
            {
                TempData["ErrorMessage"] = "Student not found or class is closed.";
                return RedirectToAction(nameof(Index), new { classId = student?.StudentClassId });
            }

            var subjects = await _gradesRepository.GetSubjectsByCourseAsync(student.StudentClass.CourseId);
            var gradeTypes = await _gradesRepository.GetGradeTypesAsync();

            var model = new AddGradesViewModel
            {
                StudentId = studentId,
                Grades = [new()],
                //Grades = new List<GradeInputModel> { new GradeInputModel() },
                Subjects = subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }),
                GradeTypes = gradeTypes.Select(gt => new SelectListItem { Value = gt.Id.ToString(), Text = gt.Name })
            };

            ViewBag.StudentName = student.User.FullName;
            return View("/Views/EmployeeDashboard/Grades/AddGrades.cshtml", model);
        }


        /// <summary>
        /// Processes grade submission for a student, with validation.
        /// </summary>
        /// <param name="model">Grade input model.</param>
        /// <returns>Redirects to index or redisplays form on error.</returns>
        [HttpPost("AddGrades")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGrades(AddGradesViewModel model)
        {
            var student = await _gradesRepository.GetStudentWithClassAsync(model.StudentId);
            if (student?.StudentClass == null || student.StudentClass.IsClosed)
            {
                TempData["ErrorMessage"] = "Student not found or class is closed.";
                return RedirectToAction(nameof(Index), new { classId = student?.StudentClassId });
            }

            // Validação das notas
            foreach (var g in model.Grades)
            {
                if (g.Grade.HasValue && g.Grade.Value > 20)
                {
                    ModelState.AddModelError("", "Grades cannot be greater than 20.");
                    var subjects = await _gradesRepository.GetSubjectsByCourseAsync(student.StudentClass.CourseId);
                    var gradeTypes = await _gradesRepository.GetGradeTypesAsync();

                    model.Subjects = subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
                    model.GradeTypes = gradeTypes.Select(gt => new SelectListItem { Value = gt.Id.ToString(), Text = gt.Name });

                    ViewBag.StudentName = student.User.FullName;
                    return View("/Views/EmployeeDashboard/Grades/AddGrades.cshtml", model);
                }
            }

            var grades = model.Grades
                .Where(g => g.SubjectId > 0 && g.GradeTypeId > 0)
                .Select(g => new StudentGrade
                {
                    StudentId = model.StudentId,
                    CourseId = student.StudentClass.CourseId,
                    SubjectId = g.SubjectId,
                    GradeTypeId = g.GradeTypeId,
                    Grade = g.Grade,
                    Absences = 0,
                    CreatedAt = DateTime.UtcNow
                });

            await _gradesRepository.AddGradesAsync(grades);
            //await _gradesRepository.SaveAllAsync();

            TempData["SuccessMessage"] = "Grades added successfully!";
            return RedirectToAction(nameof(Index), new { classId = student.StudentClassId });
        }


        /// <summary>
        /// Marks a class as closed to prevent further grade/absence entries.
        /// </summary>
        /// <param name="classId">The ID of the class to close.</param>
        /// <returns>Redirects to class overview.</returns>
        [HttpPost("CloseClass")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseClass(int classId)
        {
            var studentClass = await _studentClassRepository.GetByIdAsync(classId);
            if (studentClass == null)
            {
                TempData["ErrorMessage"] = "Class not found.";
                return RedirectToAction(nameof(Index));
            }

            if (studentClass.IsClosed)
            {
                TempData["InfoMessage"] = "Class is already closed.";
                return RedirectToAction(nameof(Index), new { classId });
            }

            studentClass.IsClosed = true;
            await _studentClassRepository.UpdateAsync(studentClass);
            //await _studentClassRepository.SaveAllAsync();

            TempData["SuccessMessage"] = "Class successfully closed.";
            return RedirectToAction(nameof(Index), new { classId });
        }


        /// <summary>
        /// Shows detailed grade and absence information for a specific student.
        /// </summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <returns>Detailed view of subject grades and absences.</returns>
        [HttpGet("Details")]
        public async Task<IActionResult> Details(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
                return BadRequest();

            var student = await _gradesRepository.GetStudentWithClassAsync(studentId);
            if (student == null || student.StudentClass == null)
                return NotFound();

            var grades = await _gradesRepository.GetGradesWithSubjectsAndTypesAsync(studentId);
            var absences = await _gradesRepository.GetAbsencesByStudentAsync(studentId);

            var groupedGrades = grades
                .GroupBy(g => g.Subject)
                .Select(g => new SubjectGradesViewModel
                {
                    SubjectName = g.Key?.Name ?? "Unkmown",
                    GradesByType = g.GroupBy(x => x.GradeType).Select(gt => new GradeTypeGroupViewModel
                    {
                        GradeTypeName = gt.Key?.Name ??"N/A",
                        Weight = gt.Key?.Weight?? 0,
                        Grades = gt.Select(x => x.Grade ?? 0).ToList()
                    }).ToList(),
                    AllowedAbsences = g.Key?.AllowedAbsences ?? 0,
                    TotalAbsences = absences.Where(a => a.SubjectId == g.Key!.Id).Sum(a => a.Absences)
                }).ToList();

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
                subject.FailedDueToAbsences = subject.TotalAbsences > subject.AllowedAbsences;
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
                StudentId = student.User.Id,
                StudentName = student.User.FullName,
                SubjectGrades = groupedGrades,
                TotalAverage = totalAverage,
                IsClassClosed = student.StudentClass?.IsClosed ?? false
            };

            return View("/Views/EmployeeDashboard/Grades/Details.cshtml", model);
        }


        /// <summary>
        /// Displays the form to add absences for a student.
        /// </summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <returns>Absence input form view.</returns>
        [HttpGet("AddAbsences")]
        public async Task<IActionResult> AddAbsences(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
                return BadRequest();

            var student = await _gradesRepository.GetStudentWithClassAsync(studentId);
            if (student?.StudentClass == null || student.StudentClass.IsClosed)
            {
                TempData["ErrorMessage"] = "Student not found or class is closed.";
                return RedirectToAction(nameof(Index), new { classId = student?.StudentClassId });
            }

            var subjects = await _gradesRepository.GetSubjectsByCourseAsync(student.StudentClass.CourseId);

            var model = new AddAbsencesViewModel
            {
                StudentId = studentId,
                Absences = [new()],
                //Absences = new List<AbsenceInputModel> { new AbsenceInputModel() },
                Subjects = subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            };

            ViewBag.StudentName = student.User.FullName;
            return View("/Views/EmployeeDashboard/Grades/AddAbsences.cshtml", model);
        }


        /// <summary>
        /// Processes absence data submission for a student.
        /// </summary>
        /// <param name="model">Absence input model.</param>
        /// <returns>Redirects to index or redisplays form on error.</returns>
        [HttpPost("AddAbsences")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAbsences(AddAbsencesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input.";
                return RedirectToAction(nameof(Index));
            }

            var student = await _gradesRepository.GetStudentWithClassAsync(model.StudentId);
            if (student?.StudentClass == null || student.StudentClass.IsClosed)
            {
                TempData["ErrorMessage"] = "Student not found or class is closed.";
                return RedirectToAction(nameof(Index), new { classId = student?.StudentClassId });
            }

            var absences = model.Absences
                .Where(a => a.SubjectId > 0)
                .Select(a => new StudentGrade
                {
                    StudentId = model.StudentId,
                    CourseId = student.StudentClass.CourseId,
                    SubjectId = a.SubjectId,
                    GradeTypeId = null,
                    Grade = null,
                    Absences = a.Absences,
                    CreatedAt = DateTime.UtcNow
                });

            await _gradesRepository.AddGradesAsync(absences);
           /* await _gradesRepository.SaveAllAsync()*/;

            TempData["SuccessMessage"] = "Absences added successfully!";
            return RedirectToAction(nameof(Index), new { classId = student.StudentClassId });
        }


        /// <summary>
        /// Displays a summary of absences per subject for a student.
        /// </summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <returns>View with summarized absences.</returns>
        [HttpGet("ViewAbsences")]
        public async Task<IActionResult> ViewAbsences(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
                return BadRequest();

            var student = await _gradesRepository.GetStudentWithClassAsync(studentId);
            if (student?.StudentClass == null)
                return BadRequest("Student does not have a class assigned.");

            var absences = await _gradesRepository.GetAbsencesByStudentAsync(studentId);

            var model = new StudentAbsencesViewModel
            {
                StudentId = studentId,
                StudentName = student.User.FullName,
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