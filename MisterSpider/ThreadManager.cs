using Microsoft.Extensions.Options;
using System;
using System.Threading;

namespace MisterSpider
{
    public class ThreadManager : IParallelManager
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private int _itensProcessDoneCount;

        private int _totalItensToProcessCount;
        public int ItensToProcessCount { get { return _totalItensToProcessCount - _itensProcessDoneCount; } }

        public bool IsCompleted { get { return _itensProcessDoneCount == _totalItensToProcessCount; } }

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

        /// <summary>
        /// Add new item to process without increment the itens to process
        /// use this if you want to reprocess a item
        /// </summary>
        /// <param name="fetchNewPage"></param>
        /// <param name="url"></param>
        protected void AddThreadPool(Action<Url> fetchNewPage, Url url)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(
             (x) =>
             {
                 CancellationToken token = (CancellationToken)x;
                 if (token.IsCancellationRequested) token.ThrowIfCancellationRequested();

                 if (_config.ShouldSleep) Thread.Sleep(IdleTime());
                 fetchNewPage(url);
             }), _cancellationTokenSource.Token);

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsCompleted)
                {
                    _cancellationTokenSource.Cancel();
                    AutoEvent.WaitOne(5000);// wait max 5secs
                    _cancellationTokenSource.Dispose();
                }

                AutoEvent.Dispose();
            }
        }
    }
}
