/*
    AuthInterface를 구현한 AuthClient 클래스입니다.
    Rest API를 사용하여 회원가입과 로그인 기능을 제공합니다.
*/

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

internal static class AuthJsonUtils 
{
    public static string ExtractTokenFromJson(string json)
    {
        const string tokenKey = "\"token\":\"";
        int start = json.IndexOf(tokenKey);
        if (start == -1) return null;

        start += tokenKey.Length;
        int end = json.IndexOf("\"", start);
        if (end == -1) return null;

        return json.Substring(start, end - start);
    }
}

public class AuthClient : AuthInterface
{
    private static readonly HttpClient httpClient = new HttpClient();

    public async Task<int> SignUp(string serverUrl, string username, string password)
    {
        string url = $"{serverUrl}/auth/signup";
        string json = JsonSerializer.Serialize(new { username, password });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(url, content);
            return (int)response.StatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Log] SignUp error: {ex.Message}");
            return -1;
        }
    }

    public async Task<(int, string)> Login(string serverUrl, string username, string password)
    {
        string url = $"{serverUrl}/auth/login";
        string json = JsonSerializer.Serialize(new { username, password });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(url, content);
            int code = (int)response.StatusCode;

            if (code == 200)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                string token = AuthJsonUtils.ExtractTokenFromJson(responseBody);
                return (code, token);
            }

            return (code, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Log] Login error: {ex.Message}");
            return (-1, null);
        }
    }
}