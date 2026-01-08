namespace ItemManagement.Api.DTOs.Requests
{
    public class CreateRequestDto
    {
        public string RequestedName { get; set; } = "";
        public string? RequestedDescription { get; set; }
    }
}
