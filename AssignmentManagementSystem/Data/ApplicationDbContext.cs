using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AssignmentManagementSystem.Models;

namespace AssignmentManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<StudentClassroom> StudentClassrooms { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure StudentClassroom (many-to-many)
            builder.Entity<StudentClassroom>()
                .HasKey(sc => new { sc.StudentId, sc.ClassroomId });
            
            builder.Entity<StudentClassroom>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.StudentClassrooms)
                .HasForeignKey(sc => sc.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<StudentClassroom>()
                .HasOne(sc => sc.Classroom)
                .WithMany(c => c.StudentClassrooms)
                .HasForeignKey(sc => sc.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure Classroom
            builder.Entity<Classroom>()
                .HasOne(c => c.CreatedBy)
                .WithMany(u => u.CreatedClassrooms)
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure Assignment
            builder.Entity<Assignment>()
                .HasOne(a => a.Classroom)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Assignment>()
                .HasOne(a => a.CreatedBy)
                .WithMany(u => u.CreatedAssignments)
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure Submission - Unique constraint (one submission per student per assignment)
            builder.Entity<Submission>()
                .HasIndex(s => new { s.AssignmentId, s.StudentId })
                .IsUnique();
            
            builder.Entity<Submission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Submission>()
                .HasOne(s => s.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
