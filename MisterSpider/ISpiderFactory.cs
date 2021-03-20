using System;

namespace MisterSpider
{
    public interface ISpiderFactory
    {
        ISpider<T> GetSpider<T>(Type spiderType);
    }
}