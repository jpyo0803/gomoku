/*
    유저 클라이언트가 서버와 통신하기 위한 REST API 클라이언트 클래스입니다.
*/

using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System;

public class RefreshResponse
{
    public string message { get; set; }
    public string accessToken { get; set; }
}

public class RestApiClient
{
    private static readonly HttpClient httpClient = new HttpClient();
    private bool isRefreshing = false;

    public async Task<string> SendRequest(HttpMethod method, string url, bool useAuth = false, string content = null)
    {
        var request = new HttpRequestMessage(method, url);

        if (useAuth && !string.IsNullOrEmpty(GameManager.instance.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GameManager.instance.AccessToken);
        }

        if (!string.IsNullOrEmpty(content))
        {
            request.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        }

        var response = await httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !isRefreshing)
        {
            GameManager.instance.DebugLog("[Log] Access token expired, attempting to refresh.");
            isRefreshing = true; // 토큰 갱신 중에 새로운 요청이 들어오지 않도록 플래그 설정 
            bool tokenRefreshed = await RefreshTokenAsync();
            isRefreshing = false;

            if (tokenRefreshed)
            {
                // 토큰이 갱신되었으므로 다시 요청을 시도
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GameManager.instance.AccessToken);
                return await SendRequest(method, url, useAuth, content);
            }
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<bool> RefreshTokenAsync()
    {
        try
        {
            // 서버의 /auth/refresh 엔드포인트에 요청
            var refreshContent = new { refreshToken = GameManager.instance.RefreshToken };
            var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(refreshContent);
            var requestContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            string url = $"{GameManager.instance.AuthServerUrl}/auth/refresh";
            var response = await httpClient.PostAsync(url, requestContent);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var newTokens = Newtonsoft.Json.JsonConvert.DeserializeObject<RefreshResponse>(responseString);

            // 새 토큰 저장
            GameManager.instance.AccessToken = newTokens.accessToken;

            GameManager.instance.DebugLog("[Log] Token refreshed successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to refresh token: " + ex.Message);
            // NOTE(jpyo0803): 리프레시 토큰도 만료된 경우 로그아웃 처리 필요. 현재는 예외 처리만 함
            return false;
        }
    }
}