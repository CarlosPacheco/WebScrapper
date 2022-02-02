using System;
using System.Collections.Generic;

namespace MisterSpider
{
    public interface IWebScrapperManager
    {
        IList<T> Start<T>(List<string> classTypes);

        ISpider<T> Start<T>(string classType, params object[] parameters);

        ISpider<T> Start<T>(Type classType, params object[] parameters);

        void Start(List<string> classTypes, TimeSpan sleepTime);

        IList<SpiderConfiguration> Start<T>(IList<SpiderConfiguration> spiderConfigs);
    }
}
