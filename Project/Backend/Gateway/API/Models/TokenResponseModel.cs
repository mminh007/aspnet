﻿namespace API.Models
{
    public class TokenResponseModel
    {

        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }

    }
}
