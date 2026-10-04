using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Payroll;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// What a first payroll needs: its number series, salary and payroll liability accounts, a basic
/// salary and a house allowance, income tax with its bands and a matched pension contribution.
/// The rates are a starting point to be replaced by the company's own.
/// </summary>
public partial class ErpDataSeederContributor
{
    private async Task SeedPayrollAsync()
    {
        await EnsureSeriesAsync("PAYROLL", "Payroll Runs", "PR-00001");

        await EnsureAccountAsync("2380", "Income Tax Payable (Payroll)", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("2390", "Pension Contributions Payable", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("6200", "Salaries and Wages", GLAccountCategory.Expense, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("6210", "Employer Pension Contributions", GLAccountCategory.Expense, IncomeBalanceType.IncomeStatement);

        if (await IsEmptyAsync<PayrollSetup>())
        {
            var setup = new PayrollSetup(NewId());
            setup.Set("PAYROLL", 2400m);
            await Repo<PayrollSetup>().InsertAsync(setup);
        }

        if (await IsEmptyAsync<PayrollEarning>())
        {
            var basic = new PayrollEarning(NewId(), "BASIC", "Basic salary");
            basic.Set(PayCalculationMethod.FlatAmount, 0m, basicPay: true, taxable: true, "6200", blocked: false);
            await Repo<PayrollEarning>().InsertAsync(basic);

            var house = new PayrollEarning(NewId(), "HOUSE", "House allowance");
            house.Set(PayCalculationMethod.PercentOfBasic, 15m, basicPay: false, taxable: true, "6200", blocked: false);
            await Repo<PayrollEarning>().InsertAsync(house);
        }

        if (await IsEmptyAsync<PayrollDeduction>())
        {
            var tax = new PayrollDeduction(NewId(), "PAYE", "Income tax");
            tax.Set(PayCalculationMethod.TaxBands, 0m, 0m, taxDeductible: false, 0m, "2380", null, blocked: false);
            tax.SetStatutory(true);
            await Repo<PayrollDeduction>().InsertAsync(tax);

            var pension = new PayrollDeduction(NewId(), "PENSION", "Pension contribution");
            pension.Set(PayCalculationMethod.PercentOfBasic, 5m, 0m, taxDeductible: true, 100m, "2390", "6210", blocked: false);
            await Repo<PayrollDeduction>().InsertAsync(pension);
        }

        if (await IsEmptyAsync<PayrollTaxBand>())
        {
            (decimal Lower, decimal Upper, decimal Rate)[] bands = [(0m, 24000m, 10m), (24000m, 32333m, 25m), (32333m, 500000m, 30m), (500000m, 800000m, 32.5m), (800000m, 0m, 35m)];
            foreach (var (lower, upper, rate) in bands)
            {
                var band = new PayrollTaxBand(NewId(), lower);
                band.Set(upper, rate);
                await Repo<PayrollTaxBand>().InsertAsync(band);
            }
        }
    }
}
