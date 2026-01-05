using AssignmentManagementSystem.DTOs;
using AssignmentManagementSystem.Models;
using AssignmentManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace AssignmentManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMailService _mailService;
        private readonly ILogger<AuthController> _logger;
        
        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IMailService mailService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mailService = mailService;
            _logger = logger;
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            // Validate role
            if (model.Role != "Teacher" && model.Role != "Student")
                return BadRequest(new { message = "Role must be either 'Teacher' or 'Student'" });
            
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Age = model.Age,
                Gender = model.Gender,
                Description = model.Description
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });
            
            // Add user to role
            await _userManager.AddToRoleAsync(user, model.Role);
            
            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            
            // Build confirmation link
            var confirmationLink = Url.Action(
                nameof(ConfirmEmail),
                "Auth",
                new { userId = user.Id, token = encodedToken },
                Request.Scheme);
            
            // Send confirmation email
            try
            {
                await _mailService.SendEmailAsync(
                    user.Email,
                    "Confirm Your Email",
                    $"<h2>Welcome to Assignment Management System!</h2>" +
                    $"<p>Please confirm your email by clicking the link below:</p>" +
                    $"<a href='{confirmationLink}'>Confirm Email</a>");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send confirmation email: {ex.Message}");
            }
            
            return Ok(new
            {
                message = "User registered successfully. Please check your email to confirm your account.",
                userId = user.Id
            });
        }
        
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Invalid confirmation link" });
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });
            
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            
            if (!result.Succeeded)
                return BadRequest(new { message = "Email confirmation failed", errors = result.Errors });
            
            return Ok(new { message = "Email confirmed successfully. You can now login." });
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });
            
            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized(new { message = "Please confirm your email before logging in" });
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid email or password" });
            
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user, roles);
            
            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Gender = user.Gender,
                Age = user.Age,
                Description = user.Description,
                Roles = roles
            };
            
            return Ok(new AuthResponseDto
            {
                Token = token,
                User = userProfile
            });
        }
        
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return Ok(new { message = "If the email exists, a password reset link has been sent." });
            }
            
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            
            // Build reset link (you can customize this to point to your frontend)
            var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?userId={user.Id}&token={encodedToken}";
            
            try
            {
                await _mailService.SendEmailAsync(
                    user.Email!,
                    "Reset Your Password",
                    $"<h2>Password Reset Request</h2>" +
                    $"<p>Click the link below to reset your password:</p>" +
                    $"<a href='{resetLink}'>Reset Password</a>" +
                    $"<p>If you didn't request this, please ignore this email.</p>");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send reset email: {ex.Message}");
            }
            
            return Ok(new { message = "If the email exists, a password reset link has been sent." });
        }
        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound(new { message = "User not found" });
            
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
            
            if (!result.Succeeded)
                return BadRequest(new { message = "Password reset failed", errors = result.Errors });
            
            return Ok(new { message = "Password reset successfully. You can now login with your new password." });
        }
        
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });
            
            var roles = await _userManager.GetRolesAsync(user);
            
            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Gender = user.Gender,
                Age = user.Age,
                Description = user.Description,
                Roles = roles
            };
            
            return Ok(userProfile);
        }
    }
}
