using System;
using System.Collections.Generic;

namespace MisterSpider
{
    public interface IWebScrapperManager
    {
        List<T> Start<T>(Dictionary<string, string> scrappersURLs, object tag = null);
        List<T> Start<T>(List<string> classTypes, object tag = null);
        void Start(List<string> classTypes, TimeSpan sleepTime);
    }
}
