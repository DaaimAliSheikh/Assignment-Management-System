using AssignmentManagementSystem.Data;
using AssignmentManagementSystem.DTOs;
using AssignmentManagementSystem.Models;
using AssignmentManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssignmentManagementSystem.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class SubmissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<SubmissionsController> _logger;

        public SubmissionsController(
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService,
            ILogger<SubmissionsController> logger)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        // POST: api/assignments/{assignmentId}/submission (Student only)
        [HttpPost("assignments/{assignmentId}/submission")]
        [Authorize(Policy = "RequireStudent")]
        public async Task<ActionResult<SubmissionDto>> CreateSubmission(int assignmentId, [FromForm] CreateSubmissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null)
                return NotFound(new { message = "Assignment not found" });

            // Check if student is enrolled in the classroom
            var isEnrolled = await _context.StudentClassrooms
                .AnyAsync(sc => sc.ClassroomId == assignment.ClassroomId && sc.StudentId == userId);

            if (!isEnrolled)
                return Forbid();

            // Check if already submitted
            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == userId);

            if (existingSubmission != null)
                return Conflict(new { message = "You have already submitted this assignment" });

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(new { message = "File is required" });

            string fileUrl;
            try
            {
                fileUrl = await _cloudinaryService.UploadFileAsync(dto.File);
            }
            catch (Exception ex)
            {
                _logger.LogError($"File upload failed: {ex.Message}");
                return BadRequest(new { message = $"File upload failed: {ex.Message}" });
            }

            var submission = new Submission
            {
                AssignmentId = assignmentId,
                StudentId = userId,
                FileUrl = fileUrl,
                SubmittedAt = DateTime.UtcNow
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            var student = await _context.Users.FindAsync(userId);

            var submissionDto = new SubmissionDto
            {
                Id = submission.Id,
                AssignmentId = submission.AssignmentId,
                AssignmentTitle = assignment.Title,
                StudentId = submission.StudentId,
                StudentName = student?.FullName ?? "",
                StudentEmail = student?.Email ?? "",
                FileUrl = submission.FileUrl,
                SubmittedAt = submission.SubmittedAt
            };

            return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, submissionDto);
        }

        // GET: api/submissions/{id}
        [HttpGet("submissions/{id}")]
        public async Task<ActionResult<SubmissionDto>> GetSubmission(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Classroom)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
                return NotFound(new { message = "Submission not found" });

            // Check access: teacher of classroom or the student who submitted
            var isTeacher = submission.Assignment.Classroom.CreatedById == userId;
            var isOwner = submission.StudentId == userId;

            if (!isTeacher && !isOwner)
                return Forbid();

            var submissionDto = new SubmissionDto
            {
                Id = submission.Id,
                AssignmentId = submission.AssignmentId,
                AssignmentTitle = submission.Assignment.Title,
                StudentId = submission.StudentId,
                StudentName = submission.Student.FullName,
                StudentEmail = submission.Student.Email!,
                FileUrl = submission.FileUrl,
                SubmittedAt = submission.SubmittedAt
            };

            return Ok(submissionDto);
        }

        // GET: api/assignments/{assignmentId}/submission (Student - get own submission)
        [HttpGet("assignments/{assignmentId}/submission")]
        [Authorize(Policy = "RequireStudent")]
        public async Task<ActionResult<SubmissionDto>> GetMySubmission(int assignmentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == userId);

            if (submission == null)
                return NotFound(new { message = "No submission found for this assignment" });

            var submissionDto = new SubmissionDto
            {
                Id = submission.Id,
                AssignmentId = submission.AssignmentId,
                AssignmentTitle = submission.Assignment.Title,
                StudentId = submission.StudentId,
                StudentName = submission.Student.FullName,
                StudentEmail = submission.Student.Email!,
                FileUrl = submission.FileUrl,
                SubmittedAt = submission.SubmittedAt
            };

            return Ok(submissionDto);
        }

        // GET: api/classrooms/{classId}/submissions (Teacher only)
        [HttpGet("classrooms/{classId}/submissions")]
        [Authorize(Policy = "RequireTeacher")]
        public async Task<ActionResult<IEnumerable<SubmissionDto>>> GetClassroomSubmissions(int classId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classroom == null)
                return NotFound(new { message = "Classroom not found" });

            // Only the teacher who created the classroom can view submissions
            if (classroom.CreatedById != userId)
                return Forbid();

            var submissions = await _context.Submissions
                .Where(s => s.Assignment.ClassroomId == classId)
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                .Select(s => new SubmissionDto
                {
                    Id = s.Id,
                    AssignmentId = s.AssignmentId,
                    AssignmentTitle = s.Assignment.Title,
                    StudentId = s.StudentId,
                    StudentName = s.Student.FullName,
                    StudentEmail = s.Student.Email!,
                    FileUrl = s.FileUrl,
                    SubmittedAt = s.SubmittedAt
                })
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return Ok(submissions);
        }

        // GET: api/assignments/{assignmentId}/submissions (Teacher only)
        [HttpGet("assignments/{assignmentId}/submissions")]
        [Authorize(Policy = "RequireTeacher")]
        public async Task<ActionResult<IEnumerable<SubmissionDto>>> GetAssignmentSubmissions(int assignmentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null)
                return NotFound(new { message = "Assignment not found" });

            // Only the teacher who created the classroom can view submissions
            if (assignment.Classroom.CreatedById != userId)
                return Forbid();

            var submissions = await _context.Submissions
                .Where(s => s.AssignmentId == assignmentId)
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                .Select(s => new SubmissionDto
                {
                    Id = s.Id,
                    AssignmentId = s.AssignmentId,
                    AssignmentTitle = s.Assignment.Title,
                    StudentId = s.StudentId,
                    StudentName = s.Student.FullName,
                    StudentEmail = s.Student.Email!,
                    FileUrl = s.FileUrl,
                    SubmittedAt = s.SubmittedAt
                })
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return Ok(submissions);
        }
    }
}
