using System;
using System.Collections.Generic;
using MisterSpider.Extensions;
using MisterSpider.Configurations;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MisterSpider.Spiders
{
    public class BookingPriceSpider : Spider<double>
    {
        public BookingPriceSpider(ILogger<BookingPriceSpider> logger, INetConnection connection, IOptions<ConfigOptions> config, IParallelManager parallelManager, SpiderParams spiderParams) : base(logger, connection, config, parallelManager)
        {
            // Connection = new NetConnectionBooking(logger, config);
            Urls = new List<string> { string.Format("{0}?checkin={1};checkout={2};dist=0;group_adults={3};group_children={4};selected_currency={5}", "http://www.booking.com/hotel/pt/foreign-friend-lisbon.pt-pt.html", spiderParams.CheckIn, spiderParams.CheckOut, spiderParams.Adults, spiderParams.Children, spiderParams.Currency) };
        }

        protected override double Crawl(Page page)
        {
            double ret = -1;
            try
            {
                double lowprice = double.MaxValue;

                if (page.Url.depth == -1) //root
                {
                    var priceRows = page.Document.DocumentNode.SelectNodes("//strong[contains(@class, 'rooms-table-room-price')]");
                    if (priceRows == null)
                    {
                        return ret;
                    }

                    foreach (var row in priceRows)
                    {
                        var val = Regex.Replace(row.Extract("data-price-without-addons"), "[^0-9-.,]", string.Empty).Trim();

                        double price;
                        if (Regex.IsMatch(val, "\\.[0-9]+$"))
                        {
                            price = double.Parse(val, CultureInfo.GetCultureInfo("en-US"));
                        }
                        else if (Regex.IsMatch(val, ",[0-9]+$"))
                        {
                            price = double.Parse(val, CultureInfo.GetCultureInfo("pt-PT"));
                        }
                        else
                        {
                            price = double.Parse(val, CultureInfo.InvariantCulture);
                        }

                        if (lowprice > price)
                        {
                            lowprice = price;
                        }
                    }
                    return lowprice;
                }

                return ret;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error get the price on Booking", ex);
                return ret;
            }
        }

    }
}
