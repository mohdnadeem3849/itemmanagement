namespace ItemManagement.Api.DTOs.Auth
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Token { get; set; } = "";
        public List<string> Roles { get; set; } = new();
    }
}
