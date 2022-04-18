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

        public bool IsCompleted { get { return _itensProcessDoneCount == _totalItensToProcessCount; } }

        public AutoResetEvent AutoEvent { get; set; }

        public CancellationToken CancellationToken { get; set; }

        private ConfigOptions _config;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ThreadManager(IOptions<ConfigOptions> config)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _config = config.Value;
            _totalItensToProcessCount = _itensProcessDoneCount = 0;
            AutoEvent = new AutoResetEvent(false);
            //init threadpool
            ThreadPool.SetMaxThreads(_config.MaximumThreads, _config.MaximumThreads);
        }

        /// <summary>
        /// Add new item to process without increment the itens to process
        /// use this if you want to reprocess a item
        /// </summary>
        /// <param name="fetchNewPage"></param>
        /// <param name="url"></param>
        protected void AddThreadPool(Action<Url> fetchNewPage, Url url)
        {
            if (CancellationToken.IsCancellationRequested) return;

            ThreadPool.QueueUserWorkItem(new WaitCallback(
            (x) =>
            {
                CancellationToken token = (CancellationToken)x!;
                if (!token.IsCancellationRequested && _config.ShouldSleep) Thread.Sleep(IdleTime());
                if (token.IsCancellationRequested)
                {
                    lock (AutoEvent) AutoEvent.Set();
                    return;
                }

                fetchNewPage(url);
            }), CancellationToken);

            //all done? signal the main thread
            if (IsCompleted) lock (AutoEvent) AutoEvent.Set();
        }

        /// <summary>
        /// Add new item to process, this will increment the itens count to process
        /// </summary>
        /// <param name="fetchNewPage"></param>
        /// <param name="url"></param>
        public void Add(Action<Url> fetchNewPage, Url url)
        {
            Interlocked.Increment(ref _totalItensToProcessCount);
            AddThreadPool(fetchNewPage, url);
        }

        /// <summary>
        /// Add new item to process, this NOT increment the itens count to process
        /// </summary>
        /// <param name="fetchNewPage"></param>
        /// <param name="url"></param>
        public void Retry(Action<Url> fetchNewPage, Url url)
        {
            AddThreadPool(fetchNewPage, url);
        }

        public void IncrementItensProcess()
        {
            Interlocked.Increment(ref _itensProcessDoneCount);

            //all done? signal the main thread
            if (IsCompleted) lock (AutoEvent) AutoEvent.Set();
        }

        public int IdleTime()
        {
            return new Random().Next(_config.MinThreadIdleTime, _config.MaxThreadIdleTime + 1);
        }

        /// <summary>
        /// Wait until all itens are completed
        /// </summary>
        public void StartWait()
        {
            AutoEvent.WaitOne();
        }
    }
}
