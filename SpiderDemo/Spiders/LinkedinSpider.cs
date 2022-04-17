using System;
using System.Collections.Generic;
using System.Threading;
using MisterSpider.Model;
using MisterSpider.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MisterSpider.Spiders
{
    public class LinkedinSpider : Spider<LinkedinItem>
    {
        private string _searchWord { get; set; }

        private int _count;

        public LinkedinSpider(ILogger<LinkedinSpider> logger, INetConnection connection, IOptions<ConfigOptions> config, IParallelManager parallelManager) : base(logger, connection, config, parallelManager)
        {
            _searchWord = "hotel teatro porto";
            // Connection = new NetConnectionLinkedin(logger, config);
            Urls = new List<string> { string.Format("https://www.linkedin.com/vsearch/p?type=people&keywords={0}", _searchWord) };
        }

        protected override LinkedinItem Crawl(Page page)
        {
            if (page.Url.depth == -1) //root
            {
                var htmldoc = page.Document.DocumentNode.SelectSingleNode("//code[contains(@id, 'voltron_srp_main-content')]").ExtractDecode(false);
                //remove <!-- -->
                htmldoc = htmldoc.Remove(0, 4);
                htmldoc = htmldoc.Remove(htmldoc.Length - 3, 3);

                dynamic jsonData = JsonSerializer.Deserialize<dynamic>(htmldoc);

                var root = jsonData.content.page.voltron_unified_search_json.search;
                var results = root.results;

                foreach (var item in results)
                {
                    var description = HtmlAgilityPackExtensions.RemoveHtmlTags(item.person.fmt_headline.Value);

                    if (CheckString(description, _searchWord))
                    {
                        //add the details itens
                        AddProcess(item.person.link_nprofile_view_headless.Value, page);
                    }
                }

                //we only run the roots pages 1 by 1 so we dont need have lock the count
                if (_count <= 10)// crawl 10 pagination pages
                {
                    Interlocked.Increment(ref _count);
                    //add the pagination url
                    AddProcess(page.CleanUrl(root.baseData.resultPagination.nextPage.pageURL.Value));
                }
                return null;
            }

            return parse_details(page);

        }

        private LinkedinItem parse_details(Page page)
        {
            LinkedinItem item = new LinkedinItem();

            var htmldoc = page.Document.DocumentNode.SelectSingleNode("//div[contains(@id, 'top-card')]");

            item.Name = htmldoc.SelectSingleNode(".//div[contains(@id, 'name')]//span[contains(@class, 'full-name')]").Extract();
            item.Description = HtmlAgilityPackExtensions.RemoveHtmlTags(htmldoc.SelectSingleNode(".//div[contains(@id, 'headline')]/p[contains(@class, 'title')]").Extract());
            item.Image = htmldoc.SelectSingleNode(".//div[contains(@class, 'profile-picture')]//img").Extract("src");
            item.Email = htmldoc.SelectSingleNode(".//div[contains(@id, 'email')]//ul//li").Extract();
            item.Phone = htmldoc.SelectNodes(".//div[contains(@id, 'phone')]//ul//li").Extract();

            return item;
        }

        private bool CheckString(string source, string toCheck)
        {
            var list = toCheck.Split(' ');
            if (list.Length == 0)
            {
                return (source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            foreach (var item in list)
            {
                if (!(source.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
