namespace ItemManagement.Api.DTOs.Admin
{
    public class CreateUserDto
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "User"; // "User" or "Admin"
    }
}
