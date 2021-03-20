using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MisterSpider
{
    public class WebScrapperManager : IWebScrapperManager
    {
        private ILogger<WebScrapperManager> _logger { get; }
        private ISpiderFactory _spiderFactory { get; }
        public WebScrapperManager(ILogger<WebScrapperManager> logger, ISpiderFactory spiderFactory)
        {
            _logger = logger;
            _spiderFactory = spiderFactory;
        }

        public List<T> Start<T>(Dictionary<string, string> scrappersURLs, object tag = null)
        {
            List<string> spiderTypes = scrappersURLs.Keys.ToList();
            return Start<T>(spiderTypes, tag);
        }

        public List<T> Start<T>(List<string> classTypes, object tag = null)
        {
            List<Thread> threads = new List<Thread>();
            List<ISpider<T>> spiders = new List<ISpider<T>>();

            foreach (string classType in classTypes)
            {
                ISpider<T> spider = _spiderFactory.GetSpider<T>(Type.GetType(classType));
                spider.Tag = tag;

                //add the new spider
                spiders.Add(spider);

                Thread thread = new Thread(new ThreadStart(spider.Go));
                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            List<T> data = new List<T>();
            foreach (ISpider<T> spider in spiders)
            {
                data.AddRange(spider.ExtractData);
            }

            _logger.LogDebug("Spider finished.");
            return data;
        }

        public void Start(List<string> classTypes, TimeSpan sleepTime)
        {
            while (true)
            {
                Start<object>(classTypes);
                Thread.Sleep(sleepTime);
            }
        }
    }
}
