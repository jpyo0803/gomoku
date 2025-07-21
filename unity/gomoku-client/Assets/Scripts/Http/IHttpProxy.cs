using System.Net.Http;
using System.Threading.Tasks;

namespace jpyo0803
{
    public class HttpArgs
    {
        public string Url { get; set; }
        public string Token { get; set; }
        public string Content { get; set; }
    }

    public interface IHttpProxy
    {
        Task<HttpResponseMessage> GetAsync(HttpArgs args);
        Task<HttpResponseMessage> PostAsync(HttpArgs args);
    }
}