using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VetCrm.Domain.Entities;

namespace VetCrm.Api.Services;

public class JwtSettings
{
    public string Key { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpiresMinutes { get; set; } = 720; // 12 saat default
}

public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public TokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public string CreateToken(User user)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_settings.Key);
        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // Kimlik
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        // Username
        if (!string.IsNullOrWhiteSpace(user.Username))
        {
            claims.Add(new(JwtRegisteredClaimNames.UniqueName, user.Username));
            // custom claim
            claims.Add(new("username", user.Username));
        }

        // Görünen isim (FullName varsa onu, yoksa username’i kullan)
        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            claims.Add(new(ClaimTypes.Name, user.FullName));
        }
        else if (!string.IsNullOrWhiteSpace(user.Username))
        {
            claims.Add(new(ClaimTypes.Name, user.Username));
        }

        // Rol
        if (!string.IsNullOrWhiteSpace(user.Role))
        {
            claims.Add(new(ClaimTypes.Role, user.Role));
        }

        var expiresMinutes = _settings.ExpiresMinutes > 0
            ? _settings.ExpiresMinutes
            : 720;

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
