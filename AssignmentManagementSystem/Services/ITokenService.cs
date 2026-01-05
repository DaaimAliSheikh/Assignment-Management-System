using AssignmentManagementSystem.Models;

namespace AssignmentManagementSystem.Services
{
    public interface ITokenService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
