using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MisterSpider;
using SpiderDemo.Model;
using System.Collections.Generic;
using System.Text.Json;

namespace SpiderDemo.Spiders
{
    public class DumbstockapiSpider : Spider<IList<Company>>
    {
        public DumbstockapiSpider(ILogger<Spider<IList<Company>>> logger, INetConnection connection, IOptions<ConfigOptions> config, IParallelManager parallelManager) : base(logger, connection, config, parallelManager)
        {
            Urls = new List<string>
            {
               string.Format("https://dumbstockapi.com/stock?countries={0}", "US")
            };
        }

        protected override IList<Company> Crawl(Page page)
        {
            return JsonSerializer.Deserialize<List<Company>>(page.Source, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }

    }
}
