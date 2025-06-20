using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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

    public async Task AssignSubjectToCourseAsync(int courseId, int subjectId)
    {
        var course = await _context.Courses
            .Include(c => c.CourseSubjects)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null) throw new Exception("Course not found");

        if (!course.CourseSubjects.Any(cs => cs.SubjectId == subjectId))
        {
            course.CourseSubjects.Add(new CourseSubject
            {
                CourseId = courseId,
                SubjectId = subjectId
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveSubjectFromCourseAsync(int courseId, int subjectId)
    {
        var course = await _context.Courses
            .Include(c => c.CourseSubjects)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null) throw new Exception("Course not found");

        var courseSubject = course.CourseSubjects.FirstOrDefault(cs => cs.SubjectId == subjectId);
        if (courseSubject != null)
        {
            course.CourseSubjects.Remove(courseSubject);
            await _context.SaveChangesAsync();
        }
    }
}
