using System.Threading.Tasks;

namespace jpyo0803
{
    public interface ITokenStorage
    {
        Task<string> GetAccessTokenAsync();

        Task<string> GetRefreshTokenAsync();

        Task UpdateAccessTokenAsync(string newAccessToken);

        Task UpdateRefreshTokenAsync(string newRefreshToken);
    }
}