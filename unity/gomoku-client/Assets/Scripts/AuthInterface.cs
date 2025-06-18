public interface AuthInterface
{
    void SignUp(string username, string password, System.Action<int> onResult);
    void Login(string username, string password, System.Action<int, string> onResult);
}