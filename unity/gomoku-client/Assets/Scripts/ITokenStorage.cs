using System.Threading.Tasks;

public interface ITokenStorage
{
    Task<string> GetAccessTokenAsync();
    
    Task<string> GetRefreshTokenAsync();

    Task UpdateAccessTokenAsync(string newAccessToken);

    Task UpdateRefreshTokenAsync(string newRefreshToken);
}
