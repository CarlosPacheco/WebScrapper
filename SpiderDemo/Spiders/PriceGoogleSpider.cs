using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using MisterSpider;
using MisterSpider.Extensions;
using SpiderDemo.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace SpiderDemo.Spiders
{
    public class PriceGoogleSpider : Spider<Company>
    {
        public Company Company;

        public PriceGoogleSpider(ILogger<Spider<Company>> logger, INetConnection connection, IOptions<ConfigOptions> config, IParallelManager parallelManager, Company spiderParams) : base(logger, connection, config, parallelManager)
        {
            Company = spiderParams;
            Urls = new List<string>
            {
                string.Format("http://performance.morningstar.com/perform/Performance/stock/quote-data-strip.action?t={0}&region=usa&culture=en-US", spiderParams.Symbol)
            };
        }

        protected override Company Crawl(Page page)
        {
            HtmlNode htmldoc = page.Document.DocumentNode.SelectSingleNode("//span[contains(@id, 'last-price-value')]");

            if (htmldoc == null) return Company;

            Company.LastClose = GetDouble(htmldoc.Extract()).ToString();

            return Company;
        }

        private double? GetDouble(string number)
        {
            if (string.IsNullOrWhiteSpace(number) || "-".Equals(number)) return null;

            return double.Parse(number, NumberStyles.Currency, CultureInfo.InvariantCulture);
        }

       
    }

}
