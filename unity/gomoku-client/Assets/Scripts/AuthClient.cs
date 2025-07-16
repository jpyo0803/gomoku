/*
    AuthInterface를 구현한 AuthClient 클래스입니다.
    Rest API를 사용하여 회원가입과 로그인 기능을 제공합니다.
*/

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

internal static class AuthJsonUtils 
{
    public static string ExtractValueFromJson(string key, string json)
    {
        string keyWithQuotes = $"\"{key}\":\"";
        int start = json.IndexOf(keyWithQuotes);
        if (start == -1) return null;

        start += keyWithQuotes.Length;
        int end = json.IndexOf("\"", start);
        if (end == -1) return null;

        return json.Substring(start, end - start);
    }
}

public class AuthClient : AuthInterface
{
    private readonly ILogger logger;

    public AuthClient()
    {
        this.logger = ServiceLocator.Get<ILogger>();
        if (this.logger == null)
        {
            throw new Exception("ILogger service is not registered.");
        }
    }

    public async Task<int> SignUp(string serverUrl, string username, string password)
    {
        logger.Log($"Signup with Username: {username}, Password: {password}");
        var restApiClient = GameManager.instance.RestApiClient;
        string url = $"{serverUrl}/auth/signup";
        string json = JsonSerializer.Serialize(new { username, password });

        try
        {
            HttpResponseMessage response = await restApiClient.SendRequestAsync(HttpMethod.Post, url, useAuth: false, content: json);
            return (int)response.StatusCode;
        }
        catch (Exception e)
        {
            logger.LogError($"SignUp error: {e.Message}");
            return -1; // 예외 발생 시 -1 반환
        }
    }

    public async Task<(int, string, string)> Login(string serverUrl, string username, string password)
    {
        logger.Log($"Login with Username: {username}, Password: {password}");
        var restApiClient = GameManager.instance.RestApiClient;
        string url = $"{serverUrl}/auth/login";
        string json = JsonSerializer.Serialize(new { username, password });

        try
        {
            HttpResponseMessage response = await restApiClient.SendRequestAsync(HttpMethod.Post, url, useAuth: false, content: json);

            // Status 코드가 200인 경우에만 토큰을 추출합니다.
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                string accessToken = AuthJsonUtils.ExtractValueFromJson("accessToken", responseBody);
                string refreshToken = AuthJsonUtils.ExtractValueFromJson("refreshToken", responseBody);
                return ((int)System.Net.HttpStatusCode.OK, accessToken, refreshToken);
            }
            else
            {
                logger.LogError($"Login failed with status code: {response.StatusCode}");
                return ((int)response.StatusCode, null, null);
            }
        }
        catch (Exception e)
        {
            logger.LogError($"Login error: {e.Message}");
            return (-1, null, null);
        }
    }
}