using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Repositories;
using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Data.Repositories;
using SchoolManagement.Web.Helpers;
using SchoolManagement.Web.Models;

public class CourseHelper : ICourseHelper
{
    private readonly ICourseRepository _courseRepository;
    private readonly ISubjectRepository _subjectRepository;

    public CourseHelper(ICourseRepository courseRepository, ISubjectRepository subjectRepository)
    {
        _courseRepository = courseRepository;
        _subjectRepository = subjectRepository;
    }

    public async Task<CourseManagementViewModel?> GetCourseManagementAsync(int courseId)
    {
        var course = await _courseRepository.GetByIdWithAllRelationsAsync(courseId);
        if (course == null) return null;

        var allSubjects = await _subjectRepository
            .GetAll().OrderBy(s => s.Name)
            .ToListAsync();

        var assignedSubjectIds = course.CourseSubjects.Select(cs => cs.SubjectId)
            .ToList();

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
        var course = await _courseRepository.GetByIdWithAllRelationsAsync(courseId);
        if (course == null)
        { 
            throw new InvalidOperationException("Course not found");
        }

        if (!course.CourseSubjects.Any(cs => cs.SubjectId == subjectId))
        {
            course.CourseSubjects.Add(new CourseSubject
            {
                CourseId = courseId,
                SubjectId = subjectId
            });
            await _courseRepository.UpdateAsync(course); 
        }
    }

    public async Task RemoveSubjectFromCourseAsync(int courseId, int subjectId)
    {
        var course = await _courseRepository.GetByIdWithAllRelationsAsync(courseId);
        if (course == null)
        {
            throw new InvalidOperationException("Course not found");
        }

        var courseSubject = course.CourseSubjects.FirstOrDefault(cs => cs.SubjectId == subjectId);
        if (courseSubject != null)
        {
            course.CourseSubjects.Remove(courseSubject);
            await _courseRepository.UpdateAsync(course); 
        }
    }

    public async Task<IEnumerable<SelectListItem>> GetCoursesSelectListAsync(int? selectedCourseId = null)
    {
        var courses = await _courseRepository.GetAll().OrderBy(c => c.Name).ToListAsync();

        return courses.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name,
            Selected = selectedCourseId.HasValue && selectedCourseId.Value == c.Id
        }).ToList();
    }
}
