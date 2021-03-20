using System.Collections.Concurrent;

namespace MisterSpider
{
    public interface ISpider<T>
    {
        public object Tag { get; set; }

        public ConcurrentBag<T> ExtractData { get; }

        void Go();

        void Go(object tag);
    }
}