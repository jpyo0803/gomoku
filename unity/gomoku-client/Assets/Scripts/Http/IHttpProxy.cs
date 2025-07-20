using System.Net.Http;
using System.Threading.Tasks;

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