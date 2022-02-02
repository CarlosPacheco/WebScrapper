using System;

namespace SpiderDemo.Model
{
    public class FinancialRatios
    {
        public DateTime Year { get; set; }

        /// <summary>
        /// Revenue
        /// </summary>
        public double Revenue { get; set; }

        /// <summary>
        /// Earnings Per Share (EPS)
        /// </summary>
        public double EarningsPerShare { get; set; }

        /// <summary>
        /// Book Value Per Share (BVPS)
        /// </summary>
        public double BookValuePerShare { get; set; }

        /// <summary>
        /// Free Cash Flow
        /// </summary>
        public double FreeCashFlow { get; set; }

        /// <summary>
        /// Return on Invested Capital % (ROIC)
        /// </summary>
        public double ReturnOnInvestedCapital { get; set; }
    }
}