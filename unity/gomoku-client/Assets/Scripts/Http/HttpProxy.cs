using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace jpyo0803
{
    public class HttpProxy : IHttpProxy
    {
        private readonly HttpService _httpService; // HttpService 인스턴스 생성
        private readonly ILogger _logger;

        public HttpProxy()
        {
            _httpService = ServiceLocator.Get<HttpService>();
            if (_httpService == null)
            {
                throw new Exception("HttpService is not registered.");
            }

            // ILogger 서비스 가져오기
            _logger = ServiceLocator.Get<ILogger>();
            if (_logger == null)
            {
                throw new Exception("ILogger service is not registered.");
            }
        }

        public async Task<HttpResponseMessage> GetAsync(HttpArgs args)
        {
            return await _httpService.GetAsync(args);
        }

        public async Task<HttpResponseMessage> PostAsync(HttpArgs args)
        {
            return await _httpService.PostAsync(args);
        }
    }
}