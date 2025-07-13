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
    public async Task<int> SignUp(string serverUrl, string username, string password)
    {
        var restApiClient = GameManager.instance.RestApiClient;
        string url = $"{serverUrl}/auth/signup";
        string json = JsonSerializer.Serialize(new { username, password });

        try
        {
            string responseBody = await restApiClient.SendRequest(HttpMethod.Post, url, useAuth: false, content: json);
            return 201; // 성공적으로 왔다면 회원가입은 보통 201 Created
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[Log] SignUp HTTP error: {e.Message}");
            return -1;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Log] SignUp general error: {e.Message}");
            return -1;
        }
    }

    public async Task<(int, string, string)> Login(string serverUrl, string username, string password)
    {
        var restApiClient = GameManager.instance.RestApiClient;
        string url = $"{serverUrl}/auth/login";
        string json = JsonSerializer.Serialize(new { username, password });

        try
        {
            string responseBody = await restApiClient.SendRequest(HttpMethod.Post, url, useAuth: false, content: json);
            string accessToken = AuthJsonUtils.ExtractValueFromJson("accessToken", responseBody);
            string refreshToken = AuthJsonUtils.ExtractValueFromJson("refreshToken", responseBody);
            return (200, accessToken, refreshToken);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[Log] Login HTTP error: {e.Message}");
            return (-1, null, null);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Log] Login general error: {e.Message}");
            return (-1, null, null);
        }
    }
}