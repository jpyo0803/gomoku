using System.Threading.Tasks;

public interface AuthInterface
{
    Task<int> SignUp(string serverUrl, string username, string password);
    Task<(int code, string accessToken, string refreshToken)> Login(string serverUrl, string username, string password);
}