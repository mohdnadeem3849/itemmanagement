using Dapper;
using ItemManagement.Api.Data;
using ItemManagement.Api.DTOs.Admin;
using ItemManagement.Api.Repositories;
using ItemManagement.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ItemManagement.Api.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly DbConnectionFactory _db;
        private readonly AuditRepository _audit;

        public AdminUsersController(DbConnectionFactory db, AuditRepository audit)
        {
            _db = db;
            _audit = audit;
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] CreateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest("Username is required.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Password is required.");

            var role = (dto.Role ?? "User").Trim();
            if (role != "User" && role != "Admin")
                return BadRequest("Role must be 'User' or 'Admin'.");

            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            using var conn = _db.CreateConnection();

            var exists = conn.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Users WHERE Username = @Username",
                new { dto.Username });

            if (exists > 0)
                return BadRequest("Username already exists.");

            PasswordHasher.CreatePasswordHash(dto.Password, out var hash, out var salt);

            var userId = conn.ExecuteScalar<int>(@"
                INSERT INTO Users (Username, PasswordHash, PasswordSalt, IsActive)
                VALUES (@Username, @Hash, @Salt, 1);
                SELECT CAST(SCOPE_IDENTITY() as int);
            ", new { dto.Username, Hash = hash, Salt = salt });

            var roleId = conn.ExecuteScalar<int>(
                "SELECT RoleId FROM Roles WHERE RoleName = @RoleName",
                new { RoleName = role });

            conn.Execute(
                "INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)",
                new { UserId = userId, RoleId = roleId });

            _audit.Write(adminId, "USER_CREATED", "User", userId, $"{{\"Username\":\"{dto.Username}\",\"Role\":\"{role}\"}}");

            return Ok(new { UserId = userId, Username = dto.Username, Role = role });
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            using var conn = _db.CreateConnection();

            var users = conn.Query(@"
                SELECT u.UserId, u.Username, u.IsActive, u.CreatedAt,
                       STRING_AGG(r.RoleName, ',') AS Roles
                FROM Users u
                LEFT JOIN UserRoles ur ON ur.UserId = u.UserId
                LEFT JOIN Roles r ON r.RoleId = ur.RoleId
                GROUP BY u.UserId, u.Username, u.IsActive, u.CreatedAt
                ORDER BY u.UserId;
            ");

            return Ok(users);
        }
    }
}
