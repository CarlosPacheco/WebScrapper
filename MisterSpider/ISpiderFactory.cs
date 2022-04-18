using System;
using System.Threading;

namespace MisterSpider
{
    public interface ISpiderFactory
    {
        ISpider<T> GetSpider<T>(Type spiderType, CancellationToken cancellationToken);

        ISpider<T> GetSpider<T>(Type spiderType, CancellationToken cancellationToken, params object[] parameters);
    }
}