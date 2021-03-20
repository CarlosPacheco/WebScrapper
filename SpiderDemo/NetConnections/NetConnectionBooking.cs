using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MisterSpider
{
    public class NetConnectionBooking : NetConnection
    {
        public NetConnectionBooking(ILogger logger, IOptions<ConfigOptions> config) : base(logger, config)
        {
        }

        protected override HttpWebRequest GetHttpWebRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US;q=0.6,en;q=0.4");

            return request;
        }

    }
}
