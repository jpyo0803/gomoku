using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Net.Http.Headers;

namespace jpyo0803
{
    public class HttpService : IHttpProxy
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly ILogger _logger;

        public HttpService()
        {
            _logger = ServiceLocator.Get<ILogger>();
            if (_logger == null)
            {
                throw new Exception("ILogger service is not registered.");
            }
        }

        public async Task<HttpResponseMessage> GetAsync(HttpArgs args)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, args.Url);
                if (!string.IsNullOrEmpty(args.Token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", args.Token);
                }

                if (!string.IsNullOrEmpty(args.Content))
                {
                    request.Content = new StringContent(args.Content, System.Text.Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError($"GET request failed: {e.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync(HttpArgs args)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, args.Url);
                if (!string.IsNullOrEmpty(args.Token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", args.Token);
                }

                if (!string.IsNullOrEmpty(args.Content))
                {
                    request.Content = new StringContent(args.Content, System.Text.Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError($"POST request failed: {e.Message}");
                throw;
            }
        }
    }
}