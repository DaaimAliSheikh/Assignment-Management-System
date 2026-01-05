namespace AssignmentManagementSystem.Models
{
    public class Classroom
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatedById { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ApplicationUser CreatedBy { get; set; } = null!;
        public ICollection<StudentClassroom> StudentClassrooms { get; set; } = new List<StudentClassroom>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
