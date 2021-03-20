using System;

namespace MisterSpider
{
    public class Url
    {
        public Uri uri { get; set; }
        public int depth { get; set; }

        public Url(Uri u, int d)
        {
            uri = u;
            depth = d;
        }
    }
}
