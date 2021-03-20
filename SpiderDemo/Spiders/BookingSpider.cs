using System.Collections.Generic;
using System.Text.RegularExpressions;
using MisterSpider.Model;
using MisterSpider.Extensions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MisterSpider.Spiders
{
    public class BookingSpider : Spider<BookingItem>
    {
        public BookingSpider(ILogger<BookingSpider> logger, INetConnection connection, IOptions<ConfigOptions> config) : base(logger, connection, config)
        {
            Urls = new List<string> { "http://www.booking.com/hotel/pt/pestanaportohotel.en-gb.html" };
        }

        protected override BookingItem Crawl(Page page)
        {
            var htmldoc = page.document.DocumentNode.SelectSingleNode("//div[contains(@id, 'blockdisplay1')]");

            BookingItem item = new BookingItem();

            item.Title = htmldoc.SelectSingleNode("//h1/span[contains(@id, 'hp_hotel_name')]").ExtractScrubHtml();
            item.Location = htmldoc.SelectSingleNode("//p/span[contains(@class, 'hp_address_subtitle')]").ExtractScrubHtml();
            item.Stars = htmldoc.SelectSingleNode("//h1/span[contains(@class, 'hp__hotel_ratings')]/span//i").Extract("title");
            item.Rating = htmldoc.SelectSingleNode(".//a[@class='big_review_score_detailed js-big_review_score_detailed ind_rev_total hp_review_score']/span/span[contains(@class, 'average')]").Extract();
            item.Description = htmldoc.SelectNodes(".//div[contains(@class, 'hotel_description_wrapper_exp')]/div/p").ExtractScrubHtml();

            Match match = Regex.Match(htmldoc.SelectSingleNode("//a[contains(@class, 'map_static_zoom')]").Extract("style"), @"-?(\d+(\.\d+)?)+(,)-?(\d+(\.\d+)?)");
            if (match.Success)
            {
                string[] coor = match.Value.Split(',');
                item.Latitude = coor[0];
                item.Longitude = coor[1];
            }
            
            HtmlNodeCollection hnc = htmldoc.SelectNodes(".//div[contains(@class, 'facilitiesChecklistSection')]");
           
            if (hnc != null)
            {
                foreach (HtmlNode hn in hnc)
                {
                    if (hn.InnerHtml.Contains("Languages spoken"))
                    {
                        continue;
                    }

                    HtmlNodeCollection hncList = hn.SelectNodes(".//ul/li");

                    foreach (HtmlNode hnfacilities in hncList)
                    {
                        item.Facilities.Add(hnfacilities.ExtractScrubHtml());
                    }                   
                }
            }

            item.HotelUrl = page.Url.uri.AbsoluteUri.ToLower();

            hnc = htmldoc.SelectNodes(".//div[contains(@id, 'photos_distinct')]/a");
           
            if (hnc != null)
            {
                foreach (HtmlNode hn in hnc)
                {
                    HtmlAttribute attri = hn.Attributes["href"];
                    if (attri != null)
                    {
                        if (attri.Value.Contains("hotel"))
                        {
                            item.Images.Add(attri.Value.Replace("max400", "max600"));
                        }
                    }
                }
            }

            return item;
        }

    }
}
