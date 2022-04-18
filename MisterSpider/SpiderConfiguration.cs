using System;

namespace MisterSpider
{
    public class SpiderConfiguration<T>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Type SpiderType { get; set; }

        public object[] Parameters { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public T? ExtractData { get; set; }
    }
}
