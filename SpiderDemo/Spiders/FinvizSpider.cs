using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MisterSpider;
using MisterSpider.Extensions;
using SpiderDemo.Model;
using System.Collections.Generic;
using System.Text.Json;

namespace SpiderDemo.Spiders
{
    public class FinvizSpider : Spider<Company>
    {
        public Company Company;

        public FinvizSpider(ILogger<Spider<Company>> logger, INetConnection connection, IOptions<ConfigOptions> config, Company spiderParams) : base(logger, connection, config)
        {
            Company = spiderParams;
            Urls = new List<string>
            {
               string.Format("https://finviz.com/quote.ashx?t={0}", spiderParams.Symbol)
            };
        }

        protected override Company Crawl(Page page)
        {
            var htmldoc = page.document.DocumentNode.SelectSingleNode("//div[contains(@class, 'fv-container')]");
            Company.Name = htmldoc.SelectNodes("//table[contains(@class, 'fullview-title')]//a[contains(@class, 'tab-link')]/b").Extract();
            HtmlNodeCollection headerDoc = htmldoc.SelectNodes("//table[contains(@class, 'fullview-title')]//td[contains(@class, 'fullview-links')]/a");
            Company.Sector = headerDoc[0].Extract();
            Company.Industry = headerDoc[1].Extract();

            var htmlsnapshottable2Tds = htmldoc.SelectNodes("//table[contains(@class, 'snapshot-table2')]//td");

            if (htmlsnapshottable2Tds != null)
            {
                for (int i = 0; i < htmlsnapshottable2Tds.Count; i++)
                {
                    HtmlNode td = htmlsnapshottable2Tds[i];

                    if (td.InnerText.Equals("Market Cap"))
                    {
                        Company.MarketCap = htmlsnapshottable2Tds[i + 1].ExtractScrubHtml();
                        continue;
                    }

                    if (td.InnerText.Equals("Shs Outstand"))
                    {
                        Company.SharesOutstanding = htmlsnapshottable2Tds[i + 1].ExtractScrubHtml();
                        continue;
                    }

                    if (td.InnerText.Equals("Price"))
                    {
                        Company.LastClose = htmlsnapshottable2Tds[i + 1].ExtractScrubHtml();
                        continue;
                    }

                    if (td.InnerText.Equals("EPS next Y"))
                    {
                        Company.EpsNextY = htmlsnapshottable2Tds[i + 1].ExtractScrubHtml();
                        continue;
                    }

                    if (td.InnerText.Equals("EPS past 5Y"))
                    {
                        Company.EpsPast5Y = htmlsnapshottable2Tds[i + 1].ExtractScrubHtml();
                        continue;
                    }

                    if (td.InnerText.Equals("EPS next 5Y"))
                    {
                        Company.EpsNext5Y = htmlsnapshottable2Tds[i + 1].ExtractScrubHtml();
                        continue;
                    }

                    if (td.InnerText.Equals("Dividend"))
                    {
                        Company.Dividend = htmlsnapshottable2Tds[i + 1].ExtractScrubHtml();
                        continue;
                    }

                    if (td.InnerText.Equals("Dividend %"))
                    {
                        Company.DividendYield = htmlsnapshottable2Tds[i + 1].ExtractScrubHtml();
                        continue;
                    }
                }
            }
          
            Company.Description = htmldoc.SelectNodes("//td[contains(@class, 'fullview-profile')]").Extract();

            return Company;
        }

    }
}
