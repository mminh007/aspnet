namespace Commons.Models
{
    public class DTOs
    {
        public class IdentityDto
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
        }

        public class TokenDto
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public int ExpiresIn { get; set; }
            public string TokenType { get; set; } = "Bearer";
            public string Roles { get; set; }
        }
    }
}
