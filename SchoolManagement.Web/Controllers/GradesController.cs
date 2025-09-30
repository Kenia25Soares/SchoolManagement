using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using SchoolManagement.Web.Services;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    [Route("EmployeeDashboard/Grades")]
    public class GradesController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IGradesRepository _gradesRepository;
        private readonly IStudentClassRepository _studentClassRepository;
        private readonly IAlertService _alertService;

        public GradesController(IUserHelper userHelper, IGradesRepository gradesRepository,
            IStudentClassRepository studentClassRepository, IAlertService alertService)
        {
            _userHelper = userHelper;
            _gradesRepository = gradesRepository;
            _studentClassRepository = studentClassRepository;
            _alertService = alertService;
        }


        /// <summary>
        /// Sets the current user's profile picture in the view data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SetUserProfilePictureAsync()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity?.Name ?? string.Empty);
            ViewData["ProfilePictureUrl"] = user?.ProfilePictureUrl;
        }


        /// <summary>
        /// Displays a list of students, their average grades, and absence status for a selected class.
        /// </summary>
        /// <param name="classId">Optional class ID to filter students.</param>
        /// <returns>Index view with student grade summaries.</returns>
        [HttpGet]
        public async Task<IActionResult> Index(int? classId)
        {
            await SetUserProfilePictureAsync();

            var classes = await _gradesRepository.GetClassSelectListAsync(classId);

            if (!classId.HasValue && classes.Count > 0)
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
                    AverageGrade = s.User != null && averages.TryGetValue(s.User.Id, out var avg) ? avg : (double?)null,
                    FailedDueToAbsences = s.User != null &&
                        failedByAbsencesDict.ContainsKey(s.User.Id) &&
                        failedByAbsencesDict[s.User.Id],
                }).ToList(),
                IsClassClosed = isClassClosed
            };

            //Views/EmployeeDashboard/Grades/Index
            return View(model);
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
                Subjects = subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }),
                GradeTypes = gradeTypes.Select(gt => new SelectListItem { Value = gt.Id.ToString(), Text = gt.Name })
            };

            ViewBag.StudentName = student.User.FullName;
            //Views/EmployeeDashboard/Grades/AddGrades
            return View(model);
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
                    //Views/EmployeeDashboard/Grades/AddGrades
                    return View(model);
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

            // Get the saved grades with their generated IDs
            var savedGrades = await _gradesRepository.GetGradesWithSubjectsAndTypesAsync(model.StudentId);
            var newlyAddedGrades = savedGrades.Where(g => 
                model.Grades.Any(mg => 
                    mg.SubjectId == g.SubjectId && 
                    mg.GradeTypeId == g.GradeTypeId && 
                    mg.Grade == g.Grade
                )
            ).ToList();

            // Create alerts for each added grade
            foreach (var grade in newlyAddedGrades)
            {
                var gradeType = await _gradesRepository.GetGradeTypeByIdAsync(grade.GradeTypeId.Value);
                var subject = await _gradesRepository.GetSubjectByIdAsync(grade.SubjectId);
                
                await _alertService.CreateGradePostedAlertAsync(
                    grade.StudentId, 
                    grade.SubjectId, 
                    grade.Id, 
                    grade.Grade, 
                    gradeType?.Name ?? "Grade"
                );
            }

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

            // Get all students in the class and create alerts
            var studentsInClass = await _gradesRepository.GetStudentsByClassIdAsync(classId);
            var studentIds = studentsInClass.Select(s => s.UserId).ToList();
            
            if (studentIds.Any())
            {
                await _alertService.CreateClassClosedAlertAsync(studentIds, classId, studentClass.Name);
            }

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

            // Agrupar notas por disciplina 
            var groupedGrades = grades
                .GroupBy(g => g.Subject)
                .Select(g => new SubjectGradesViewModel
                {
                    SubjectName = g.Key?.Name ?? "Unknown",
                    GradesByType = g.GroupBy(x => x.GradeType)
                .Where(gt => gt.Key != null) 
                .Select(gt => new GradeTypeGroupViewModel
                {
                    GradeTypeName = gt.Key!.Name,  
                    Weight = gt.Key.Weight,
                    Grades = gt.Select(x => x.Grade ?? 0).ToList()
                }).ToList(),
                    AllowedAbsences = g.Key?.AllowedAbsences ?? 0,
                    TotalAbsences = absences.Where(a => a.SubjectId == g.Key!.Id).Sum(a => a.Absences)
                }).ToList();

            foreach (var subject in groupedGrades) // Cálculo das mediias por disciplina
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

            // Cálculo para a média geral
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

            if (groupedGrades.Any(g => g.TotalAbsences > g.AllowedAbsences))
            {
                var profile = await _gradesRepository.GetStudentProfileByUserIdAsync(student.User.Id);
                if (profile != null && !profile.IsExcludedDueToAbsences)
                {
                    profile.IsExcludedDueToAbsences = true;
                    await _gradesRepository.UpdateStudentProfileAsync(profile);
                }
            }

            //Views/EmployeeDashboard/Grades/Details
            return View(model);
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
            //Views/EmployeeDashboard/Grades/AddAbsences
            return View(model);
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

            // Get the saved absences with their generated IDs
            var savedAbsences = await _gradesRepository.GetAbsencesByStudentAsync(model.StudentId);
            var newlyAddedAbsences = savedAbsences.Where(a => 
                model.Absences.Any(ma => 
                    ma.SubjectId == a.SubjectId && 
                    ma.Absences == a.Absences
                )
            ).ToList();

            // Create alerts for attendance records
            foreach (var absence in newlyAddedAbsences)
            {
                var subject = await _gradesRepository.GetSubjectByIdAsync(absence.SubjectId);
                await _alertService.CreateGradePostedAlertAsync(
                    absence.StudentId, 
                    absence.SubjectId, 
                    absence.Id, 
                    null, // No grade value for absences
                    "Attendance"
                );
            }

            // ver se o aluno ultrapassou o limite das faltas permitidas
            var absencesAfterSave = await _gradesRepository.GetAbsencesByStudentAsync(model.StudentId);

            var subjectsExceeded = absencesAfterSave
                .GroupBy(a => a.Subject)
                .Where(g => g.Sum(x => x.Absences) > g.Key.AllowedAbsences)
                .ToList();

            if (subjectsExceeded.Any())
            {
                var profile = await _gradesRepository.GetStudentProfileByUserIdAsync(model.StudentId);
                if (profile != null && !profile.IsExcludedDueToAbsences)
                {
                    profile.IsExcludedDueToAbsences = true;
                    await _gradesRepository.UpdateStudentProfileAsync(profile);

                    // Create alerts for each subject where student exceeded absences
                    foreach (var subjectGroup in subjectsExceeded)
                    {
                        var totalAbsences = subjectGroup.Sum(x => x.Absences);
                        var allowedAbsences = subjectGroup.Key.AllowedAbsences;
                        
                        await _alertService.CreateExcludedByAbsencesAlertAsync(
                            model.StudentId,
                            subjectGroup.Key.Id,
                            subjectGroup.Key.Name,
                            totalAbsences,
                            allowedAbsences
                        );
                    }
                }
            }

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

            //Views/EmployeeDashboard/Grades/ViewAbsences
            return View(model);
        }
    }
}