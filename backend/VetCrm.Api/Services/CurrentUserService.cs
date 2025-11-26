using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace VetCrm.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var sub = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (int.TryParse(sub, out var id))
                return id;
            return null;
        }
    }

    public string? Username =>
        User?.FindFirst("username")?.Value ??
        User?.FindFirstValue(JwtRegisteredClaimNames.UniqueName);

    public string? Role => User?.FindFirstValue(ClaimTypes.Role);

    public string? FullName => User?.FindFirstValue(ClaimTypes.Name);

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
}
