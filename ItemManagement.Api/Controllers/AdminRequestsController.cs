using Dapper;
using ItemManagement.Api.Data;
using ItemManagement.Api.DTOs.Admin;
using ItemManagement.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ItemManagement.Api.Controllers
{
    [ApiController]
    [Route("api/admin/requests")]
    [Authorize(Roles = "Admin")]
    public class AdminRequestsController : ControllerBase
    {
        private readonly DbConnectionFactory _db;
        private readonly AuditRepository _audit;

        public AdminRequestsController(DbConnectionFactory db, AuditRepository audit)
        {
            _db = db;
            _audit = audit;
        }

        // ✅ Admin: view all requests
        [HttpGet]
        public IActionResult GetAll()
        {
            using var conn = _db.CreateConnection();

            var requests = conn.Query(@"
                SELECT RequestId, RequestedName, RequestedDescription, Status, RejectionReason,
                       RequestedByUserId, DecisionByAdminUserId, DecidedAt, CreatedAt
                FROM ItemRequests
                ORDER BY CreatedAt DESC
            ");

            return Ok(requests);
        }

        // ✅ Admin: deny request (reason required)
        [HttpPost("{id}/deny")]
        public IActionResult Deny(int id, [FromBody] DenyRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("Denial reason is required.");

            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            using var conn = _db.CreateConnection();

            var updated = conn.Execute(@"
                UPDATE ItemRequests
                SET Status = 'Denied',
                    RejectionReason = @Reason,
                    DecisionByAdminUserId = @AdminId,
                    DecidedAt = SYSUTCDATETIME()
                WHERE RequestId = @Id
            ", new { Id = id, Reason = dto.Reason, AdminId = adminId });

            if (updated == 0)
                return NotFound("Request not found.");

            _audit.Write(adminId, "REQUEST_DENIED", "ItemRequest", id, $"{{\"Reason\":\"{dto.Reason}\"}}");

            return Ok();
        }

        // ✅ Admin: approve request (creates item + marks request approved)
        [HttpPost("{id}/approve")]
        public IActionResult Approve(int id)
        {
            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            using var conn = _db.CreateConnection();

            var req = conn.QueryFirstOrDefault<dynamic>(@"
                SELECT RequestId, RequestedName, RequestedDescription, Status
                FROM ItemRequests
                WHERE RequestId = @Id
            ", new { Id = id });

            if (req is null)
                return NotFound("Request not found.");

            if (req.Status != "Pending" && req.Status != "Denied")
                return BadRequest("Only Pending/Denied requests can be approved.");

            var itemId = conn.ExecuteScalar<int>(@"
                INSERT INTO Items (Name, Description, CreatedByUserId)
                VALUES (@Name, @Desc, @AdminId);
                SELECT CAST(SCOPE_IDENTITY() as int);
            ", new
            {
                Name = (string)req.RequestedName,
                Desc = (string?)req.RequestedDescription,
                AdminId = adminId
            });

            conn.Execute(@"
                UPDATE ItemRequests
                SET Status = 'Approved',
                    RejectionReason = NULL,
                    DecisionByAdminUserId = @AdminId,
                    DecidedAt = SYSUTCDATETIME()
                WHERE RequestId = @Id
            ", new { Id = id, AdminId = adminId });

            _audit.Write(adminId, "REQUEST_APPROVED", "ItemRequest", id, null);
            _audit.Write(adminId, "ITEM_CREATED_FROM_REQUEST", "Item", itemId, $"{{\"RequestId\":{id}}}");

            return Ok(new { ItemId = itemId });
        }
    }
}
