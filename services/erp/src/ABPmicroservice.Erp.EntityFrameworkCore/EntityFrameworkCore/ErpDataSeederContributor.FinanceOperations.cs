using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// Payment discount, fee and interest accounts on the posting groups, reminder and finance charge
/// terms, deferral templates and the series of reminders and finance charge memos.
/// </summary>
public partial class ErpDataSeederContributor
{
    private async Task SeedFinanceOperationsAsync()
    {
        await EnsureAccountAsync("1500", "Prepaid Expenses", GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("2400", "Deferred Revenue", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("4920", "Fees and Charges Income", GLAccountCategory.Income, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("4930", "Interest Income", GLAccountCategory.Income, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("4950", "Payment Discounts Received", GLAccountCategory.Income, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("6300", "Payment Discounts Granted", GLAccountCategory.Expense, IncomeBalanceType.IncomeStatement);

        foreach (var group in await Repo<CustomerPostingGroup>().GetListAsync())
        {
            if (group.PaymentDiscDebitAccNo == null && group.InterestAccountNo == null)
            {
                group.SetCollectionAccounts("6300", "6300", "4920", "4930");
                await Repo<CustomerPostingGroup>().UpdateAsync(group);
            }
        }

        foreach (var group in await Repo<VendorPostingGroup>().GetListAsync())
        {
            if (group.PaymentDiscCreditAccNo == null)
            {
                group.SetPaymentDiscountAccounts("4950", "4950");
                await Repo<VendorPostingGroup>().UpdateAsync(group);
            }
        }

        if (await IsEmptyAsync<ReminderTerms>())
        {
            var terms = new ReminderTerms(NewId(), "DOMESTIC", "Domestic customers");
            terms.SetRules(postInterest: true, postAdditionalFee: true, maxNoOfReminders: 3, minimumAmountLcy: 0m);
            await Repo<ReminderTerms>().InsertAsync(terms);

            await AddReminderLevelAsync("DOMESTIC", 1, "5D", "7D", 0m, false);
            await AddReminderLevelAsync("DOMESTIC", 2, "10D", "7D", 10m, true);
            await AddReminderLevelAsync("DOMESTIC", 3, "10D", "7D", 25m, true);
        }

        if (await IsEmptyAsync<FinanceChargeTerms>())
        {
            var terms = new FinanceChargeTerms(NewId(), "1.5 PCT", "1.5 % per month");
            terms.SetRules(1.5m, 30, 1m, 0m, "5D", "14D", postInterest: true, postAdditionalFee: true);
            await Repo<FinanceChargeTerms>().InsertAsync(terms);
        }

        if (await IsEmptyAsync<DeferralTemplate>())
        {
            var revenue = new DeferralTemplate(NewId(), "12M-REV", "Revenue over 12 months", "2400", 12);
            revenue.SetRules("2400", 100m, DeferralCalculationMethod.StraightLine, DeferralStartDate.BeginningOfPeriod, 12, "Revenue %1, %2");
            await Repo<DeferralTemplate>().InsertAsync(revenue);

            var prepaid = new DeferralTemplate(NewId(), "12M-PREP", "Prepaid expense over 12 months", "1500", 12);
            prepaid.SetRules("1500", 100m, DeferralCalculationMethod.StraightLine, DeferralStartDate.BeginningOfPeriod, 12, "Expense %1, %2");
            await Repo<DeferralTemplate>().InsertAsync(prepaid);
        }

        await EnsureSeriesAsync("REM", "Reminders", "R00001");
        await EnsureSeriesAsync("REM+", "Issued reminders", "IR00001");
        await EnsureSeriesAsync("FIN", "Finance charge memos", "FC00001");
        await EnsureSeriesAsync("FIN+", "Issued finance charge memos", "IFC00001");

        var salesSetup = await Repo<SalesReceivablesSetup>().FirstOrDefaultAsync();
        if (salesSetup != null && salesSetup.ReminderNos == null)
        {
            salesSetup.SetCollectionNumberSeries("REM", "REM+", "FIN", "FIN+");
            await Repo<SalesReceivablesSetup>().UpdateAsync(salesSetup);
        }

        if (await IsEmptyAsync<CashFlowForecast>())
        {
            await Repo<CashFlowForecast>().InsertAsync(new CashFlowForecast(NewId(), "CF-MAIN", "Main cash flow forecast"));
        }
    }

    private async Task AddReminderLevelAsync(string termsCode, int no, string gracePeriod, string dueDate, decimal fee, bool interest)
    {
        var level = new ReminderLevel(NewId(), termsCode, no);
        level.SetRules(gracePeriod, dueDate, fee, interest);
        await Repo<ReminderLevel>().InsertAsync(level);
    }
}
