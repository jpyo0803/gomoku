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

public class AuthService : IAuth
{
    private IHttpProxy _httpProxy;

    private readonly ILogger _logger;

    public AuthService()
    {
        _httpProxy = new HttpProxy();

        this._logger = ServiceLocator.Get<ILogger>();
        if (this._logger == null)
        {
            throw new Exception("ILogger service is not registered.");
        }
    }

    public async Task<AuthResponse> SignUp(AuthArgs args)
    {
        _logger.Log($"Signup with Username: {args.Username}, Password: {args.Password}");

        string url = $"{args.ServerUrl}/auth/signup";
        string json = JsonSerializer.Serialize(new { username = args.Username, password = args.Password });


        try
        {
            HttpResponseMessage response = await _httpProxy.PostAsync(new HttpArgs
            {
                Url = url,
                Content = json,
                Token = null // SignUp 시 토큰은 필요하지 않음
            });
            return new AuthResponse
            {
                Code = (int)response.StatusCode,
                AccessToken = null,
                RefreshToken = null
            };
        }
        catch (Exception e)
        {
            _logger.LogError($"SignUp error: {e.Message}");
            return new AuthResponse { Code = -1 };
        }
    }

    public async Task<AuthResponse> Login(AuthArgs args)
    {
        _logger.Log($"Login with Username: {args.Username}, Password: {args.Password}");

        string url = $"{args.ServerUrl}/auth/login";
        string json = JsonSerializer.Serialize(new { username = args.Username, password = args.Password });

        try
        {
            HttpResponseMessage response = await _httpProxy.PostAsync(new HttpArgs
            {
                Url = url,
                Content = json,
                Token = null // Login 시 토큰은 필요하지 않음
            });

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                string accessToken = AuthJsonUtils.ExtractValueFromJson("accessToken", content);
                string refreshToken = AuthJsonUtils.ExtractValueFromJson("refreshToken", content);

                return new AuthResponse
                {
                    Code = (int)response.StatusCode,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }
            else
            {
                return new AuthResponse { Code = (int)response.StatusCode };
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Login error: {e.Message}");
            return new AuthResponse { Code = -1 };
        }
    }
}