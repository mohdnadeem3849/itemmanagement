using Dapper;
using ItemManagement.Api.Data;

namespace ItemManagement.Api.Repositories
{
    public class AuditRepository
    {
        private readonly DbConnectionFactory _db;

        public AuditRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public void Write(int actorUserId, string actionType, string entityType, int? entityId, string? metadataJson)
        {
            using var conn = _db.CreateConnection();

            conn.Execute(@"
                INSERT INTO dbo.AuditLogs (ActorUserId, ActionType, EntityType, EntityId, MetadataJson)
                VALUES (@ActorUserId, @ActionType, @EntityType, @EntityId, @MetadataJson);
            ", new
            {
                ActorUserId = actorUserId,
                ActionType = actionType,
                EntityType = entityType,
                EntityId = entityId,
                MetadataJson = metadataJson
            });
        }
    }
}
