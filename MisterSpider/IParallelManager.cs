using System;

namespace MisterSpider
{
    public interface IParallelManager
    {
        int ItensToProcessCount { get; }

        void IncrementItensProcess();

        void AddThreadPool(Action<Url> fetchNewPage, Url url);

        void AddProcess(Action<Url> fetchNewPage, Url url);

        void StartWait();
    }
}
