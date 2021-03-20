using System;

namespace MisterSpider.Configurations
{
    public class SpiderParams
    {
        public string ScrapperURL { get; set; }

        public DateTime CheckIn { get; set; }

        public DateTime CheckOut { get; set; }

        public string Nights { get; set; }

        public string Adults { get; set; }

        public string Children { get; set; }

        public string Currency { get; set; }
    }
}
