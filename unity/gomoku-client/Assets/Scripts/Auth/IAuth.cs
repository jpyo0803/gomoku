using System.Threading.Tasks;

namespace jpyo0803
{
    public class SignUpLoginDto
    {
        public string ServerUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SignupResponse
    {
        public int Code { get; set; }
    }

    public class LoginResponse
    {
        public int Code { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RefreshDto
    {
        public string ServerUrl { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RefreshResponse
    {
        public int Code { get; set; }
        public string AccessToken { get; set; }
    }

    public interface IAuth
    {
        Task<SignupResponse> SignUp(SignUpLoginDto dto);
        Task<LoginResponse> Login(SignUpLoginDto dto);
        Task<RefreshResponse> RefreshToken(RefreshDto args);
    }
}