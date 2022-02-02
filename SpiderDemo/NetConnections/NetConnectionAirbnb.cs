using HtmlAgilityPack;
using System;
using System.Net;
using MisterSpider.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Microsoft.Net.Http.Headers;

namespace MisterSpider
{
    public class NetConnectionAirbnb : NetConnection
    {
        private const string HttpAuthenticityToken = "authenticity_token";

        private CookieCollection Cookies { get; set; }

        private string SessionToken { get; set; }

        private string TargetUrl { get; set; }

        public NetConnectionAirbnb(string targetUrl, ILogger<NetConnectionAirbnb> logger, IOptions<ConfigOptions> config) : base(logger, config)
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
                CookieContainer cookieJar = new CookieContainer();
                Cookies = new CookieCollection();
                HttpClient request01 = new HttpClient(new HttpClientHandler() { CookieContainer = cookieJar, UseCookies = true, });
                cookieJar.Add(Cookies);

                request01.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36");

                // HttpWebRequest request01 = (HttpWebRequest)WebRequest.Create(TargetUrl);
                // request01.CookieContainer = cookieJar;
                // request01.CookieContainer.Add(Cookies);
                // request01.Method = WebRequestMethods.Http.Get;
                // request01.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36";
                // request01.Proxy = null;

                //Get the response from the server and save the cookies from the first request..
                HttpResponseMessage response01 = request01.GetAsync(TargetUrl).Result;

                Cookies = cookieJar.GetCookies(new Uri(TargetUrl));
                SessionToken = GetDataForm(ReadPage(response01));

            }
            catch (Exception ex)
            {
                _logger.LogError("Trying again get the cookie session, Error get the cookie session", ex);
                GetHeaders();
            }
        }

        protected override HttpClient GetHttpClient(HttpClientHandler httpClientHandler)
        {
            GetHeaders();

            CookieContainer cookieJar = new CookieContainer();
            cookieJar.Add(Cookies);
            HttpClient request = new HttpClient(new HttpClientHandler() { CookieContainer = cookieJar, UseCookies = true, });
            request.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36");
            request.DefaultRequestHeaders.Add(HeaderNames.ContentType, "application/json, text/javascript, */*; q=0.01");
            request.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            
            // HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("{0}&authenticity_token={1}", url, SessionToken));
            // request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36";
            //  request.CookieContainer = request.CookieContainer ?? new CookieContainer();
            //request.CookieContainer.Add(Cookies);
            //request.ContentType = "application/json, text/javascript, */*; q=0.01";
            //request.Headers.Add("X-Requested-With", "XMLHttpRequest");

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
