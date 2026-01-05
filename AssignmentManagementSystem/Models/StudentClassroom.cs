namespace AssignmentManagementSystem.Models
{
    public class StudentClassroom
    {
        public string StudentId { get; set; } = string.Empty;
        public int ClassroomId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ApplicationUser Student { get; set; } = null!;
        public Classroom Classroom { get; set; } = null!;
    }
}
