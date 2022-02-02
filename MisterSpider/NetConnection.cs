using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace MisterSpider
{
    public class NetConnection : INetConnection
    {
        private ConfigOptions _config;

        protected ILogger _logger { get; }

        public NetConnection(ILogger<NetConnection> logger, IOptions<ConfigOptions> config)
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

        protected virtual HttpClient GetHttpClient(HttpClientHandler httpClientHandler)
        {
            HttpClient client = httpClientHandler == null ? new HttpClient() : new HttpClient(httpClientHandler);
            client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36");

            return client;
        }

        private HttpResponseMessage GetResponse(string url)
        {
            HttpResponseMessage response = null;
            HttpClientHandler httpClientHandler = null;

            _logger.LogDebug("Loading new page, {0}", url);

            try
            {
                if (!string.IsNullOrWhiteSpace(_config.WebProxyAddress))
                {
                    //United States proxy, from http://www.hidemyass.com/proxy-list/
                    // myProxy.BypassProxyOnLocal = true;
                    httpClientHandler = new HttpClientHandler()
                    {
                        Proxy = new WebProxy(_config.WebProxyAddress, true),
                        PreAuthenticate = true,
                        UseDefaultCredentials = false,
                        // DefaultProxyCredentials = CredentialCache.DefaultCredentials,
                    };

                    //request.Proxy = myProxy;
                }
                HttpClient request = GetHttpClient(httpClientHandler);

                response = request.GetAsync(url).Result;
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

        protected string ReadPage(HttpResponseMessage response)
        {
            string html = string.Empty;
            if (response == null) return html;

            try
            {
                html = response.Content.ReadAsStringAsync().Result;
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
