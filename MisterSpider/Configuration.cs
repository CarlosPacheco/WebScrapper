using System;
using System.Collections.Generic;

namespace MisterSpider
{
    public class SpiderConfiguration
    {
        public Type SpiderType { get; set; }

        public object[] Parameters { get; set; }

        public List<object> ExtractData { get; set; }
    }
}
