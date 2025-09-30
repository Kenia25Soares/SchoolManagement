using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Web.Data.Entities;

namespace SchoolManagement.Web.Data
{
    public class DataContext : IdentityDbContext<ApplicationUser>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<CourseSubject> CourseSubjects { get; set; }
        public DbSet<StudentGrade> StudentGrades { get; set; }
        public DbSet<Alert> Alerts { get; set; }

        public DbSet<GradeType> GradeTypes { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<SubjectEnrollmentRequest> SubjectEnrollmentRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Discriminador p diferentes tipos de utilizador
            builder.Entity<ApplicationUser>()
                .HasDiscriminator<string>("UserType")
                .HasValue<ApplicationUser>("ApplicationUser");

            // Chave composta para CourseSubject
            builder.Entity<CourseSubject>()
                .HasKey(cs => new { cs.CourseId, cs.SubjectId });

            // Relacionamento CourseSubject - Course
            builder.Entity<CourseSubject>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.CourseSubjects)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento CourseSubject - Subject
            builder.Entity<CourseSubject>()
                .HasOne(cs => cs.Subject)
                .WithMany(s => s.CourseSubjects)
                .HasForeignKey(cs => cs.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentGrade - Course
            builder.Entity<StudentGrade>()
                .HasOne(sg => sg.Course)
                .WithMany()
                .HasForeignKey(sg => sg.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubjectEnrollmentRequest relationships
            builder.Entity<SubjectEnrollmentRequest>()
                .HasOne(ser => ser.Student)
                .WithMany()
                .HasForeignKey(ser => ser.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectEnrollmentRequest>()
                .HasOne(ser => ser.Subject)
                .WithMany()
                .HasForeignKey(ser => ser.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectEnrollmentRequest>()
                .HasOne(ser => ser.ProcessedBy)
                .WithMany()
                .HasForeignKey(ser => ser.ProcessedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Alert relationships
            builder.Entity<Alert>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Alert>()
                .HasOne(a => a.Subject)
                .WithMany()
                .HasForeignKey(a => a.SubjectId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Alert>()
                .HasOne(a => a.StudentClass)
                .WithMany()
                .HasForeignKey(a => a.StudentClassId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Alert>()
                .HasOne(a => a.StudentGrade)
                .WithMany()
                .HasForeignKey(a => a.StudentGradeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Alert>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Impede delete em cascata 
            foreach (var relationship in builder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

        }
    }
}
