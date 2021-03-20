using System;
using System.Collections.Generic;
using MisterSpider.Configurations;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MisterSpider.Spiders
{
    public class AirbnbSpider : Spider<double>
    {
        private SpiderParams _spiderParams { get { return (SpiderParams)Tag; } }

        public AirbnbSpider(ILogger<AirbnbSpider> logger, INetConnection connection, IOptions<ConfigOptions> config) : base(logger, connection, config)
        {
            Connection = new NetConnectionAirbnb("https://www.airbnb.pt/rooms/5073240", logger, config);
            Urls = new List<string> { string.Format("https://www.airbnb.pt/rooms/ajax_refresh_subtotal?utf8=%E2%9C%93&checkin={1}&checkout={2}&number_of_guests={3}&hosting_id={0}&from_search_checkin={1}&from_search_checkout={2}", "https://www.airbnb.pt/rooms/5073240".Replace("https://www.airbnb.pt/rooms/", string.Empty), _spiderParams.CheckIn, _spiderParams.CheckOut, _spiderParams.Adults, _spiderParams.Currency) };
        }

        protected override double Crawl(Page page)
        {
            double ret = -1;
            try
            {
                double lowprice = double.MaxValue;

                if (page.Url.depth == -1) //root
                {
                    dynamic jsonData = JsonSerializer.Deserialize<dynamic>(page.Source);
                    if (jsonData == null)
                    {
                        return ret;
                    }

                    lowprice = jsonData.total_price_with_fees_and_tax_native.Value;

                    return lowprice;
                }

                return ret;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error get the price on Airbnb", ex);
                return ret;
            }
        }

    }
}
