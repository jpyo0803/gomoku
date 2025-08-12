using System.Threading.Tasks;

namespace jpyo0803
{
    [System.Serializable]
    public class ApiResponse
    {
        public ResponseContent content;
        public int statusCode;
        public string reasonPhrase;
        public bool isSuccessStatusCode;
        public string version;
    }

    [System.Serializable]
    public class ResponseContent
    {
        public bool success;
        public string message;      // 성공/실패 모두 있음
        public UserData data;       // 성공시만 있음
        public ErrorInfo error;     // 실패시만 있음
    }

    [System.Serializable]
    public class UserData
    {
        public string username;
        public string createdAt;
        public string accessToken;
        public string refreshToken;
    }

    [System.Serializable]
    public class ErrorInfo
    {
        public string code;
        public string details;
    }

    public class SignUpLoginDto
    {
        public string ServerUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SignupResponse
    {
        public int HttpStatusCode { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        public string CreatedAt { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDetails;
    }

    public class LoginResponse
    {
        public int HttpStatusCode { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDetails { get; set; }
    }

    public class RefreshResponse
    {
        public int HttpStatusCode { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDetails { get; set; }
    }

    public class RefreshDto
    {
        public string ServerUrl { get; set; }
        public string RefreshToken { get; set; }
    }

    public interface IAuth
    {
        Task<SignupResponse> SignUp(SignUpLoginDto dto);
        Task<LoginResponse> Login(SignUpLoginDto dto);
        Task<RefreshResponse> RefreshToken(RefreshDto args);
    }
}