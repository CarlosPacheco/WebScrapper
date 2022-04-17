using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MisterSpider
{
    public class WebScrapperManager : IWebScrapperManager, IDisposable
    {
        private bool disposedValue;

        private ILogger<WebScrapperManager> _logger { get; }
        private ISpiderFactory _spiderFactory { get; }
        private IList<Thread> _threads { get; }
        private IList<ISpider> _spiders { get; }

        // TODO:: add this in all the  start methods
        //public readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public WebScrapperManager(ILogger<WebScrapperManager> logger, ISpiderFactory spiderFactory)
        {
            _logger = logger;
            _spiderFactory = spiderFactory;
            _threads = new List<Thread>();
            _spiders = new List<ISpider>();
        }

        /// <summary>
        /// Concurrent methos, each spider will run inside a thread
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classTypes"></param>
        /// <returns></returns>
        public IList<T> StartConcurrent<T>(List<string> classTypes)
        {
            if (disposedValue) return null;
            foreach (string classType in classTypes)
            {
                ISpider<T> spider = _spiderFactory.GetSpider<T>(Type.GetType(classType));

                //add the new spider
                _spiders.Add(spider);

                Thread thread = new Thread(new ThreadStart(spider.Go));
                _threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in _threads)
            {
                thread.Join();
            }

            List<T> data = new List<T>();
            foreach (ISpider<T> spider in _spiders)
            {
                data.AddRange(spider.ExtractData);
            }

            _logger.LogDebug("Spider finished.");
            return data;
        }

        public T StartSingle<T>(string classType, params object[] parameters)
        {
            return StartSingle<T>(Type.GetType(classType), parameters);
        }

        public T StartSingle<T>(Type classType, params object[] parameters)
        {
            if (disposedValue) return default;
            ISpider<T> spider = _spiderFactory.GetSpider<T>(classType, parameters);
            //add the new spider
            _spiders.Add(spider);
            spider.Go();

            spider.ExtractData.TryTake(out T spiderData);
            //add the new spider
            lock (_spiders)
            {
                _spiders.Remove(spider);
                spider.Dispose();
            }

            _logger.LogDebug("Spider finished.");
            return spiderData;
        }

        public IList<T> StartSingleList<T>(Type classType, params object[] parameters)
        {
            if (disposedValue) return default;
            ISpider<T> spider = _spiderFactory.GetSpider<T>(classType, parameters);
            //add the new spider
            _spiders.Add(spider);
            spider.Go();

            List<T> spiderData = new List<T>();
            spiderData.AddRange(spiderData);

            //add the new spider
            lock (_spiders)
            {
                _spiders.Remove(spider);
                spider.Dispose();
            }

            _logger.LogDebug("Spider finished.");
            return spiderData;
        }

        /// <summary>
        /// Concurrent methos, each spider will run inside a thread
        /// </summary>
        /// <param name="classTypes"></param>
        /// <param name="sleepTime"></param>
        public void StartConcurrent(List<string> classTypes, TimeSpan sleepTime)
        {
            while (true)
            {
                StartConcurrent<object>(classTypes);
                Thread.Sleep(sleepTime);
            }
        }

        public IList<SpiderConfiguration<T>> Start<T>(IList<SpiderConfiguration<T>> spiderConfigs)
        {
            if (spiderConfigs == null)
            {
                throw new ArgumentNullException("Configuration parameter is null.");
            }

            foreach (SpiderConfiguration<T> spiderCofig in spiderConfigs)
            {
                spiderCofig.ExtractData = StartSingle<T>(spiderCofig.SpiderType, spiderCofig.Parameters);
            }

            _logger.LogDebug("Spider finished.");

            return spiderConfigs;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (ISpider spider in _spiders)
                    {
                        spider.Dispose();
                    }

                    foreach (Thread thread in _threads)
                    {
                        if (thread.IsAlive)
                        {
                            thread.Interrupt();
                            thread.Join(2000);// wait max 2 secs
                        }
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WebScrapperManager()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
