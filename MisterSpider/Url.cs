using System;

namespace MisterSpider
{
    public enum UrlStatus
    {
        None,
        Queue,
        Process,
        Completed,
        Retry
    }

    public class Url
    {
        public Uri uri { get; set; }
        public int depth { get; set; }

        public UrlStatus status { get; set; }

        public string link { get; set; }

        public Url(Uri u, int d, UrlStatus s)
        {
            uri = u;
            depth = d;
            status = s;
            link = uri.AbsoluteUri.ToLower();
        }
    }
}
