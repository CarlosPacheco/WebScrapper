using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SpiderDemo.Model
{
    public class Company
    {
        public string Name { get; set; }

        [JsonPropertyName("ticker")]
        public string Symbol { get; set; }

        public string Description { get; set; }

        public string Industry { get; set; }

        public string Sector { get; set; }

        public string Exchange { get; set; }

        public string MarketCap { get; set; }

        public string SharesOutstanding { get; set; }

        public string LastClose { get; set; }

        public string EpsNextY { get; set; }

        public string EpsPast5Y { get; set; }

        public string EpsNext5Y { get; set; }

        public string Currency { get; set; }

        public string Dividend { get; set; }

        public string DividendYield { get; set; }

        /// <summary>
        /// The month the Fiscal year ends
        /// </summary>
        public string FiscalYearEnds { get; set; }

        public IList<IncomeStatement> IncomeStatement { get; set; } = new List<IncomeStatement>();

        public IList<BalanceSheet> BalanceSheet { get; set; } = new List<BalanceSheet>();

        public IList<CashFlow> CashFlow { get; set; } = new List<CashFlow>();

        public IList<FinancialRatios> FinancialRatios { get; set; } = new List<FinancialRatios>();
    }
}
