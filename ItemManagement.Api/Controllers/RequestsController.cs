using Dapper;
using ItemManagement.Api.Data;
using ItemManagement.Api.DTOs.Requests;
using ItemManagement.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ItemManagement.Api.Controllers
{
    [ApiController]
    [Route("api/requests")]
    public class RequestsController : ControllerBase
    {
        private readonly DbConnectionFactory _db;
        private readonly AuditRepository _audit;

        public RequestsController(DbConnectionFactory db, AuditRepository audit)
        {
            _db = db;
            _audit = audit;
        }

        // ✅ USER: Create a request (Pending)
        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult Create([FromBody] CreateRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RequestedName))
                return BadRequest("RequestedName is required.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            using var conn = _db.CreateConnection();

            var requestId = conn.ExecuteScalar<int>(@"
                INSERT INTO ItemRequests (RequestedName, RequestedDescription, RequestedByUserId)
                VALUES (@Name, @Desc, @UserId);
                SELECT CAST(SCOPE_IDENTITY() as int);
            ", new
            {
                Name = dto.RequestedName,
                Desc = dto.RequestedDescription,
                UserId = userId
            });

            _audit.Write(userId, "REQUEST_CREATED", "ItemRequest", requestId, null);

            return Ok(new { RequestId = requestId });
        }

        // ✅ USER: View own requests
        [HttpGet("my")]
        [Authorize(Roles = "User")]
        public IActionResult My()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            using var conn = _db.CreateConnection();

            var requests = conn.Query<RequestDto>(@"
                SELECT RequestId, RequestedName, RequestedDescription, Status, RejectionReason,
                       RequestedByUserId, DecisionByAdminUserId, DecidedAt, CreatedAt
                FROM ItemRequests
                WHERE RequestedByUserId = @UserId
                ORDER BY CreatedAt DESC
            ", new { UserId = userId });

            return Ok(requests);
        }

        // ✅ USER: Appeal denied request
        [HttpPost("{id}/appeal")]
        [Authorize(Roles = "User")]
        public IActionResult Appeal(int id, [FromBody] CreateAppealDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.AppealMessage))
                return BadRequest("AppealMessage is required.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            using var conn = _db.CreateConnection();

            var req = conn.QueryFirstOrDefault<RequestDto>(@"
                SELECT RequestId, Status, RequestedByUserId
                FROM ItemRequests
                WHERE RequestId = @Id
            ", new { Id = id });

            if (req is null)
                return NotFound("Request not found.");

            if (req.RequestedByUserId != userId)
                return Forbid();

            if (req.Status != "Denied")
                return BadRequest("Only denied requests can be appealed.");

            var appealId = conn.ExecuteScalar<int>(@"
                INSERT INTO ItemRequestAppeals (RequestId, AppealMessage, CreatedByUserId)
                VALUES (@Id, @Msg, @UserId);
                SELECT CAST(SCOPE_IDENTITY() as int);
            ", new { Id = id, Msg = dto.AppealMessage, UserId = userId });

            _audit.Write(userId, "APPEAL_SUBMITTED", "ItemRequest", id, $"{{\"AppealId\":{appealId}}}");

            return Ok(new { AppealId = appealId });
        }
    }
}
