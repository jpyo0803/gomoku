using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class AuthSceneManager : MonoBehaviour
{
    private AuthInterface authClient;

    [Header("Input Fields")]
    [SerializeField]
    private TMP_InputField usernameInput;
    [SerializeField]
    private TMP_InputField passwordInput;

    [Header("Buttons")]
    [SerializeField]
    private Button loginButton;
    [SerializeField]
    private Button signupButton;

    [Header("Output Displays")]
    [SerializeField]
    private TextMeshProUGUI responseDisplay;

    void Start()
    {
        authClient = new AuthClient(); // Task 기반 구현체
        if (authClient == null)
        {
            Debug.LogError("[Log Error] AuthClient is not initialized properly.");
            return;
        }

        loginButton.onClick.AddListener(() => OnLoginClicked());
        signupButton.onClick.AddListener(() => OnSignupClicked());
    }

    private async void OnSignupClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            responseDisplay.text = "Username and password cannot be empty.";
            Debug.Log("[Log] Username or password is empty.");
            return;
        }

        Debug.Log($"[Log; Signup Attempt] Username: {username}, Password: {password}");

        int code = await authClient.SignUp(GameManager.instance.AuthServerUrl, username, password);

        if (code == 201) // 회원가입 성공
        {
            responseDisplay.text = "SignUp successful! You can now log in.";
        }
        else if (code == 400) // 잘못된 요청
        {
            responseDisplay.text = "Bad request. Please check your input.";
        }
        else if (code == 409) // 중복된 사용자 이름
        {
            responseDisplay.text = "Username already exists. Please choose a different username.";
        }
        else // 기타 오류
        {
            responseDisplay.text = "SignUp failed with code: " + code;
            Debug.LogError($"[Log Error] SignUp failed with code: {code}. Please check the server logs for more details.");
        }
    }

    private async void OnLoginClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            responseDisplay.text = "Username and password cannot be empty.";
            Debug.Log("[Log] Username or password is empty.");
            return;
        }

        Debug.Log($"[Log; Login Attempt] Username: {username}, Password: {password}");

        var (code, token) = await authClient.Login(GameManager.instance.AuthServerUrl, username, password);

        if (code == 200) // 로그인 성공
        {
            GameManager.instance.JwtToken = token; // JWT 토큰 저장
            responseDisplay.text = "Login successful!";
            Debug.Log("[Log] Login successful!");

            SceneManager.LoadScene("GameSettingScene");
        }
        else if (code == 401) // 인증 실패
        {
            responseDisplay.text = "Invalid username or password.";
            Debug.Log("[Log] Invalid username or password.");
        }
        else // 기타 오류
        {
            responseDisplay.text = "Login failed with code: " + code;
            Debug.LogError($"[Log Error] Login failed with code: {code}. Please check the server logs for more details.");
        }
    }
}
