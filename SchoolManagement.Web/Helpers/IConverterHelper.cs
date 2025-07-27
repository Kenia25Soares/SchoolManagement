using SchoolManagement.Web.Data.Entities;
using SchoolManagement.Web.Models;

namespace SchoolManagement.Web.Helpers
{
    public interface IConverterHelper
    {
        CourseViewModel ToCourseViewModel(Course course);  //Converte o objeto Course para CourseViewModel
        Course ToCourseEntity(CourseViewModel model, bool isNew); // Se não for novo, retorna o objeto Course com os dados do model

        SubjectViewModel ToSubjectViewModel(Subject subject);
        Subject ToSubjectEntity(SubjectViewModel model, bool isNew);
    }
}
