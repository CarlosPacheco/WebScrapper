using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MisterSpider
{
    public class WebScrapperManager : IWebScrapperManager, IDisposable
    {
        private ILogger<WebScrapperManager> _logger { get; }
        private ISpiderFactory _spiderFactory { get; }
        private IList<Thread> _threads = new List<Thread>();

        public WebScrapperManager(ILogger<WebScrapperManager> logger, ISpiderFactory spiderFactory)
        {
            _logger = logger;
            _spiderFactory = spiderFactory;
        }

        public IList<T> Start<T>(List<string> classTypes)
        {

            IList<ISpider<T>> spiders = new List<ISpider<T>>();

            foreach (string classType in classTypes)
            {
                ISpider<T> spider = _spiderFactory.GetSpider<T>(Type.GetType(classType));

                //add the new spider
                spiders.Add(spider);

                Thread thread = new Thread(new ThreadStart(spider.Go));
                _threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in _threads)
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

        public ISpider<T> Start<T>(string classType, params object[] parameters)
        {
            return Start<T>(Type.GetType(classType), parameters);
        }

        public ISpider<T> Start<T>(Type classType, params object[] parameters)
        {
            IList<ISpider<T>> spiders = new List<ISpider<T>>();

            ISpider<T> spider = _spiderFactory.GetSpider<T>(classType, parameters);

            Thread thread = new Thread(new ThreadStart(spider.Go));
            _threads.Add(thread);
            thread.Start();
            thread.Join();

            _logger.LogDebug("Spider finished.");
            return spider;
        }

        public void Start(List<string> classTypes, TimeSpan sleepTime)
        {
            while (true)
            {
                Start<object>(classTypes);
                Thread.Sleep(sleepTime);
            }
        }

        public IList<SpiderConfiguration> Start<T>(IList<SpiderConfiguration> spiderConfigs)
        {
            if (spiderConfigs == null)
            {
                throw new ArgumentNullException("Configuration parameter is null.");
            }

            foreach (SpiderConfiguration spiderCofig in spiderConfigs)
            {
                ISpider<T> spider = Start<T>(spiderCofig.SpiderType, spiderCofig.Parameters);
                spiderCofig.ExtractData.AddRange((IEnumerable<object>)spider.ExtractData);
            }

            _logger.LogDebug("Spider finished.");

            return spiderConfigs;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Thread thread in _threads)
                {
                    if (thread.IsAlive)
                        thread.Interrupt();
                }
            }
        }
    }
}
