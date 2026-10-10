using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DocumentSearch.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool TryGetEmployeeId(this ClaimsPrincipal user, out int employeeId)
        {
            string? value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(value))
            {
                value = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            }

            return int.TryParse(value, out employeeId);
        }
    }
}
