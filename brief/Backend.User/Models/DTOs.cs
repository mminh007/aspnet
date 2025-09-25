namespace Backend.User.Models
{
    public class DTOs
    {
        public class UserDto
        {
            public Guid UserId { get; set; }
            public string Email { get; set; }
            public string? Address { get; set; }
            public string? PhoneNumber { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
