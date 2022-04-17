using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MisterSpider;
using SpiderDemo.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SpiderDemo.Spiders
{
    public class CurrencyfreaksSpider : Spider<IList<Company>>
    {
        public CurrencyfreaksSpider(ILogger<Spider<IList<Company>>> logger, INetConnection connection, IOptions<ConfigOptions> config, IParallelManager parallelManager) : base(logger, connection, config, parallelManager)
        {
            Urls = new List<string>
            {
               string.Format("https://api.currencyfreaks.com/latest?apikey={0}", "c0df19e7abca48ddaa6863b2969a85c7")
            };
        }

        protected override IList<Company> Crawl(Page page)
        {
            JsonNode obj = JsonNode.Parse(page.Source);
            JsonObject rates = obj["rates"].AsObject();

            foreach (KeyValuePair<string, JsonNode> property in rates)
            {
                var code = property.Key;
                var value = double.Parse((string)property.Value, NumberStyles.Currency, CultureInfo.InvariantCulture);
            }

        
           // JsonElement jsonDoc = JsonElement.ParseValue(page.Source);
          // var x = jsonDoc[""];
            return JsonSerializer.Deserialize<dynamic>(page.Source, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }

    }
}
