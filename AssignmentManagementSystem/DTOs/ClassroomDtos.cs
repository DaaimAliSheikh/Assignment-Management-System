using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.DTOs
{
    public class CreateClassroomDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class ClassroomDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatedById { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int AssignmentCount { get; set; }
        public int StudentCount { get; set; }
    }

    public class ClassroomDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatedById { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<AssignmentDto> Assignments { get; set; } = new();
        public int StudentCount { get; set; }
        public bool IsEnrolled { get; set; }
    }
}
