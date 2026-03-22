namespace WebApp.Models.DTOs
{
    public enum AuthStatus
    {
        Success,
        InvalidCredentials,
        Blocked
    }

    public class AuthResult
    {
        public AuthStatus Status { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
    }
}
