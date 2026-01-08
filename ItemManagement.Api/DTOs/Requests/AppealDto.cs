namespace ItemManagement.Api.DTOs.Requests
{
    public class AppealDto
    {
        public int AppealId { get; set; }
        public int RequestId { get; set; }
        public string AppealMessage { get; set; } = "";
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
