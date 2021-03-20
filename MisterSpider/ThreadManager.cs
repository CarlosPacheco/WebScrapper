using Microsoft.Extensions.Options;
using System;
using System.Threading;

namespace MisterSpider
{
    public class ThreadManager : IParallelManager
    {
        private int _itensProcessDoneCount;

        private int _totalItensToProcessCount;
        public int ItensToProcessCount { get { return _totalItensToProcessCount - _itensProcessDoneCount; } }

        public AutoResetEvent AutoEvent { get; set; }

        private ConfigOptions _config;

        public ThreadManager(IOptions<ConfigOptions> config)
        {
            _config = config.Value;
            _totalItensToProcessCount = _itensProcessDoneCount = 0;
            AutoEvent = new AutoResetEvent(false);
            //init threadpool
            ThreadPool.SetMaxThreads(_config.MaximumThreads, _config.MaximumThreads);
        }

        public void AddThreadPool(Action<Url> fetchNewPage, Url url)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(
             (x) =>
             {
                 if (_config.ShouldSleep) Thread.Sleep(IdleTime());
                 fetchNewPage(url);
             }));

            //all done? signal the main thread
            if (_itensProcessDoneCount == _totalItensToProcessCount) lock (AutoEvent) AutoEvent.Set();
        }

        public void AddProcess(Action<Url> fetchNewPage, Url url)
        {
            Interlocked.Increment(ref _totalItensToProcessCount);
            AddThreadPool(fetchNewPage, url);
        }

        public void IncrementItensProcess()
        {
            Interlocked.Increment(ref _itensProcessDoneCount);

            //all done? signal the main thread
            if (_itensProcessDoneCount == _totalItensToProcessCount) lock (AutoEvent) AutoEvent.Set();
        }

        public int IdleTime()
        {
            return new Random().Next(_config.MinThreadIdleTime, _config.MaxThreadIdleTime + 1);
        }

        public void StartWait()
        {
            AutoEvent.WaitOne();
        }
    }
}
