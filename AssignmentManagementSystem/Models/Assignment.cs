namespace AssignmentManagementSystem.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Marks { get; set; }
        public int ClassroomId { get; set; }
        public string CreatedById { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Classroom Classroom { get; set; } = null!;
        public ApplicationUser CreatedBy { get; set; } = null!;
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
