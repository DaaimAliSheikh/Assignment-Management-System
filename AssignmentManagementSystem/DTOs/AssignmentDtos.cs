using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.DTOs
{
    public class CreateAssignmentDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Text { get; set; } = string.Empty;
        
        [Range(0, 1000)]
        public int Marks { get; set; }
    }

    public class UpdateAssignmentDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Text { get; set; } = string.Empty;
        
        [Range(0, 1000)]
        public int Marks { get; set; }
    }

    public class AssignmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Marks { get; set; }
        public int ClassroomId { get; set; }
        public string ClassroomTitle { get; set; } = string.Empty;
        public string CreatedById { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int SubmissionCount { get; set; }
        public bool HasSubmitted { get; set; }
    }
}
