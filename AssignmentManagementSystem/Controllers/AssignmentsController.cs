using AssignmentManagementSystem.Data;
using AssignmentManagementSystem.DTOs;
using AssignmentManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssignmentManagementSystem.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class AssignmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AssignmentsController> _logger;

        public AssignmentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<AssignmentsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: api/classrooms/{classId}/assignments
        [HttpGet("classrooms/{classId}/assignments")]
        public async Task<ActionResult<IEnumerable<AssignmentDto>>> GetAssignments(int classId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var classroom = await _context.Classrooms
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classroom == null)
                return NotFound(new { message = "Classroom not found" });

            // Check if user is enrolled (for students) or is the teacher
            var isTeacher = classroom.CreatedById == userId;
            var isEnrolled = await _context.StudentClassrooms
                .AnyAsync(sc => sc.ClassroomId == classId && sc.StudentId == userId);

            if (!isTeacher && !isEnrolled)
                return Forbid();

            var assignments = await _context.Assignments
                .Where(a => a.ClassroomId == classId)
                .Include(a => a.CreatedBy)
                .Include(a => a.Submissions)
                .Select(a => new AssignmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Text = a.Text,
                    Marks = a.Marks,
                    ClassroomId = a.ClassroomId,
                    ClassroomTitle = classroom.Title,
                    CreatedById = a.CreatedById,
                    CreatedByName = a.CreatedBy.FullName,
                    CreatedAt = a.CreatedAt,
                    SubmissionCount = a.Submissions.Count,
                    HasSubmitted = a.Submissions.Any(s => s.StudentId == userId)
                })
                .ToListAsync();

            return Ok(assignments);
        }

        // GET: api/assignments/{id}
        [HttpGet("assignments/{id}")]
        public async Task<ActionResult<AssignmentDto>> GetAssignment(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                    .ThenInclude(c => c.CreatedBy)
                .Include(a => a.CreatedBy)
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
                return NotFound(new { message = "Assignment not found" });

            // Check if user has access
            var isTeacher = assignment.Classroom.CreatedById == userId;
            var isEnrolled = await _context.StudentClassrooms
                .AnyAsync(sc => sc.ClassroomId == assignment.ClassroomId && sc.StudentId == userId);

            if (!isTeacher && !isEnrolled)
                return Forbid();

            var assignmentDto = new AssignmentDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Text = assignment.Text,
                Marks = assignment.Marks,
                ClassroomId = assignment.ClassroomId,
                ClassroomTitle = assignment.Classroom.Title,
                CreatedById = assignment.CreatedById,
                CreatedByName = assignment.CreatedBy.FullName,
                CreatedAt = assignment.CreatedAt,
                SubmissionCount = assignment.Submissions.Count,
                HasSubmitted = assignment.Submissions.Any(s => s.StudentId == userId)
            };

            return Ok(assignmentDto);
        }

        // POST: api/classrooms/{classId}/assignments (Teacher only)
        [HttpPost("classrooms/{classId}/assignments")]
        [Authorize(Policy = "RequireTeacher")]
        public async Task<ActionResult<AssignmentDto>> CreateAssignment(int classId, [FromBody] CreateAssignmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var classroom = await _context.Classrooms
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classroom == null)
                return NotFound(new { message = "Classroom not found" });

            // Only the teacher who created the classroom can add assignments
            if (classroom.CreatedById != userId)
                return Forbid();

            var assignment = new Assignment
            {
                Title = dto.Title,
                Text = dto.Text,
                Marks = dto.Marks,
                ClassroomId = classId,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            var assignmentDto = new AssignmentDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Text = assignment.Text,
                Marks = assignment.Marks,
                ClassroomId = assignment.ClassroomId,
                ClassroomTitle = classroom.Title,
                CreatedById = assignment.CreatedById,
                CreatedByName = classroom.CreatedBy.FullName,
                CreatedAt = assignment.CreatedAt,
                SubmissionCount = 0,
                HasSubmitted = false
            };

            return CreatedAtAction(nameof(GetAssignment), new { id = assignment.Id }, assignmentDto);
        }

        // PUT: api/assignments/{id} (Teacher only - owner)
        [HttpPut("assignments/{id}")]
        [Authorize(Policy = "RequireTeacher")]
        public async Task<IActionResult> UpdateAssignment(int id, [FromBody] UpdateAssignmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
                return NotFound(new { message = "Assignment not found" });

            // Only the teacher who created the assignment can update it
            if (assignment.CreatedById != userId)
                return Forbid();

            assignment.Title = dto.Title;
            assignment.Text = dto.Text;
            assignment.Marks = dto.Marks;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Assignment updated successfully" });
        }

        // DELETE: api/assignments/{id} (Teacher only - owner)
        [HttpDelete("assignments/{id}")]
        [Authorize(Policy = "RequireTeacher")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
                return NotFound(new { message = "Assignment not found" });

            // Only the teacher who created the assignment can delete it
            if (assignment.CreatedById != userId)
                return Forbid();

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Assignment deleted successfully" });
        }
    }
}
