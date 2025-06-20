using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

public class CourseHelper : ICourseHelper
{
    private readonly DataContext _context;

    public CourseHelper(DataContext context)
    {
        _context = context;
    }

    public async Task<CourseManagementViewModel> GetCourseManagementAsync(int courseId)
    {
        var course = await _context.Courses
            .Include(c => c.CourseSubjects).ThenInclude(cs => cs.Subject)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null) return null;

        var allSubjects = await _context.Subjects.ToListAsync();
        var assignedSubjectIds = course.CourseSubjects.Select(cs => cs.SubjectId).ToList();

        return new CourseManagementViewModel
        {
            CourseId = course.Id,
            CourseName = course.Name,

            AvailableSubjects = allSubjects
                .Where(s => !assignedSubjectIds.Contains(s.Id))
                .Select(s => new SubjectAssignmentViewModel
                {
                    SubjectId = s.Id,
                    SubjectName = s.Name
                }).ToList(),

            AssignedSubjects = course.CourseSubjects
                .Select(cs => new SubjectAssignmentViewModel
                {
                    SubjectId = cs.Subject.Id,
                    SubjectName = cs.Subject.Name
                }).ToList()
        };
    }

    public async Task UpdateCourseAssignmentsAsync(CourseManagementViewModel model)
    {
        var course = await _context.Courses
            .Include(c => c.CourseSubjects)
            .FirstOrDefaultAsync(c => c.Id == model.CourseId);

        if (course == null) return;

        _context.CourseSubjects.RemoveRange(course.CourseSubjects);

        var subjectIds = (model.AssignedSubjectsHidden ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id, out var sid) ? sid : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .ToList();

        foreach (var subjectId in subjectIds)
        {
            _context.CourseSubjects.Add(new CourseSubject
            {
                CourseId = course.Id,
                SubjectId = subjectId
            });
        }

        await _context.SaveChangesAsync();
    }
}
