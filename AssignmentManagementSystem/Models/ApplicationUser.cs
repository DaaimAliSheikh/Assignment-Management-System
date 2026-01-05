using Microsoft.AspNetCore.Identity;

namespace AssignmentManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        
        // Navigation properties
        public ICollection<Classroom> CreatedClassrooms { get; set; } = new List<Classroom>();
        public ICollection<StudentClassroom> StudentClassrooms { get; set; } = new List<StudentClassroom>();
        public ICollection<Assignment> CreatedAssignments { get; set; } = new List<Assignment>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
