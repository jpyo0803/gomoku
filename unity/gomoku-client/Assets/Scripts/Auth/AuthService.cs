/*
    AuthInterface를 구현한 AuthClient 클래스입니다.
    Rest API를 사용하여 회원가입과 로그인 기능을 제공합니다.
*/

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using UnityEngine;

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

                string responseBody = await response.Content.ReadAsStringAsync();

                // JSON 파싱
                ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(responseBody);
                
                if (apiResponse.content.success)
                {
                    return new SignupResponse
                    {
                        HttpStatusCode = apiResponse.statusCode,
                        Success = true,
                        Message = apiResponse.content.message,
                        Username = apiResponse.content.data.username,
                        CreatedAt = apiResponse.content.data.createdAt
                    };
                }
                else
                {
                    return new SignupResponse
                    {
                        HttpStatusCode = apiResponse.statusCode,
                        Success = false,
                        Message = apiResponse.content.message,         // 실패 메시지
                        ErrorCode = apiResponse.content.error.code,    // 에러 코드
                        ErrorDetails = apiResponse.content.error.details // 에러 상세
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"SignUp error: {e.Message}");
                return new SignupResponse { HttpStatusCode = -1 };
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

                string responseBody = await response.Content.ReadAsStringAsync();

                // JSON 파싱
                ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(responseBody);

                if (apiResponse.content.success)
                {
                    return new LoginResponse
                    {
                        HttpStatusCode = apiResponse.statusCode,
                        Success = true,
                        Message = apiResponse.content.message,
                        Username = apiResponse.content.data.username,
                        AccessToken = apiResponse.content.data.accessToken,
                        RefreshToken = apiResponse.content.data.refreshToken
                    };
                }
                else
                {
                    return new LoginResponse
                    {
                        HttpStatusCode = apiResponse.statusCode,
                        Success = false,
                        Message = apiResponse.content.message,
                        ErrorCode = apiResponse.content.error.code,
                        ErrorDetails = apiResponse.content.error.details
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Login error: {e.Message}");
                return new LoginResponse { HttpStatusCode = -1 };
            }
        }

        public async Task<RefreshResponse> RefreshToken(RefreshDto dto)
        {
            var refreshContent = new { refreshToken = dto.RefreshToken };
            var jsonContent = JsonSerializer.Serialize(refreshContent);
            string url = $"{dto.ServerUrl}/auth/refresh";
            try
            {
                HttpResponseMessage response = await _httpProxy.PostAsync(new HttpArgs
                {
                    Url = url,
                    Content = jsonContent,
                    Token = null // Refresh 시 토큰은 필요하지 않음
                });

                string responseBody = await response.Content.ReadAsStringAsync();

                ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(responseBody);

                if (apiResponse.content.success)
                {
                    return new RefreshResponse
                    {
                        HttpStatusCode = apiResponse.statusCode,
                        Success = true,
                        Message = apiResponse.content.message,
                        AccessToken = apiResponse.content.data.accessToken
                    };
                }
                else
                {
                    return new RefreshResponse
                    {
                        HttpStatusCode = apiResponse.statusCode,
                        Success = false,
                        Message = apiResponse.content.message,
                        ErrorCode = apiResponse.content.error.code,
                        ErrorDetails = apiResponse.content.error.details
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"RefreshToken error: {e.Message}");
                return new RefreshResponse { HttpStatusCode = -1 };
            }
        }
    }
}