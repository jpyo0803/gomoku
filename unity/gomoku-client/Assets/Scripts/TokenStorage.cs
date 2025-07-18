using System.Threading.Tasks;

public class TokenStorage : ITokenStorage
{
    private string _accessToken;
    private string _refreshToken;
    
    public Task<string> GetAccessTokenAsync()
    {
        return Task.FromResult(_accessToken);
    }

    public Task<string> GetRefreshTokenAsync()
    {
        return Task.FromResult(_refreshToken);
    }

    public Task UpdateAccessTokenAsync(string newAccessToken)
    {
        _accessToken = newAccessToken;
        return Task.CompletedTask;
    }

    public Task UpdateRefreshTokenAsync(string newRefreshToken)
    {
        _refreshToken = newRefreshToken;
        return Task.CompletedTask;
    }
}
