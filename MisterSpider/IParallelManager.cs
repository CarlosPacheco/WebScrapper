using System;

namespace MisterSpider
{
    public interface IParallelManager : IDisposable
    {
        int ItensToProcessCount { get; }

        public bool IsCompleted { get; }

        void IncrementItensProcess();

        /// <summary>
        /// Add new item to process, this will increment the itens count to process
        /// </summary>
        /// <param name="fetchNewPage"></param>
        /// <param name="url"></param>
        void Add(Action<Url> fetchNewPage, Url url);

        /// <summary>
        /// Add new item to process, this NOT increment the itens count to process
        /// </summary>
        /// <param name="fetchNewPage"></param>
        /// <param name="url"></param>
        void Retry(Action<Url> fetchNewPage, Url url);

        /// <summary>
        /// Wait until all itens are completed
        /// </summary>
        void StartWait();
    }
}
