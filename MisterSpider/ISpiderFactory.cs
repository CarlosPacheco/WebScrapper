using System;

namespace MisterSpider
{
    public interface ISpiderFactory
    {
        ISpider<T> GetSpider<T>(Type spiderType);

        ISpider<T> GetSpider<T>(Type spiderType, params object[] parameters);
    }
}