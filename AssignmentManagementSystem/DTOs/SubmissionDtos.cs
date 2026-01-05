namespace AssignmentManagementSystem.DTOs
{
    public class SubmissionDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }

    public class CreateSubmissionDto
    {
        public IFormFile File { get; set; } = null!;
    }
}
