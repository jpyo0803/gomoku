using UnityEngine;
using UnityEngine.UI;
using TMPro; // TMP_InputField를 쓰기 위해 필요
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

public class AuthManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    [Header("Buttons")]
    public Button loginButton;
    public Button signupButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private string jwtToken;

    private const string serverUrl = "http://localhost:3000";

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        signupButton.onClick.AddListener(OnSignupClicked);
    }

    void OnLoginClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        Debug.Log($"[Login Attempt] Username: {username}, Password: {password}");

        StartCoroutine(LoginCoroutine(username, password, (code, token) =>
        {
            if (code == 200 && !string.IsNullOrEmpty(token))
            {
                jwtToken = token;
                Debug.Log("Login successful! JWT Token: " + jwtToken);
            }
            else
            {
                Debug.LogError("Login failed with code: " + code);
            }
        }));
    }

    void OnSignupClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        Debug.Log($"[SignUp Attempt] Username: {username}, Password: {password}");

        StartCoroutine(SignUpCoroutine(username, password, (code) =>
        {
            if (code == 201)
            {
                Debug.Log("SignUp successful.");
            }
            else
            {
                Debug.LogError("SignUp failed with code: " + code);
            }
        }));
    }

    private IEnumerator SignUpCoroutine(string username, string password, System.Action<int> onResult)
    {
        string url = $"{serverUrl}/auth/signup";
        string json = $"{{\"username\": \"{username}\", \"password\": \"{password}\"}}";
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(jsonBytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        onResult?.Invoke((int)req.responseCode);
    }

    private IEnumerator LoginCoroutine(string username, string password, System.Action<int, string> onResult)
    {
        string url = $"{serverUrl}/auth/login";
        string json = $"{{\"username\": \"{username}\", \"password\": \"{password}\"}}";
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(jsonBytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        int code = (int)req.responseCode;

        if (code == 200)
        {
            // 응답에서 token 추출
            string jsonResponse = req.downloadHandler.text;
            string token = ExtractTokenFromJson(jsonResponse);
            onResult?.Invoke(code, token);
        }
        else
        {
            onResult?.Invoke(code, null);
        }
    }
    
    private string ExtractTokenFromJson(string json)
    {
        // 간단한 문자열 파싱 (MiniJSON 없을 때)
        const string tokenKey = "\"token\":\"";
        int start = json.IndexOf(tokenKey);
        if (start == -1) return null;

        start += tokenKey.Length;
        int end = json.IndexOf("\"", start);
        if (end == -1) return null;

        return json.Substring(start, end - start);
    }
}
