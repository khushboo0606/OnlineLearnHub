using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineLearnHub.Models;

namespace OnlineLearnHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        

        // Configure Enrollment entity
            modelBuilder.Entity<Enrollment>(entity =>
        {
            // Set composite key
            entity.HasKey(e => new { e.StudentId, e.CourseId });

            // Configure relationships
            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

            // Configure Course entity
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany() // Assuming a User can have many Courses
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}