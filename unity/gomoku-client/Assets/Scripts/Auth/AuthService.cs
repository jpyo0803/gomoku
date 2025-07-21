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

namespace jpyo0803
{
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

        public async Task<SignupResponse> SignUp(SignUpLoginDto dto)
        {
            _logger.Log($"Signup with Username: {dto.Username}, Password: {dto.Password}");

            string url = $"{dto.ServerUrl}/auth/signup";
            string json = JsonSerializer.Serialize(new { username = dto.Username, password = dto.Password });


            try
            {
                HttpResponseMessage response = await _httpProxy.PostAsync(new HttpArgs
                {
                    Url = url,
                    Content = json,
                    Token = null // SignUp 시 토큰은 필요하지 않음
                });
                return new SignupResponse
                {
                    Code = (int)response.StatusCode
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"SignUp error: {e.Message}");
                return new SignupResponse { Code = -1 };
            }
        }

        public async Task<LoginResponse> Login(SignUpLoginDto dto)
        {
            _logger.Log($"Login with Username: {dto.Username}, Password: {dto.Password}");

            string url = $"{dto.ServerUrl}/auth/login";
            string json = JsonSerializer.Serialize(new { username = dto.Username, password = dto.Password });

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

                    return new LoginResponse
                    {
                        Code = (int)response.StatusCode,
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    };
                }
                else
                {
                    return new LoginResponse { Code = (int)response.StatusCode };
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Login error: {e.Message}");
                return new LoginResponse { Code = -1 };
            }
        }

        public async Task<RefreshResponse> RefreshToken(RefreshDto dto)
        {
            // Not implemented yet
            _logger.Log("RefreshToken method is not implemented yet.");
            return new RefreshResponse { Code = -1 };
        }
    }
}