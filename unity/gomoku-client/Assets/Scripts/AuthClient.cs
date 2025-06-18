/*
    AuthInterface를 구현한 AuthClient 클래스입니다.
    Rest API를 사용하여 회원가입과 로그인 기능을 제공합니다.
*/

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class AuthClient : MonoBehaviour, AuthInterface
{
    // 서버 URL, 실제 서버 주소로 변경 필요
    private const string serverUrl = "http://localhost:3000";

    public void SignUp(string username, string password, System.Action<int> onResult)
    {
        // TODO(jpyo0803): 보안을 위해 종단간 암호화를 고려할 것
        StartCoroutine(SignUpCoroutine(username, password, onResult));
    }

    public void Login(string username, string password, System.Action<int, string> onResult)
    {
        // TODO(jpyo0803): 보안을 위해 종단간 암호화를 고려할 것
        StartCoroutine(LoginCoroutine(username, password, onResult));
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