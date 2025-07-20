using System.Threading.Tasks;

public class AuthArgs
{
    public string ServerUrl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

public class AuthResponse
{
    public int Code { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public interface IAuth
{
    Task<AuthResponse> SignUp(AuthArgs args);
    Task<AuthResponse> Login(AuthArgs args);
}