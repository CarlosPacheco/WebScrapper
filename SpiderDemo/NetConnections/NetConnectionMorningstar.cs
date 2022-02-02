using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace MisterSpider
{
    public class NetConnectionMorningstar : NetConnection
    {
        public string RefererParam { get; set; }

        public NetConnectionMorningstar(ILogger<NetConnectionMorningstar> logger, IOptions<ConfigOptions> config) : base(logger, config)
        {
        }

        protected override HttpClient GetHttpClient(HttpClientHandler httpClientHandler)
        {
            HttpClient request = httpClientHandler == null ? new HttpClient() : new HttpClient(httpClientHandler);
            request.DefaultRequestHeaders.Add(HeaderNames.Referer, RefererParam);

            return request;
        }

    }
}
