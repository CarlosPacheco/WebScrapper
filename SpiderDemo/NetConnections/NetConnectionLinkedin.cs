using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MisterSpider
{
    public class NetConnectionLinkedin : NetConnection
    {
        private IEnumerable<string> Cookie { get; set; }

        public NetConnectionLinkedin(ILogger<NetConnectionLinkedin> logger, IOptions<ConfigOptions> config, IHttpClientFactory clientFactory) : base(logger, config, clientFactory)
        {
        }

        private void GetCookie()
        {
            if (Cookie != null) return;

            try
            {
                string targetUrl = "https://www.linkedin.com/";

                HttpClient request01 = _httpClientFactory.CreateClient();
                request01.DefaultRequestHeaders.Add(HeaderNames.Referer, "https://www.linkedin.com/nhome/");

                //Get the response from the server and save the cookies from the first request..
                HttpResponseMessage response01 = request01.GetAsync(targetUrl).Result;
                IEnumerable<string> cookies = response01.Headers.GetValues(HeaderNames.SetCookie);

                HttpClient request02 = _httpClientFactory.CreateClient();
                request02.DefaultRequestHeaders.Add(HeaderNames.Cookie, cookies);
                request02.DefaultRequestHeaders.Add(HeaderNames.ContentType, "application/x-www-form-urlencoded");
                request02.DefaultRequestHeaders.Add(HeaderNames.KeepAlive, "true");

                var todoItemJson = new StringContent(
                                GetDataForm(response01.Content.ReadAsStringAsync().Result),
                                Encoding.ASCII,
                                "application/x-www-form-urlencoded");

                HttpResponseMessage response02 = request02.PostAsync("https://www.linkedin.com/uas/login-submit", todoItemJson).Result;

                Cookie = response01.Headers.GetValues(HeaderNames.SetCookie);//string.Format("RT=s=1432807786573&r=https%3A%2F%2Fwww.linkedin.com%2Fvsearch%2Fp%3Ftype%3Dpeople%26keywords%3Dhotel%2Bteatro%2Bporto%26orig%3DGLHD%26rsid%3D4251239101432807335687%26pageKey%3Dvoltron_people_search_internal_jsp%26trkInfo%3D%26search%3DPesquisar");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error get the cookie session", ex);
                _logger.LogError("Trying again get the cookie session");
                GetCookie();
            }
        }

        protected override HttpClient GetHttpClient()
        {
            GetCookie();
            HttpClient request = _httpClientFactory.CreateClient();
            request.DefaultRequestHeaders.Add(HeaderNames.Cookie, Cookie);

            return null;
        }

        private static string GetDataForm(string htmltext)
        {
            string email = "email";
            string passwd = "passwd";
            string postData = string.Empty;

            //Load linkedin login Page HTML
            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(htmltext); //Get Login Form Tag HtmlNode

            HtmlNode node = doc.GetElementbyId("login");

            node = node.ParentNode; //Get All Hidden Input Fields //Prepare Post Data
            int i = 0;
            foreach (HtmlNode h in node.Elements("input"))
            {
                if (i > 0)
                {
                    postData += "&";
                }
                if (i == 1)
                {
                    postData += "session_key=" + email + "&";
                    postData += "session_password=" + passwd + "&";
                }

                postData += (h.GetAttributeValue("name", "") + "=" +
                   h.GetAttributeValue("value", ""));
                i++;
            }
            return postData;
        }

    }
}
