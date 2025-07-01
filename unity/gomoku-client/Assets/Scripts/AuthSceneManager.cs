using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class AuthSceneManager : MonoBehaviour
{
    private AuthInterface authClient;

    [Header("Input Fields")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    [Header("Buttons")]
    public Button loginButton;
    public Button signupButton;

    [SerializeField]
    private TextMeshProUGUI responseDisplay;

    void Start()
    {
        authClient = new AuthClient(); // Task 기반 구현체

        loginButton.onClick.AddListener(() => OnLoginClicked());
        signupButton.onClick.AddListener(() => OnSignupClicked());
    }

    private async void OnLoginClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        Debug.Log($"[Login Attempt] Username: {username}, Password: {password}");

        var (code, token) = await authClient.Login(username, password);

        if (code == 200 && !string.IsNullOrEmpty(token))
        {
            GameManager.instance.SetJwtToken(token);
            responseDisplay.text = "Login successful!";

            // GomokuClient 연결
            GameManager.instance.ConnectGomokuClient();

            SceneManager.LoadScene("GameSettingScene");
        }
        else
        {
            if (code == 401)
            {
                responseDisplay.text = "Invalid username or password.";
            }
            else
            {
                responseDisplay.text = "Login failed with code: " + code;
            }
        }
    }

    private async void OnSignupClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        Debug.Log($"[SignUp Attempt] Username: {username}, Password: {password}");

        int code = await authClient.SignUp(username, password);

        if (code == 201)
        {
            responseDisplay.text = "SignUp successful! You can now log in.";
        }
        else if (code == 400)
        {
            responseDisplay.text = "Bad request. Please check your input.";
        }
        else if (code == 409)
        {
            responseDisplay.text = "Username already exists. Please choose a different username.";
        }
        else
        {
            Debug.LogError("SignUp failed with code: " + code);
            responseDisplay.text = "SignUp failed with code: " + code;
        }
    }
}
