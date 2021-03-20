using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MisterSpider
{
    public class Page
    {
        private static Regex UrlPattern = new Regex(@"(href|src)=""[\d\w\/:#@%;$\(\)~_\?\+\-=\\\.&]*", RegexOptions.Compiled | RegexOptions.IgnoreCase); 
        private ILogger _logger { get; }
        private List<Url> UrlList { get; set; }
        public Url Url { get; private set; }
        public string Source { get; private set; }
        public HtmlDocument document { get; private set; }

        public Page(ILogger logger, Url url, string source)
        {
            _logger = logger;
            Url = url;
            Source = source;
            document = new HtmlDocument();
            document.LoadHtml(source);
            UrlList = new List<Url>();
        }

        public void FetchAllUrls(int depth)
        {
            MatchCollection matches = UrlPattern.Matches(Source);

            foreach (Match match in matches)
            {
                string cleanUrl = CleanUrl(match.Value);

                if (!string.IsNullOrEmpty(cleanUrl))
                {
                    Url url = new Url(new Uri(cleanUrl), depth + 1);

                    UrlList.Add(url);
                    _logger.LogInformation("Grabbed URL from page", url.uri.AbsoluteUri);
                }
            }
        }

        public string CleanUrl(string url)
        {
            StringBuilder cleanUrl = new StringBuilder(string.Empty);

            if (!url.Contains("mailto:"))
            {
                try
                {
                    cleanUrl.Append(Regex.Replace(url, @"(?i)(href|src)=|""", ""));

                    Uri uri;

                    if (!IsAbsoluteUrl(cleanUrl.ToString()))
                    {
                        if (cleanUrl.ToString().StartsWith("/"))
                        {
                            uri = new Uri(GetParentUriString(Url.uri), cleanUrl.ToString());
                        }
                        else
                        {
                            uri = new Uri(Url.uri.AbsoluteUri + cleanUrl.ToString());
                        }
                    }
                    else 
                    { 
                        uri = new Uri(cleanUrl.ToString()); 
                    }

                    UriBuilder uriBuilder = new UriBuilder(uri);
                    uriBuilder.Fragment = string.Empty;

                    cleanUrl.Clear();
                    cleanUrl.Append(uriBuilder.Uri.AbsoluteUri);
                }
                catch (UriFormatException ex)
                {
                    _logger.LogError(ex.Message, url);
                }
            }

            return cleanUrl.ToString();
        }

        private bool IsAbsoluteUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        private Uri GetParentUriString(Uri uri)
        {
            string path = uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length);
            return new Uri(path);
        }
    }
}
