using System;

namespace SpiderDemo.Model
{
    public class BalanceSheet
    {
        public DateTime Year { get; set; }

        #region Assets

        /// <summary>
        /// Cash and cash equivalents
        /// </summary>
        public double CashAndCashEquivalents { get; set; }

        /// <summary>
        /// Cash Short-term investments
        /// </summary>
        public double ShortTermInvestments { get; set; }

        public double TotalCash { get; set; }

        public double Receivables { get; set; }

        public double Inventories { get; set; }

        /// <summary>
        /// Deferred income taxes (CurrentAssets)
        /// </summary>
        public double DeferredIncomeTaxesCurrentAssets { get; set; }

        /// <summary>
        /// Prepaid expenses
        /// </summary>
        public double PrepaidExpenses { get; set; }

        /// <summary>
        /// Other current assets
        /// </summary>
        public double OtherCurrentAssets { get; set; }

        public double TotalCurrentAssets { get; set; }

        /// <summary>
        /// Gross property, plant and equipment
        /// </summary>
        public double GrossPropertyPlantEquipment { get; set; }

        /// <summary>
        /// Accumulated Depreciation
        /// </summary>
        public double AccumulatedDepreciation { get; set; }

        /// <summary>
        /// Net property, plant and equipment
        /// </summary>
        public double NetPropertyPlantEquipment { get; set; }

        /// <summary>
        /// Equity and other investments
        /// </summary>
        public double EquityAndOtherInvestments { get; set; }

        public double Goodwill { get; set; }

        public double IntangibleAssets { get; set; }

        /// <summary>
        /// Deferred income taxes (non-CurrentAssets)
        /// </summary>
        public double DeferredIncomeTaxesNonCurrentAssets { get; set; }

        public double OtherLongTermAssets { get; set; }

        public double TotalNonCurrentAssets { get; set; }

        public double TotalAssets { get; set; }

        #endregion

        #region Liabilities and stockholders equity

        /// <summary>
        /// Short-term debt
        /// </summary>
        public double ShortTermDebt { get; set; }

        /// <summary>
        /// Capital leases (currentLiabilities)
        /// </summary>
        public double CapitalLeasesCurrentLiabilities { get; set; }

        /// <summary>
        /// Taxes payable
        /// </summary>
        public double TaxesPayable { get; set; }

        public double AccountsPayable { get; set; }

        public double AccruedLiabilities { get; set; }

        /// <summary>
        /// Deferred revenues (currentLiabilities)
        /// </summary>
        public double DeferredRevenuesCurrentLiabilities { get; set; }

        /// <summary>
        /// Other current liabilities
        /// </summary>
        public double OtherCurrentLiabilities { get; set; }

        public double TotalCurrentLiabilities { get; set; }

        public double LongTermDebt { get; set; }

        /// <summary>
        /// Capital leases (non-currentLiabilities)
        /// </summary>
        public double CapitalLeasesNonCurrentLiabilities { get; set; }

        public double DeferredTaxesLiabilities { get; set; }

        /// <summary>
        /// Deferred revenues (non-currentLiabilities)
        /// </summary>
        public double DeferredRevenuesNonCurrentLiabilities { get; set; }
        
        /// <summary>
        /// Minority interest
        /// </summary>
        public double MinorityInterest { get; set; }

        public double OtherLongTermLiabilities { get; set; }

        public double TotalNonCurrentLiabilities { get; set; }

        public double TotalLiabilities { get; set; }

        public double CommonStock { get; set; }

        /// <summary>
        /// Other Equity
        /// </summary>
        public double OtherEquity { get; set; }

        public double AdditionalPaidInCapital { get; set; }

        public double RetainedEarnings { get; set; }

        public double TreasuryStock { get; set; }

        public double AccumulatedOtherComprehensiveIncome { get; set; }

        /// <summary>
        /// Total stockholders' equity
        /// </summary>
        public double TotalStockholdersEquity { get; set; }

        /// <summary>
        /// Total liabilities and stockholders' equity
        /// </summary>
        public double TotalLiabilitiesStockholdersEquity { get; set; }

        #endregion
    }
}