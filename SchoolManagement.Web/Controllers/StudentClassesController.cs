using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using SchoolManagement.Web.Services;

namespace SchoolManagement.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    [Route("EmployeeDashboard/StudentClasses")]
    public class StudentClassesController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IStudentClassHelper _studentClassHelper;
        private readonly IStudentClassRepository _studentClassRepository;
        private readonly IStudentProfileRepository _studentProfileRepository;
        private readonly IAlertService _alertService;

        public StudentClassesController(
            IUserHelper userHelper,
            IStudentClassHelper studentClassHelper,
            IStudentClassRepository studentClassRepository,
            IStudentProfileRepository studentProfileRepository,
            IAlertService alertService)
        {
            _userHelper = userHelper;
            _studentClassHelper = studentClassHelper;
            _studentClassRepository = studentClassRepository;
            _studentProfileRepository = studentProfileRepository;
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
        /// Displays the list of all student classes.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUserProfilePictureAsync();

            var model = await _studentClassHelper.GetAllAsync();
            //Views/EmployeeDashboard/StudentClasses/Index
            return View(model);
        }


        /// <summary>
        /// Displays the form to create a new student class.
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new StudentClassViewModel
            {
                Courses = await _studentClassHelper.GetCoursesSelectListAsync()
            };
            //Views/EmployeeDashboard/StudentClasses/Create
            return View(vm);
        }


        /// <summary>
        /// Creates a new student class.
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentClassViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Courses = await _studentClassHelper.GetCoursesSelectListAsync(model.CourseId);
                TempData["ErrorMessage"] = "Invalid input. Please correct the form.";
               
                return View(model);
            }

            var entity = new StudentClass
            {
                Name = model.Name,
                AcademicYear = model.AcademicYear,
                Shift = model.Shift,
                CourseId = model.CourseId
            };

            await _studentClassRepository.CreateAsync(entity);
            TempData["SuccessMessage"] = "Student class created successfully.";
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Displays the form to edit a student class.
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _studentClassHelper.GetByIdAsync(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Student class not found.";
                return RedirectToAction(nameof(Index));
            }

            model.Courses = await _studentClassHelper.GetCoursesSelectListAsync(model.CourseId);
            //Views/EmployeeDashboard/StudentClasses/Edit
            return View(model);
        }


        /// <summary>
        /// Updates an existing student class.
        /// </summary>
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentClassViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Courses = await _studentClassHelper.GetCoursesSelectListAsync(model.CourseId);
                TempData["ErrorMessage"] = "Invalid input. Please correct the form.";
                //Views/EmployeeDashboard/StudentClasses/Edit
                return View(model);
            }

            var entity = await _studentClassRepository.GetByIdAsync(model.Id);
            if (entity == null)
            {
                TempData["ErrorMessage"] = "Student class not found.";
                return RedirectToAction(nameof(Index));
            }

            entity.Name = model.Name;
            entity.AcademicYear = model.AcademicYear;
            entity.Shift = model.Shift;
            entity.CourseId = model.CourseId;

            await _studentClassRepository.UpdateAsync(entity);
            TempData["SuccessMessage"] = "Student class updated successfully.";
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Displays the confirmation page to delete a student class.
        /// </summary>
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _studentClassHelper.GetByIdAsync(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Student class not found.";
                return RedirectToAction(nameof(Index));
            }

            //Views/EmployeeDashboard/StudentClasses/Delete
            return View(model);
        }


        /// <summary>
        /// Deletes a student class after confirmation.
        /// </summary>
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _studentClassRepository.GetByIdWithDetailsAsync(id);

            if (entity == null)
            {
                TempData["ErrorMessage"] = "Student class not found.";
                return RedirectToAction(nameof(Index));
            }

            if (entity.Students != null && entity.Students.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete the class because it has assigned students.";
                return RedirectToAction(nameof(Index));
            }

            await _studentClassRepository.DeleteAsync(entity);
            TempData["SuccessMessage"] = "Student class deleted successfully.";
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Displays the interface to manage students assigned to a class.
        /// </summary>
        [HttpGet("Manage/{id}")]
        public async Task<IActionResult> Manage(int id)
        {
            var studentClass = await _studentClassRepository.GetByIdWithDetailsAsync(id);
            if (studentClass == null)
            {
                TempData["ErrorMessage"] = "Student class not found.";
                return RedirectToAction(nameof(Index));
            }

            var allProfiles = await _studentProfileRepository.GetAll().Include(p => p.User).ToListAsync();

            var assignedStudents = studentClass.Students.Select(s => new StudentAssignmentViewModel
            {
                StudentId = s.User.Id,
                StudentName = s.User.FullName
            }).ToList();

            var availableStudents = allProfiles
                .Where(p => p.StudentClassId == null)
                .Select(p => new StudentAssignmentViewModel
                {
                    StudentId = p.User.Id,
                    StudentName = p.User.FullName
                }).ToList();

            var vm = new ManageStudentClassViewModel
            {
                StudentClassId = studentClass.Id,
                StudentClassName = studentClass.Name,
                AssignedStudents = assignedStudents,
                AvailableStudents = availableStudents
            };

            //Views/EmployeeDashboard/StudentClasses/Manage
            return View(vm);
        }


        /// <summary>
        /// Assigns a student to a class.
        /// </summary>
        [HttpPost("AssignStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStudent([FromBody] AssignStudentRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.StudentId))
                return BadRequest("Invalid request.");

            var profile = await _studentClassRepository.GetStudentProfileByUserIdAsync(request.StudentId);
            if (profile == null)
                return NotFound("Student profile not found.");

            var previousClassId = profile.StudentClassId;
            profile.StudentClassId = request.StudentClassId ?? 0;

            await _studentClassRepository.UpdateStudentProfileAsync(profile);

            // Get class details for alert
            var studentClass = await _studentClassRepository.GetByIdAsync(request.StudentClassId ?? 0);
            if (studentClass != null)
            {
                // Create alert for being added to new class
                await _alertService.CreateAddedToClassAlertAsync(
                    request.StudentId, 
                    request.StudentClassId ?? 0, 
                    studentClass.Name
                );

                // If student was removed from previous class, create alert for that too
                if (previousClassId.HasValue)
                {
                    var previousClass = await _studentClassRepository.GetByIdAsync(previousClassId.Value);
                    if (previousClass != null)
                    {
                        await _alertService.CreateRemovedFromClassAlertAsync(
                            request.StudentId, 
                            previousClass.Name
                        );
                    }
                }
            }

            return Ok(new { message = "Student assigned successfully." });
        }

        /// <summary>
        /// Displays the details of a student class, including course and subject info.
        /// </summary>
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var studentClass = await _studentClassRepository.GetAll()
                .Where(sc => sc.Id == id)
                .Select(sc => new
                {
                    sc.Id,
                    sc.Name,
                    sc.AcademicYear,
                    sc.Shift,
                    Course = sc.Course,
                    Subjects = sc.Course.CourseSubjects.Select(cs => cs.Subject.Name)
                })
                .FirstOrDefaultAsync();

            if (studentClass == null)
            {
                TempData["ErrorMessage"] = "Student class not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new StudentClassViewModel
            {
                Id = studentClass.Id,
                Name = studentClass.Name,
                AcademicYear = studentClass.AcademicYear,
                Shift = studentClass.Shift,
                CourseName = studentClass.Course.Name
            };

            ViewBag.Subjects = studentClass.Subjects.ToList();

            //Views/EmployeeDashboard/StudentClasses/Details
            return View(model);
        }
    }
}
