/*
    유저 클라이언트가 서버와 통신하기 위한 REST API 클라이언트 클래스입니다.
*/


using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

public class RestApiClient
{
    private static readonly HttpClient httpClient = new HttpClient();

    public async Task<string> SendRequest(HttpMethod method, string url, string jwtToken = null, string content = null)
    {
        var request = new HttpRequestMessage(method, url);

        // JWT 토큰이 제공된 경우 Authorization 헤더에 추가
        if (!string.IsNullOrEmpty(jwtToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        // 요청 본문이 있는 경우 설정
        if (!string.IsNullOrEmpty(content))
        {
            request.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        }

        var response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode(); // HTTP 오류 시 예외 발생

        return await response.Content.ReadAsStringAsync();
    }
}