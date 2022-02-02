using System.Collections.Concurrent;

namespace MisterSpider
{
    public interface ISpider<T>
    {
        public ConcurrentBag<T> ExtractData { get; }

        void Go();
    }
}