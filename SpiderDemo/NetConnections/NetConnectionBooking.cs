using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace MisterSpider
{
    public class NetConnectionBooking : NetConnection
    {
        public NetConnectionBooking(ILogger<NetConnectionBooking> logger, IOptions<ConfigOptions> config) : base(logger, config)
        {
        }

        protected override HttpClient GetHttpClient(HttpClientHandler httpClientHandler)
        {
            HttpClient request = new HttpClient(httpClientHandler);
            request.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36");
            request.DefaultRequestHeaders.Add(HeaderNames.AcceptLanguage, "en-US;q=0.6,en;q=0.4");

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36";
           // request..Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US;q=0.6,en;q=0.4");

            return request;
        }

    }
}
