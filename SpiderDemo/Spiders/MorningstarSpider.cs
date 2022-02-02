using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MisterSpider;
using SpiderDemo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SpiderDemo.Spiders
{
    public class MorningstarSpider : Spider<Company>
    {
        public Company Company;

        public MorningstarSpider(ILogger<Spider<Company>> logger, IOptions<ConfigOptions> config, Company spiderParams, NetConnectionMorningstar connection) : base(logger, connection, config)
        {
            string baseHost = "http://financials.morningstar.com/ajax/ReportProcess4CSV.html";
            Company = spiderParams;
            connection.RefererParam = string.Format("{0}?t={1}&region=usa&culture=en-US&reportType={2}&period=12&dataType=A&order=asc", baseHost, Company.Symbol, "is");
            Urls = new List<string>
            {
                connection.RefererParam,
                string.Format("{0}?t={1}&region=usa&culture=en-US&reportType={2}&period=12&dataType=A&order=asc", baseHost, Company.Symbol, "bs"),
                string.Format("{0}?t={1}&region=usa&culture=en-US&reportType={2}&period=12&dataType=A&order=asc", baseHost, Company.Symbol, "cf"),
                string.Format("{0}?t={1}&region=usa&culture=en-US&order=asc", "http://financials.morningstar.com/finan/ajax/exportKR2CSV.html", Company.Symbol, "cf"),
            };
        }

        protected override Company Crawl(Page page)
        {
            Company company = Company;
            using (StreamReader reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(page.Source ?? string.Empty))))
            {
                // skip the first line
                string firstLine = reader.ReadLine();
                // Fiscal year ends in March. CNY.,2017-03,2018-03,2019-03,2020-03,2021-03,TTM
                // Fiscal year ends in December. USD.,2016-12,2017-12,2018-12,2019-12,2020-12,TTM
                if (firstLine.EndsWith("INCOME STATEMENT"))
                {
                    try
                    {
                        IncomeStatement(company, reader);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("INCOME STATEMENT: " + ex.Message.ToString());
                    }
                }
                else if (firstLine.EndsWith("CASH FLOW"))
                {
                    try
                    {
                        CashFlow(company, reader);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("CASH FLOW: " + ex.Message.ToString());
                    }

                }
                else if (firstLine.EndsWith("BALANCE SHEET"))
                {
                    try
                    {
                        BalanceSheet(company, reader);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("BALANCE SHEET: " + ex.Message.ToString());
                    }
                }
                else if (firstLine.StartsWith("Growth Profitability and Financial Ratios"))
                {
                    try
                    {
                        FinancialRatios(company, reader);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Growth Profitability and Financial Ratios: " + ex.Message.ToString());
                    }
                }
            }

            // to shared the same item
            return null;
        }

        private void FinancialRatios(Company company, StreamReader reader)
        {
            string pattern = "\"(.*?)\"";
            string mil = "000000";
            bool financials = false;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                line = Regex.Replace(line, pattern, m => {
                    string newLine = m.Value.Replace(",", string.Empty).Replace("\"", string.Empty);
                    if (line.Contains("Mil"))
                        return newLine + mil;
                    return newLine; 
                });
                string identifierString = string.Empty;
                string[] values = null;
                if (line.StartsWith("\""))
                {
                    values = line.Split("\",");
                    identifierString = values[0].Replace("\"", string.Empty);
                    if (values.Length > 1)
                        values = ("NC," + values[1]).Split(',');
                }
                else
                {
                    values = line.Split(',');
                    identifierString = values[0];
                }


                switch (identifierString)
                {
                    case string id when id.Equals("Financials"):
                        financials = true;
                        // read the next line
                        values = reader.ReadLine().Split(",");
                        for (int i = 1; i < values.Length; i++)
                        {
                            string yearString = values[i];
                            if (yearString.Contains("TTM")) continue;

                            DateTime year = DateTime.Parse(yearString);

                            if (!company.FinancialRatios.Any(x => x.Year == year))
                            {
                                company.FinancialRatios.Add(new FinancialRatios()
                                {
                                    Year = year
                                });
                            }
                        }

                        break;

                    case string id when id.StartsWith("Revenue") && financials:
                        for (int i = 0; i < company.FinancialRatios.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.FinancialRatios.ElementAt(i).Revenue = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.StartsWith("Earnings Per Share") && financials:
                        for (int i = 0; i < company.FinancialRatios.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.FinancialRatios.ElementAt(i).EarningsPerShare = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.StartsWith("Book Value Per Share") && financials:
                        for (int i = 0; i < company.FinancialRatios.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.FinancialRatios.ElementAt(i).BookValuePerShare = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.StartsWith("Free Cash Flow") && financials:
                        financials = false;
                        for (int i = 0; i < company.FinancialRatios.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.FinancialRatios.ElementAt(i).FreeCashFlow = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Return on Invested Capital %"):
                        for (int i = 0; i < company.FinancialRatios.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.FinancialRatios.ElementAt(i).ReturnOnInvestedCapital = GetDouble(values[i + 1]);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void IncomeStatement(Company company, StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string identifierString = string.Empty;
                string[] values = null;
                if (line.StartsWith("\""))
                {
                    values = line.Split("\",");
                    identifierString = values[0].Replace("\"", string.Empty);
                    if (values.Length > 1)
                        values = ("NC," + values[1]).Split(',');
                }
                else
                {
                    values = line.Split(',');
                    identifierString = values[0];
                }

                switch (identifierString)
                {
                    case string id when id.StartsWith("Fiscal year ends "):
                        string[] fiscalYearLineValues = identifierString.Replace("Fiscal year ends in ", string.Empty).Split('.');
                        company.FiscalYearEnds = fiscalYearLineValues[0].Trim();
                        company.Currency = fiscalYearLineValues[1].Trim();
                        for (int i = 1; i < values.Length; i++)
                        {
                            string yearString = values[i];
                            if (yearString.Contains("TTM")) continue;

                            DateTime year = DateTime.Parse(yearString);

                            if (!company.IncomeStatement.Any(x => x.Year == year))
                            {
                                company.IncomeStatement.Add(new IncomeStatement()
                                {
                                    Year = year
                                });
                            }
                        }

                        break;

                    case string id when id.Equals("Revenue"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).Revenue = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Cost of revenue"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).CostOfRevenue = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Gross profit"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).GrossProfit = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Operating expenses"):
                        break;

                    case string id when id.Equals("Research and development"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).ResearchAndDevelopment = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Sales, General and administrative"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).SalesGeneralAdministrative = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Restructuring, merger and acquisition"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).RestructuringMergerAndAcquisition = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other operating expenses"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).OtherOperatingExpenses = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Total operating expenses"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).TotalOperatingExpenses = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Operating income"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).OperatingIncome = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Interest Expense"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).InterestExpense = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other income (expense)"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).OtherIncomeExpense = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Income before taxes"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).IncomeBeforeTaxes = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Provision for income taxes"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).ProvisionForIncomeTaxes = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other income"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).OtherIncome = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Net income from continuing operations"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).NetIncomeFromContinuingOperations = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).Other = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Net income"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).NetIncome = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Preferred dividend"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).PreferredDividend = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Net income available to common shareholders"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).NetIncomeAvailableToCommonShareholders = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Earnings per share"):
                        // skip Basic values, we only want the Diluted
                        reader.ReadLine();
                        line = reader.ReadLine();
                        values = line.Split(',');

                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).EarningsPerShare = GetDouble(values[i + 1]);
                        }

                        break;

                    case string id when id.Equals("Weighted average shares outstanding"):
                        // skip Basic values, we only want the Diluted
                        reader.ReadLine();
                        line = reader.ReadLine();
                        values = line.Split(',');

                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).Shares = GetDouble(values[i + 1]);
                        }

                        break;
                    case string id when id.Equals("EBITDA"):
                        for (int i = 0; i < company.IncomeStatement.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.IncomeStatement.ElementAt(i).Ebitda = GetDouble(values[i + 1]);
                        }
                        break;

                    default:
                        break;
                }

            }
        }

        private void CashFlow(Company company, StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string identifierString = string.Empty;
                string[] values = null;
                if (line.StartsWith("\""))
                {
                    values = line.Split("\",");
                    identifierString = values[0].Replace("\"", string.Empty);
                    if (values.Length > 1)
                        values = ("NC," + values[1]).Split(',');
                }
                else
                {
                    values = line.Split(',');
                    identifierString = values[0];
                }

                switch (identifierString)
                {
                    case string id when id.StartsWith("Fiscal year ends "):
                        string[] fiscalYearLineValues = identifierString.Replace("Fiscal year ends in ", string.Empty).Split('.');
                        company.FiscalYearEnds = fiscalYearLineValues[0].Trim();
                        company.Currency = fiscalYearLineValues[1].Trim();
                        for (int i = 1; i < values.Length; i++)
                        {
                            string yearString = values[i];
                            if (yearString.Contains("TTM")) continue;

                            DateTime year = DateTime.Parse(yearString);

                            if (!company.CashFlow.Any(x => x.Year == year))
                            {
                                company.CashFlow.Add(new CashFlow()
                                {
                                    Year = year
                                });
                            }
                        }

                        break;

                    case string id when id.Equals("Cash Flows From Operating Activities"):
                        break;

                    case string id when id.Equals("Net income"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).NetIncome = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Depreciation & amortization"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).DepreciationAmortization = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Investment/asset impairment charges"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).InvestmentAassetImpairmentCharges = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Investments losses (gains)"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).InvestmentsLosses = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Deferred income taxes"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).DeferredIncomeTaxes = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Prepaid expenses"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).PrepaidExpenses = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Income taxes payable"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).IncomeTaxesPayable = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Stock based compensation"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).StockBasedCompensation = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Change in working capital"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).ChangeInWorkingCapital = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Accounts receivable"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).AccountsReceivable = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Inventory"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).Inventory = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Accounts payable"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).AccountsPayable = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Accrued liabilities"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).AccruedLiabilities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other working capital"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).OtherWorkingCapital = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other non-cash items"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).OtherNonCashItems = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Net cash provided by operating activities"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).NetCashProvidedByOperatingActivities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Cash Flows From Investing Activities"):
                        break;

                    case string id when id.Equals("Investments in property, plant, and equipment"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).InvestmentsInPropertyPlantEquipment = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Property, plant, and equipment reductions"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).PropertyPlantEquipmentReductions = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Acquisitions, net"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).AcquisitionsNet = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Purchases of investments"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).PurchasesOfInvestments = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Sales/Maturities of investments"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).SalesMaturitiesOfInvestments = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Purchases of intangibles"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).PurchasesOfIntangibles = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Sales of intangibles"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).SalesOfIntangibles = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other investing activities"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).OtherInvestingActivities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Common stock issued"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).CommonStockIssued = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Common stock repurchased"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).CommonStockRepurchased = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other financing activities"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).OtherFinancingActivities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Net cash used for investing activities"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).NetCashUsedForInvestingActivities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Cash Flows From Financing Activities"):
                        break;

                    case string id when id.Equals("Debt issued"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).DebtIssued = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Debt repayment"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).DebtRepayment = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Excess tax benefit from stock based compensation"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).ExcessTaxBenefitFromStockBasedCompensation = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Net cash provided by (used for) financing activities"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).NetCashProvidedByFinancingActivities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Effect of exchange rate changes"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).EffectOfExchangeRateChanges = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Net change in cash"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).NetChangeInCash = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Cash at beginning of period"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).CashAtBeginningOfPeriod = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Cash at end of period"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).CashAtEndOfPeriod = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Free Cash Flow"):
                        break;

                    case string id when id.Equals("Operating cash flow"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).OperatingCashFlow = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Capital expenditure"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).CapitalExpenditure = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Free cash flow"):
                        for (int i = 0; i < company.CashFlow.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.CashFlow.ElementAt(i).FreeCashFlow = GetDouble(values[i + 1]);
                        }
                        break;

                    default:
                        break;
                }
            }

        }

        private void BalanceSheet(Company company, StreamReader reader)
        {
            bool currentAssets = false;
            bool nonCurrentAssets = false;

            bool currentLiabilities = false;
            bool nonCurrentLiabilities = false;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string identifierString = string.Empty;
                string[] values = null;
                if (line.StartsWith("\""))
                {
                    values = line.Split("\",");
                    identifierString = values[0].Replace("\"", string.Empty);
                    if (values.Length > 1)
                        values = ("NC," + values[1]).Split(',');
                }
                else
                {
                    values = line.Split(',');
                    identifierString = values[0];
                }

                switch (identifierString)
                {
                    case string id when id.StartsWith("Fiscal year ends "):
                        string[] fiscalYearLineValues = identifierString.Replace("Fiscal year ends in ", string.Empty).Split('.');
                        company.FiscalYearEnds = fiscalYearLineValues[0].Trim();
                        company.Currency = fiscalYearLineValues[1].Trim();
                        for (int i = 1; i < values.Length; i++)
                        {
                            string yearString = values[i];
                            if (yearString.Contains("TTM")) continue;

                            DateTime year = DateTime.Parse(yearString);

                            if (!company.BalanceSheet.Any(x => x.Year == year))
                            {
                                company.BalanceSheet.Add(new BalanceSheet()
                                {
                                    Year = year
                                });
                            }
                        }

                        break;

                    case string id when id.Equals("Assets"):
                        break;

                    case string id when id.Equals("Current assets"):
                        currentAssets = true;
                        nonCurrentAssets = false;
                        break;

                    case string id when id.Equals("Cash"):
                        break;

                    case string id when id.Equals("Cash and cash equivalents"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).CashAndCashEquivalents = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Short-term investments"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).ShortTermInvestments = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Total cash"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).TotalCash = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Receivables"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).Receivables = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Inventories"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).Inventories = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Deferred income taxes") && currentAssets:
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).DeferredIncomeTaxesCurrentAssets = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Prepaid expenses"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).PrepaidExpenses = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other current assets"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).OtherCurrentAssets = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Total current assets"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).TotalCurrentAssets = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Non-current assets"):
                        currentAssets = false;
                        nonCurrentAssets = true;
                        break;

                    case string id when id.Equals("Property, plant and equipment"):
                        break;

                    case string id when id.Equals("Gross property, plant and equipment"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).GrossPropertyPlantEquipment = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Accumulated Depreciation"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).AccumulatedDepreciation = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Net property, plant and equipment"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).NetPropertyPlantEquipment = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Equity and other investments"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).EquityAndOtherInvestments = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Goodwill"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).Goodwill = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Intangible assets"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).IntangibleAssets = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Deferred income taxes") && nonCurrentAssets:
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).DeferredIncomeTaxesNonCurrentAssets = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other long-term assets"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).OtherLongTermAssets = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Total non-current assets"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).TotalNonCurrentAssets = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Total assets"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).TotalAssets = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Liabilities and stockholders' equity"):
                        break;

                    case string id when id.Equals("Liabilities"):
                        break;

                    case string id when id.Equals("Current liabilities"):
                        currentLiabilities = true;
                        nonCurrentLiabilities = false;
                        break;

                    case string id when id.Equals("Short-term debt"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).ShortTermDebt = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Capital leases") && currentLiabilities:
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).CapitalLeasesCurrentLiabilities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Taxes payable"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).TaxesPayable = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Accounts payable"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).AccountsPayable = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Accrued liabilities"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).AccruedLiabilities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Deferred revenues") && currentLiabilities:
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).DeferredRevenuesCurrentLiabilities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other current liabilities"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).OtherCurrentLiabilities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Total current liabilities"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).TotalCurrentLiabilities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Non-current liabilities"):
                        currentLiabilities = false;
                        nonCurrentLiabilities = true;
                        break;

                    case string id when id.Equals("Long-term debt"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).LongTermDebt = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Capital leases") && nonCurrentLiabilities:
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).CapitalLeasesNonCurrentLiabilities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Deferred taxes liabilities"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).DeferredTaxesLiabilities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Deferred revenues") && nonCurrentLiabilities:
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).DeferredRevenuesNonCurrentLiabilities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Minority interest"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).MinorityInterest = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other long-term liabilities"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).OtherLongTermLiabilities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Total non-current liabilities"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).TotalNonCurrentLiabilities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Total liabilities"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).TotalLiabilities = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Stockholders' equity"):
                        break;

                    case string id when id.Equals("Common stock"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).CommonStock = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Other Equity"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).OtherEquity = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Additional paid-in capital"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).AdditionalPaidInCapital = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Retained earnings"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).RetainedEarnings = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Treasury stock"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).TreasuryStock = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Accumulated other comprehensive income"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).AccumulatedOtherComprehensiveIncome = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Total stockholders' equity"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).TotalStockholdersEquity = GetDouble(values[i + 1]);
                        }
                        break;

                    case string id when id.Equals("Total liabilities and stockholders' equity"):
                        for (int i = 0; i < company.BalanceSheet.Count; i++)
                        {
                            // +1 because the position 0 is the identifier string
                            company.BalanceSheet.ElementAt(i).TotalLiabilitiesStockholdersEquity = GetDouble(values[i + 1]);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private double GetDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0.0;

            return double.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}
