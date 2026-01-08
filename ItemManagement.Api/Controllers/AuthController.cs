using Dapper;
using ItemManagement.Api.Data;
using ItemManagement.Api.DTOs.Auth;
using ItemManagement.Api.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ItemManagement.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly DbConnectionFactory _db;
        private readonly IConfiguration _config;

        public AuthController(DbConnectionFactory db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            using var connection = _db.CreateConnection();

            // 1) Get user
            var user = connection.QueryFirstOrDefault<UserRow>(@"
                SELECT UserId, Username, PasswordHash, PasswordSalt, IsActive
                FROM Users
                WHERE Username = @Username
            ", new { Username = request.Username });

            if (user is null)
                return Unauthorized("Invalid username or password.");

            if (!user.IsActive)
                return Unauthorized("User is inactive.");

            // 2) Verify password
            var ok = PasswordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!ok)
                return Unauthorized("Invalid username or password.");

            // 3) Get roles
            var roles = connection.Query<string>(@"
                SELECT r.RoleName
                FROM UserRoles ur
                INNER JOIN Roles r ON r.RoleId = ur.RoleId
                WHERE ur.UserId = @UserId
            ", new { UserId = user.UserId }).ToList();

            // 4) Create JWT
            var token = CreateJwtToken(user.UserId, user.Username, roles);

            return Ok(new LoginResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Roles = roles,
                Token = token
            });
        }

        private string CreateJwtToken(int userId, string username, List<string> roles)
        {
            var key = _config["Jwt:Key"]!;
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private class UserRow
        {
            public int UserId { get; set; }
            public string Username { get; set; } = "";
            public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
            public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
            public bool IsActive { get; set; }
        }
    }
}
