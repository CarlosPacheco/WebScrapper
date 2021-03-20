using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;

namespace MisterSpider
{
    public class NetConnection : INetConnection
    {     
        private ConfigOptions _config;
        protected ILogger _logger { get; }

        public NetConnection(ILogger logger, IOptions<ConfigOptions> config)
        {
            _logger = logger;
            _config = config.Value;
        }

        public string Go(Url url)
        {
            return ReadPage(GetResponse(url.uri.AbsoluteUri));
        }

        public string Go(string absoluteUri)
        {
            return ReadPage(GetResponse(absoluteUri));
        }

        protected virtual HttpWebRequest GetHttpWebRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36";
            request.Proxy = null;

            return request;
        }

        private HttpWebResponse GetResponse(string url)
        {
            HttpWebResponse response = null;
            _logger.LogError("Loading new page, {0}", url);

            try
            {
                HttpWebRequest request = GetHttpWebRequest(url);

                if (_config.IpAddress != null)
                {
                    WebProxy myProxy = new WebProxy(_config.IpAddress.Address.ToString(), _config.IpAddress.Port);

                    //United States proxy, from http://www.hidemyass.com/proxy-list/
                    myProxy.BypassProxyOnLocal = true;
                    request.Proxy = myProxy;
                }

                response = (HttpWebResponse)request.GetResponse();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Accepted: break;
                    case HttpStatusCode.OK: break;
                    case HttpStatusCode.Found: break;
                    case HttpStatusCode.MovedPermanently: break;
                    default:
                        throw new Exception(response.StatusCode.ToString());
                }
            }
            catch (WebException ex)
            {
                _logger.LogError("Network Error: {0}\nStatus code: {1}", ex.Message, ex.Status);
            }
            catch (ProtocolViolationException ex)
            {
                _logger.LogError("Protocol Error: {0}", ex.Message);
            }
            catch (UriFormatException ex)
            {
                _logger.LogError("URI Format Error: {0}", ex.Message);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogError("Unknown Protocol: {0}", ex.Message);
            }
            catch (IOException ex)
            {
                _logger.LogError("I/O Error: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Fatal Error: {0}", ex.Message);
            }

            return response;
        }

        protected string ReadPage(HttpWebResponse response)
        {
            string html = string.Empty;
            if (response == null) return html;
            
            try
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    html = reader.ReadToEnd();
                }
                response.Dispose();
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (WebException ex)
            {
                _logger.LogError("Network Error: {0}\nStatus code: {1}", ex.Message, ex.Status);
            }
            catch (IOException ex)
            {
                _logger.LogError("I/O Error: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Fatal Error: {0}", ex.Message);
            }

            return html;
        }
    }
}
