using System;
using System.Collections.Generic;

namespace MisterSpider
{
    public class SpiderConfiguration<T>
    {
        public Type SpiderType { get; set; }

        public object[] Parameters { get; set; }

        public T ExtractData { get; set; }
    }
}
