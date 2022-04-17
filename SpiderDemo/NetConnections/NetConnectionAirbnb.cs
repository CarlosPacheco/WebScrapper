using HtmlAgilityPack;
using System;
using MisterSpider.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;

namespace MisterSpider
{
    public class NetConnectionAirbnb : NetConnection
    {
        private const string HttpAuthenticityToken = "authenticity_token";

        private IEnumerable<string> Cookies { get; set; }

        private string SessionToken { get; set; }

        private string TargetUrl { get; set; }

        public NetConnectionAirbnb(string targetUrl, ILogger<NetConnectionAirbnb> logger, IOptions<ConfigOptions> config, IHttpClientFactory clientFactory) : base(logger, config, clientFactory)
        {
            TargetUrl = targetUrl;
        }

        private void GetHeaders()
        {
            if (Cookies != null && !string.IsNullOrWhiteSpace(SessionToken))
            {
                return;
            }

            try
            {
                HttpClient request01 = _httpClientFactory.CreateClient();
                
                //Get the response from the server and save the cookies from the first request..
                HttpResponseMessage response01 = request01.GetAsync(TargetUrl).Result;
                Cookies = response01.Headers.GetValues(HeaderNames.SetCookie);

                SessionToken = GetDataForm(response01.Content.ReadAsStringAsync().Result);

            }
            catch (Exception ex)
            {
                _logger.LogError("Trying again get the cookie session, Error get the cookie session", ex);
                GetHeaders();
            }
        }

        protected override HttpClient GetHttpClient()
        {
            GetHeaders();
           
            HttpClient request = _httpClientFactory.CreateClient();
            request.DefaultRequestHeaders.Add(HeaderNames.ContentType, "application/json, text/javascript, */*; q=0.01");
            request.DefaultRequestHeaders.Add(HeaderNames.SetCookie, Cookies);
            request.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            
            return request;
        }

        private static string GetDataForm(string htmltext)
        {
            //Load HTMML Page
            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(htmltext); //Get Login Form Tag HtmlNode
            string token = doc.DocumentNode.SelectSingleNode(string.Format("//body//input[contains(@name, '{0}')]", HttpAuthenticityToken)).Extract("value", false);

            return token;
        }
    }
}
