using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using VetCrm.Infrastructure.Data;
using VetCrm.Api.Services;
using VetCrm.Domain.Entities;
using BC = BCrypt.Net.BCrypt;

namespace VetCrm.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]  
    public class AuthController : ControllerBase
    {
        private readonly VetCrmDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthController(VetCrmDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        public class LoginRequestDto
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        public class LoginResponseDto
        {
            public int Id { get; set; }
            public string FullName { get; set; } = null!;
            public string Username { get; set; } = null!;
            public string Role { get; set; } = null!;
            public string Token { get; set; } = null!;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Kullanıcı adı ve şifre zorunludur.");

            var normalizedUsername = dto.Username.Trim().ToLowerInvariant();

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername);

            if (user == null || !BC.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");


            var token = _tokenService.CreateToken(user);

            return Ok(new LoginResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Role = user.Role,
                Token = token
            });
        }

        public class ClientLoginRequestDto
        {
            public string Phone { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        public class ClientLoginResponseDto
        {
            public int Id { get; set; }
            public string FullName { get; set; } = null!;
            public string Phone { get; set; } = null!;
            public string Role { get; set; } = "Client";
            public string Token { get; set; } = null!;
        }

        [HttpPost("client-login")]
        public async Task<ActionResult<ClientLoginResponseDto>> ClientLogin([FromBody] ClientLoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Phone) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Telefon numarası ve şifre zorunludur.");

            var cleanedPhone = new string(dto.Phone.Where(char.IsDigit).ToArray());
            if (cleanedPhone.Length == 10 && cleanedPhone.StartsWith("5"))
            {
                cleanedPhone = "90" + cleanedPhone;
            }

            var owner = await _db.Owners
                .FirstOrDefaultAsync(o => o.PhoneE164 == cleanedPhone);

            if (owner == null || string.IsNullOrWhiteSpace(owner.PasswordHash) || !BC.Verify(dto.Password, owner.PasswordHash))
                return Unauthorized("Telefon numarası veya şifre hatalı.");

            var token = _tokenService.CreateToken(owner);

            return Ok(new ClientLoginResponseDto
            {
                Id = owner.Id,
                FullName = owner.FullName,
                Phone = owner.PhoneE164,
                Token = token
            });
        }
    }
}
