using System.Threading.Tasks;

public interface AuthInterface
{
    Task<int> SignUp(string username, string password);
    Task<(int code, string token)> Login(string username, string password);
}