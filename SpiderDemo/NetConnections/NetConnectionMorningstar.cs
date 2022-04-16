using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace MisterSpider
{
    public class NetConnectionMorningstar : NetConnection
    {
        public string RefererParam { get; set; }

        public NetConnectionMorningstar(ILogger<NetConnectionMorningstar> logger, IOptions<ConfigOptions> config, IHttpClientFactory clientFactory) : base(logger, config, clientFactory)
        {
        }

        protected override HttpClient GetHttpClient()
        {
            HttpClient request = _httpClientFactory.CreateClient();
            request.DefaultRequestHeaders.Add(HeaderNames.Referer, RefererParam);

            return request;
        }

    }
}
