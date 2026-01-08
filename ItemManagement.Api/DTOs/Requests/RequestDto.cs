namespace ItemManagement.Api.DTOs.Requests
{
    public class RequestDto
    {
        public int RequestId { get; set; }
        public string RequestedName { get; set; } = "";
        public string? RequestedDescription { get; set; }
        public string Status { get; set; } = "";
        public string? RejectionReason { get; set; }
        public int RequestedByUserId { get; set; }
        public int? DecisionByAdminUserId { get; set; }
        public DateTime? DecidedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
