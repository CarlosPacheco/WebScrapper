using System;
using System.Threading;

namespace MisterSpider
{
    public interface ISpiderFactory
    {
        ISpider<T> GetSpider<T>(Type spiderType, CancellationTokenSource cancellationToken);

        ISpider<T> GetSpider<T>(Type spiderType, CancellationTokenSource cancellationToken, params object[] parameters);
    }
}