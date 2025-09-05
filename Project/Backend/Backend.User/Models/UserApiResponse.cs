namespace Backend.User.Models
{
    public class UserApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
