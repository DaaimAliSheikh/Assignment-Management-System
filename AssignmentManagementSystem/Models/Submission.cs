namespace AssignmentManagementSystem.Models
{
    public class Submission
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Assignment Assignment { get; set; } = null!;
        public ApplicationUser Student { get; set; } = null!;
    }
}
