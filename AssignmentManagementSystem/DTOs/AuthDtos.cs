using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty; // "Teacher" or "Student"
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public int Age { get; set; }
        
        [Required]
        public string Gender { get; set; } = string.Empty;
        
        public string? Description { get; set; }
    }
    
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }
    
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
    
    public class ResetPasswordDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Token { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
    
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Description { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
    
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserProfileDto User { get; set; } = null!;
    }
}
