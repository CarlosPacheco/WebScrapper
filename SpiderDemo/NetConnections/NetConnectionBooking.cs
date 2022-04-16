using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace MisterSpider
{
    public class NetConnectionBooking : NetConnection
    {
        public NetConnectionBooking(ILogger<NetConnectionBooking> logger, IOptions<ConfigOptions> config, IHttpClientFactory clientFactory) : base(logger, config, clientFactory)
        {
        }

        protected override HttpClient GetHttpClient()
        {
            HttpClient request = _httpClientFactory.CreateClient();
            request.DefaultRequestHeaders.Add(HeaderNames.AcceptLanguage, "en-US;q=0.6,en;q=0.4");
            return request;
        }

    }
}
