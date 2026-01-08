using Dapper;
using ItemManagement.Api.Data;
using ItemManagement.Api.DTOs.Items;
using ItemManagement.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ItemManagement.Api.Controllers
{
    [ApiController]
    [Route("api/items")]
    public class ItemsController : ControllerBase
    {
        private readonly DbConnectionFactory _db;
        private readonly AuditRepository _audit;

        public ItemsController(DbConnectionFactory db, AuditRepository audit)
        {
            _db = db;
            _audit = audit;
        }

        // ✅ User + Admin
        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            using var conn = _db.CreateConnection();

            var items = conn.Query<ItemDto>(@"
                SELECT ItemId, Name, Description, CreatedByUserId, CreatedAt
                FROM Items
                ORDER BY CreatedAt DESC
            ");

            return Ok(items);
        }

        // ✅ Admin ONLY
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] CreateItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Item name is required.");

            var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            using var conn = _db.CreateConnection();

            var itemId = conn.ExecuteScalar<int>(@"
                INSERT INTO Items (Name, Description, CreatedByUserId)
                VALUES (@Name, @Description, @CreatedByUserId);
                SELECT CAST(SCOPE_IDENTITY() as int);
            ", new
            {
                dto.Name,
                dto.Description,
                CreatedByUserId = adminUserId
            });

            _audit.Write(
                adminUserId,
                "ITEM_CREATED_DIRECT",
                "Item",
                itemId,
                $"{{\"Name\":\"{dto.Name}\"}}"
            );

            return Ok(new { ItemId = itemId });
        }
    }
}
