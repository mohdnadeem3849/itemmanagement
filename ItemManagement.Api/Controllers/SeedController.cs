using Dapper;
using Microsoft.AspNetCore.Mvc;
using ItemManagement.Api.Data;
using ItemManagement.Api.Security;

namespace ItemManagement.Api.Controllers
{
    [ApiController]
    [Route("api/seed")]
    public class SeedController : ControllerBase
    {
        private readonly DbConnectionFactory _db;

        public SeedController(DbConnectionFactory db)
        {
            _db = db;
        }

        [HttpPost("admin")]
        public IActionResult CreateAdmin()
        {
            using var connection = _db.CreateConnection();

            const string adminUsername = "admin";
            const string adminPassword = "Admin@123";

            var existingUser = connection.QueryFirstOrDefault<int>(
                "SELECT COUNT(1) FROM Users WHERE Username = @Username",
                new { Username = adminUsername });

            if (existingUser > 0)
                return BadRequest("Admin already exists");

            PasswordHasher.CreatePasswordHash(
                adminPassword,
                out var hash,
                out var salt);

            var userId = connection.ExecuteScalar<int>(@"
                INSERT INTO Users (Username, PasswordHash, PasswordSalt)
                VALUES (@Username, @Hash, @Salt);
                SELECT CAST(SCOPE_IDENTITY() as int);
            ", new { Username = adminUsername, Hash = hash, Salt = salt });

            var adminRoleId = connection.ExecuteScalar<int>(
                "SELECT RoleId FROM Roles WHERE RoleName = 'Admin'");

            connection.Execute(
                "INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)",
                new { UserId = userId, RoleId = adminRoleId });

            return Ok("Admin user created. Username=admin Password=Admin@123");
        }
    }
}
