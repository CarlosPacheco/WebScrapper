using System;

namespace SpiderDemo.Model
{
    public class CashFlow
    {
        public DateTime Year { get; set; }

        /// <summary>
        /// Cash Flows From Operating Activities
        /// Net income
        /// </summary>
        public double NetIncome { get; set; }

        /// <summary>
        /// Cash Flows From Operating Activities
        /// Depreciation & amortization
        /// </summary>
        public double DepreciationAmortization { get; set; }

        /// <summary>
        /// Investment/asset impairment charges
        /// </summary>
        public double InvestmentAassetImpairmentCharges { get; set; }

        /// <summary>
        /// Investments losses (gains)
        /// </summary>
        public double InvestmentsLosses { get; set; }

        /// <summary>
        /// Cash Flows From Operating Activities
        /// Deferred income taxes
        /// </summary>
        public double DeferredIncomeTaxes { get; set; }

        /// <summary>
        /// Prepaid expenses
        /// </summary>
        public double PrepaidExpenses { get; set; }

        /// <summary>
        /// Income taxes payable
        /// </summary>
        public double IncomeTaxesPayable { get; set; }

        /// <summary>
        /// Cash Flows From Operating Activities
        /// Stock based compensation
        /// </summary>
        public double StockBasedCompensation { get; set; }

        /// <summary>
        /// Cash Flows From Operating Activities
        /// Change in working capital
        /// </summary>
        public double ChangeInWorkingCapital { get; set; }

        /// <summary>
        /// Cash Flows From Operating Activities
        /// Accounts receivable
        /// </summary>
        public double AccountsReceivable { get; set; }

        /// <summary>
        /// Cash Flows From Operating Activities
        /// Inventory
        /// </summary>
        public double Inventory { get; set; }

        /// <summary>
        /// Cash Flows From Operating Activities
        /// Accounts payable
        /// </summary>
        public double AccountsPayable { get; set; }

        /// <summary>
        /// Cash Flows From Operating Activities
        /// Accrued liabilities
        /// </summary>
        public double AccruedLiabilities { get; set; }

        /// <summary>
        /// Cash Flows From Operating Activities
        /// Other working capital
        /// </summary>
        public double OtherWorkingCapital { get; set; }

        /// <summary>
        /// Cash Flows From Operating Activities
        /// Other non-cash items
        /// </summary>
        public double OtherNonCashItems { get; set; }

        /// <summary>
        /// Cash Flows From Operating Activities
        /// Net cash provided by operating activities
        /// </summary>
        public double NetCashProvidedByOperatingActivities { get; set; }

        /// <summary>
        /// Cash Flows From Investing Activities
        /// Investments in property, plant, and equipment
        /// </summary>
        public double InvestmentsInPropertyPlantEquipment { get; set; }

        /// <summary>
        /// Cash Flows From Investing Activities
        /// Property, plant, and equipment reductions
        /// </summary>
        public double PropertyPlantEquipmentReductions { get; set; }

        /// <summary>
        /// Cash Flows From Investing Activities
        /// Acquisitions, net
        /// </summary>
        public double AcquisitionsNet { get; set; }

        /// <summary>
        /// Cash Flows From Investing Activities
        /// Purchases of investments
        /// </summary>
        public double PurchasesOfInvestments { get; set; }

        /// <summary>
        /// Cash Flows From Investing Activities
        /// Sales/Maturities of investments
        /// </summary>
        public double SalesMaturitiesOfInvestments { get; set; }

        /// <summary>
        /// Purchases of intangibles
        /// </summary>
        public double PurchasesOfIntangibles { get; set; }

        /// <summary>
        /// Sales of intangibles
        /// </summary>
        public double SalesOfIntangibles { get; set; }

        /// <summary>
        /// Other investing activities
        /// </summary>
        public double OtherInvestingActivities { get; set; }

        /// <summary>
        /// Common stock issued
        /// </summary>
        public double CommonStockIssued { get; set; }

        /// <summary>
        /// Common stock repurchased
        /// </summary>
        public double CommonStockRepurchased { get; set; }

        /// <summary>
        /// Other financing activities
        /// </summary>
        public double OtherFinancingActivities { get; set; }

        /// <summary>
        /// Cash Flows From Investing Activities
        /// Net cash used for investing activities
        /// </summary>
        public double NetCashUsedForInvestingActivities { get; set; }

        /// <summary>
        /// Cash Flows From Financing Activities
        /// Debt issued
        /// </summary>
        public double DebtIssued { get; set; }

        /// <summary>
        /// Cash Flows From Financing Activities
        /// Debt repayment
        /// </summary>
        public double DebtRepayment { get; set; }

        /// <summary>
        /// Cash Flows From Financing Activities
        /// Excess tax benefit from stock based compensation
        /// </summary>
        public double ExcessTaxBenefitFromStockBasedCompensation { get; set; }

        /// <summary>
        /// Cash Flows From Financing Activities
        /// Net cash provided by (used for) financing activities
        /// </summary>
        public double NetCashProvidedByFinancingActivities { get; set; }

        /// <summary>
        /// Effect of exchange rate changes
        /// </summary>
        public double EffectOfExchangeRateChanges { get; set; }

        /// <summary>
        /// Net change in cash
        /// </summary>
        public double NetChangeInCash { get; set; }

        /// <summary>
        /// Cash at beginning of period
        /// </summary>
        public double CashAtBeginningOfPeriod { get; set; }

        /// <summary>
        /// Cash at end of period
        /// </summary>
        public double CashAtEndOfPeriod { get; set; }

        /// <summary>
        /// Operating cash flow
        /// </summary>
        public double OperatingCashFlow { get; set; }

        /// <summary>
        /// Capital expenditure
        /// </summary>
        public double CapitalExpenditure { get; set; }

        /// <summary>
        /// Free cash flow
        /// </summary>
        public double FreeCashFlow { get; set; }
    }
}