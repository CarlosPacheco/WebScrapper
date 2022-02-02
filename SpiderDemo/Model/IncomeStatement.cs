using System;

namespace SpiderDemo.Model
{
    public class IncomeStatement
    {
        public DateTime Year { get; set; }
        
        public double Revenue { get; set; }

        public double CostOfRevenue { get; set; }

        public double GrossProfit { get; set; }

        /// <summary>
        /// Research and development
        /// </summary>
        public double ResearchAndDevelopment { get; set; }

        /// <summary>
        /// Operating expenses
        /// Sales, General and administrative
        /// </summary>
        public double SalesGeneralAdministrative { get; set; }

        /// <summary>
        /// Restructuring, merger and acquisition
        /// </summary>
        public double RestructuringMergerAndAcquisition { get; set; }

        /// <summary>
        /// Operating expenses
        /// Other operating expenses
        /// </summary>
        public double OtherOperatingExpenses { get; set; }

        /// <summary>
        /// Operating expenses
        /// Total operating expenses
        /// </summary>
        public double TotalOperatingExpenses { get; set; }

        public double OperatingIncome { get; set; }

        public double InterestExpense { get; set; }

        /// <summary>
        /// Other income (expense)
        /// </summary>
        public double OtherIncomeExpense { get; set; }

        /// <summary>
        /// Income before taxes
        /// </summary>
        public double IncomeBeforeTaxes { get; set; }

        /// <summary>
        /// Provision for income taxes
        /// </summary>
        public double ProvisionForIncomeTaxes { get; set; }

        /// <summary>
        /// Other income
        /// </summary>
        public double OtherIncome { get; set; }

        /// <summary>
        /// Net income from continuing operations
        /// </summary>
        public double NetIncomeFromContinuingOperations { get; set; }

        /// <summary>
        /// Other
        /// </summary>
        public double Other { get; set; }

        /// <summary>
        /// Net income
        /// </summary>
        public double NetIncome { get; set; }

        /// <summary>
        /// Preferred dividend
        /// </summary>
        public double PreferredDividend { get; set; }

        /// <summary>
        /// Net income available to common shareholders
        /// </summary>
        public double NetIncomeAvailableToCommonShareholders { get; set; }

        /// <summary>
        /// EPS - Earnings per share (Diluted)
        /// </summary>
        public double EarningsPerShare { get; set; }

        /// <summary>
        /// Weighted average shares outstanding (Diluted) in Mil
        /// </summary>
        public double Shares { get; set; }

        public double Ebitda { get; set; }

    }
}