using System;
using System.Collections.Concurrent;

namespace MisterSpider
{
    public interface ISpider<T> : ISpider
    {
        public ConcurrentBag<T> ExtractData { get; }

        void Go();

        Action OnSpiderError { get; }

        UrlProcessError OnUrlProcessError { get; }
    }

    public delegate void UrlProcessError(Url url);

    public interface ISpider : IDisposable
    { 
    }

}