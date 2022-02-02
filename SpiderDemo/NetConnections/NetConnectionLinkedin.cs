using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace MisterSpider
{
    public class NetConnectionLinkedin : NetConnection
    {
        private string Cookie { get; set; }

        public NetConnectionLinkedin(ILogger<NetConnectionLinkedin> logger, IOptions<ConfigOptions> config) : base(logger, config)
        {
        }

        private void GetCookie()
        {
            if (!string.IsNullOrWhiteSpace(Cookie)) return;

            try
            {
                var cookieJar = new CookieContainer();
                string targetUrl = "https://www.linkedin.com/";
                CookieCollection cookies = new CookieCollection();
                HttpWebRequest request01 = (HttpWebRequest)WebRequest.Create(targetUrl);
                request01.CookieContainer = cookieJar;
                request01.CookieContainer.Add(cookies);
                request01.Method = WebRequestMethods.Http.Get;
                request01.Referer = "https://www.linkedin.com/nhome/";
                //Get the response from the server and save the cookies from the first request..
                HttpWebResponse response01 = (HttpWebResponse)request01.GetResponse();
                cookies = response01.Cookies;

                var request02 = (HttpWebRequest)WebRequest.Create("https://www.linkedin.com/uas/login-submit");

                request02.CookieContainer = cookieJar;
                request02.CookieContainer.Add(cookies); //recover cookies First request
                request02.Method = WebRequestMethods.Http.Post;
                request02.ContentType = "application/x-www-form-urlencoded";
                request02.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
                request02.KeepAlive = true;
                // request02.AllowAutoRedirect = false;

                //Converting postData to Array of Bytes
                byte[] byteArray = null;// Encoding.ASCII.GetBytes(GetDataForm(ReadPage(response01)));

                //Setting Content-Length Header of the Request
                request02.ContentLength = byteArray.Length;

                //Obtaining the Stream To Write Data
                Stream newStream = request02.GetRequestStream();

                //Writing Data To Stream
                newStream.Write(byteArray, 0, byteArray.Length);
                newStream.Close();

                HttpWebResponse response02 = (HttpWebResponse)request02.GetResponse();
                Cookie = request02.Headers[HttpRequestHeader.Cookie] + string.Format("RT=s=1432807786573&r=https%3A%2F%2Fwww.linkedin.com%2Fvsearch%2Fp%3Ftype%3Dpeople%26keywords%3Dhotel%2Bteatro%2Bporto%26orig%3DGLHD%26rsid%3D4251239101432807335687%26pageKey%3Dvoltron_people_search_internal_jsp%26trkInfo%3D%26search%3DPesquisar");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error get the cookie session", ex);
                _logger.LogError("Trying again get the cookie session");
                GetCookie();
            }
        }

        protected override HttpClient GetHttpClient(HttpClientHandler httpClientHandler)
        {
            GetCookie();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36";
            request.Headers.Add(HttpRequestHeader.Cookie, Cookie);

            return null;
        }

        private static string GetDataForm(string htmltext)
        {
            string email = "it@guestu.com";
            string passwd = "uGu3st2115";
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
