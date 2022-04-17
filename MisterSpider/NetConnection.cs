using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        protected IHttpClientFactory _httpClientFactory { get; }

        public NetConnection(ILogger<NetConnection> logger, IOptions<ConfigOptions> config, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _httpClientFactory = clientFactory;
            _config = config.Value;
        }

        public Stream? Read(Url url)
        {
            return ReadPage(url.uri.AbsoluteUri);
        }

        public Stream? Read(string absoluteUri)
        {
            return ReadPage(absoluteUri);
        }

        protected virtual HttpClient GetHttpClient()
        {
            return _httpClientFactory.CreateClient();
        }

        private HttpResponseMessage? GetResponse(string url)
        {
            HttpResponseMessage response = null;

            _logger.LogDebug("Loading new page, {0}", url);

            try
            {
                HttpClient request = GetHttpClient();

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

        protected Stream? ReadPage(string url)
        {
            HttpResponseMessage? response = GetResponse(url);

            Stream? stream = null;
            if (response == null) return stream;
            if (!response.IsSuccessStatusCode) return stream;

            try
            {
                stream = response.Content.ReadAsStream();
            }
            catch (Exception ex)
            {
                _logger.LogError("Fatal Error: {0}", ex.Message);
                response?.Dispose();
            }

            return stream;
        }
    }
}
