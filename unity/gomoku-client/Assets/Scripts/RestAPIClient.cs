/*
    유저 클라이언트가 서버와 통신하기 위한 REST API 클라이언트 클래스입니다.
*/


using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

public class RestAPIClient
{
    private static readonly HttpClient httpClient = new HttpClient();

    // Match history 조회
    public async Task<string> RequestMatchHistory(string serverUrl, string jwtToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{serverUrl}/Sql/my-match-history");

        // JWT 토큰을 Authorization 헤더에 추가
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        var response = await httpClient.SendAsync(request);
        
        response.EnsureSuccessStatusCode(); // HTTP 오류 시 예외 발생

        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }
}