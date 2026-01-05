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
    [Route("api/[controller]")]
    public class ClassroomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ClassroomsController> _logger;

        public ClassroomsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ClassroomsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: api/classrooms (Public - list all classrooms)
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ClassroomDto>>> GetClassrooms()
        {
            var classrooms = await _context.Classrooms
                .Include(c => c.CreatedBy)
                .Include(c => c.StudentClassrooms)
                .Include(c => c.Assignments)
                .Select(c => new ClassroomDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    CreatedById = c.CreatedById,
                    CreatedByName = c.CreatedBy.FullName,
                    CreatedAt = c.CreatedAt,
                    AssignmentCount = c.Assignments.Count,
                    StudentCount = c.StudentClassrooms.Count
                })
                .ToListAsync();

            return Ok(classrooms);
        }

        // GET: api/classrooms/{id} (Authenticated - get classroom details)
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ClassroomDetailDto>> GetClassroom(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var classroom = await _context.Classrooms
                .Include(c => c.CreatedBy)
                .Include(c => c.StudentClassrooms)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Submissions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom == null)
                return NotFound(new { message = "Classroom not found" });

            var isEnrolled = await _context.StudentClassrooms
                .AnyAsync(sc => sc.ClassroomId == id && sc.StudentId == userId);

            var classroomDto = new ClassroomDetailDto
            {
                Id = classroom.Id,
                Title = classroom.Title,
                Description = classroom.Description,
                CreatedById = classroom.CreatedById,
                CreatedByName = classroom.CreatedBy.FullName,
                CreatedAt = classroom.CreatedAt,
                StudentCount = classroom.StudentClassrooms.Count,
                IsEnrolled = isEnrolled,
                Assignments = classroom.Assignments.Select(a => new AssignmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Text = a.Text,
                    Marks = a.Marks,
                    ClassroomId = a.ClassroomId,
                    ClassroomTitle = classroom.Title,
                    CreatedById = a.CreatedById,
                    CreatedByName = classroom.CreatedBy.FullName,
                    CreatedAt = a.CreatedAt,
                    SubmissionCount = a.Submissions.Count,
                    HasSubmitted = a.Submissions.Any(s => s.StudentId == userId)
                }).ToList()
            };

            return Ok(classroomDto);
        }

        // POST: api/classrooms (Teacher only - create classroom)
        [HttpPost]
        [Authorize(Policy = "RequireTeacher")]
        public async Task<ActionResult<ClassroomDto>> CreateClassroom([FromBody] CreateClassroomDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var classroom = new Classroom
            {
                Title = dto.Title,
                Description = dto.Description ?? string.Empty,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);

            var classroomDto = new ClassroomDto
            {
                Id = classroom.Id,
                Title = classroom.Title,
                Description = classroom.Description,
                CreatedById = classroom.CreatedById,
                CreatedByName = user?.FullName ?? "",
                CreatedAt = classroom.CreatedAt,
                AssignmentCount = 0,
                StudentCount = 0
            };

            return CreatedAtAction(nameof(GetClassroom), new { id = classroom.Id }, classroomDto);
        }

        // POST: api/classrooms/{id}/join (Student only - join classroom)
        [HttpPost("{id}/join")]
        [Authorize(Policy = "RequireStudent")]
        public async Task<IActionResult> JoinClassroom(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null)
                return NotFound(new { message = "Classroom not found" });

            // Check if already enrolled
            var existingEnrollment = await _context.StudentClassrooms
                .FirstOrDefaultAsync(sc => sc.StudentId == userId && sc.ClassroomId == id);

            if (existingEnrollment != null)
                return BadRequest(new { message = "You are already enrolled in this classroom" });

            var studentClassroom = new StudentClassroom
            {
                StudentId = userId,
                ClassroomId = id,
                JoinedAt = DateTime.UtcNow
            };

            _context.StudentClassrooms.Add(studentClassroom);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully joined the classroom" });
        }

        // GET: api/classrooms/my-classrooms (Get user's classrooms)
        [HttpGet("my-classrooms")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ClassroomDto>>> GetMyClassrooms()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            IQueryable<Classroom> query;

            if (roles.Contains("Teacher"))
            {
                // Teachers see classrooms they created
                query = _context.Classrooms
                    .Where(c => c.CreatedById == userId);
            }
            else
            {
                // Students see classrooms they joined
                query = _context.Classrooms
                    .Where(c => c.StudentClassrooms.Any(sc => sc.StudentId == userId));
            }

            var classrooms = await query
                .Include(c => c.CreatedBy)
                .Include(c => c.StudentClassrooms)
                .Include(c => c.Assignments)
                .Select(c => new ClassroomDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    CreatedById = c.CreatedById,
                    CreatedByName = c.CreatedBy.FullName,
                    CreatedAt = c.CreatedAt,
                    AssignmentCount = c.Assignments.Count,
                    StudentCount = c.StudentClassrooms.Count
                })
                .ToListAsync();

            return Ok(classrooms);
        }
    }
}
