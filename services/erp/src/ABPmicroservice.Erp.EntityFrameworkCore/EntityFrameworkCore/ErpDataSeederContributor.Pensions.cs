using System.Threading.Tasks;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Pensions;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// What the pension module needs before a first scheme can be set up: the dimension schemes are
/// values of, number series, the G/L accounts the processes post to, the usual exit reasons, pay
/// modes, suspension reasons and revision reasons.
/// </summary>
public partial class ErpDataSeederContributor
{
    public const string SchemeDimensionCode = "SCHEME";

    private async Task SeedPensionsAsync()
    {
        if (!await _dimensionRepository.AnyAsync(d => d.Code == SchemeDimensionCode))
        {
            await _dimensionRepository.InsertAsync(new Dimension(NewId(), SchemeDimensionCode, "Pension Scheme"), autoSave: true);
        }

        await EnsureSeriesAsync("PEN-MEM", "Pension Members", "M000010");
        await EnsureSeriesAsync("PEN-SPON", "Pension Sponsors", "SP0010");
        await EnsureSeriesAsync("PEN-CONT", "Contribution Schedules", "PC-00001");
        await EnsureSeriesAsync("PEN-INT", "Interest Allocations", "PI-00001");
        await EnsureSeriesAsync("PEN-EXIT", "Member Exits", "PX-00001");

        await EnsureAccountAsync("1310", "Contributions Receivable", GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("2610", "Member Funds", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("2620", "Benefits Payable", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("2630", "Tax on Benefits and Interest", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("8610", "Interest Distributed to Members", GLAccountCategory.Expense, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("8620", "Pensions Paid", GLAccountCategory.Expense, IncomeBalanceType.IncomeStatement);

        await EnsureSeriesAsync("PEN-PNR", "Pensioners", "PN0010");
        await EnsureSeriesAsync("PEN-PAY", "Pension Payrolls", "PP-00001");
        await EnsureSeriesAsync("PEN-DBC", "Benefit Calculations", "DBC-00001");
        await EnsureSeriesAsync("PEN-INC", "Pension Increments", "INC-00001");

        await SeedPaymentVouchersAsync();

        if (await IsEmptyAsync<PensionSetup>())
        {
            var setup = new PensionSetup(NewId());
            setup.SetSchemeDimension(SchemeDimensionCode);
            setup.SetNumbering("PEN-MEM", "PEN-SPON", "PEN-CONT", "PEN-INT", "PEN-EXIT");
            setup.SetAccounts("2610", "1310", "8610", "2620", "2630");
            setup.SetPensionPayroll("PEN-PNR", "PEN-PAY", "8620");
            setup.SetBenefitCalculationNumbering("PEN-DBC");
            setup.SetMemberAdministration(null, ExcessContributionAllocation.EmployeePriority, 12, "PEN-INC");
            setup.SetPensionerDefaults("BANK", 0m);
            await Repo<PensionSetup>().InsertAsync(setup);
        }

        if (await IsEmptyAsync<PensionerPayMode>())
        {
            (string Code, string Description, PensionerPaymentType Type)[] modes =
            [
                ("BANK", "Bank transfer", PensionerPaymentType.Bank),
                ("MOBILE", "Mobile money", PensionerPaymentType.MobileMoney),
                ("CHEQUE", "Cheque", PensionerPaymentType.Cheque),
            ];
            foreach (var (code, description, type) in modes)
            {
                var mode = new PensionerPayMode(NewId(), code, description);
                mode.Set(type);
                await Repo<PensionerPayMode>().InsertAsync(mode);
            }
        }

        if (await IsEmptyAsync<PensionerSuspensionReason>())
        {
            var lifeCertificate = new PensionerSuspensionReason(NewId(), "LIFECERT", PensionerAdministrator.LifeCertificateOverdue);
            lifeCertificate.Set(true);
            await Repo<PensionerSuspensionReason>().InsertAsync(lifeCertificate);
            await Repo<PensionerSuspensionReason>().InsertAsync(new PensionerSuspensionReason(NewId(), "DECEASED", "Reported deceased"));
            await Repo<PensionerSuspensionReason>().InsertAsync(new PensionerSuspensionReason(NewId(), "BANK", "Bank account closed or rejected"));
        }

        if (await IsEmptyAsync<PensionRevisionReason>())
        {
            await Repo<PensionRevisionReason>().InsertAsync(new PensionRevisionReason(NewId(), "COLA", "Cost of living adjustment"));
            await Repo<PensionRevisionReason>().InsertAsync(new PensionRevisionReason(NewId(), "CORRECTION", "Correction of the pension"));
        }

        if (await IsEmptyAsync<LumpsumTaxTable>())
        {
            var table = new LumpsumTaxTable(NewId(), "WITHDRAWAL", "Withdrawal before retirement");
            table.Set(annualTaxFreeAmount: 60000m, maxTaxFreeAmount: 600000m, maxAgeTaxable: 0);
            await Repo<LumpsumTaxTable>().InsertAsync(table);

            (decimal Lower, decimal Upper, decimal Rate)[] bands = [(0m, 400000m, 10m), (400000m, 800000m, 15m), (800000m, 1200000m, 20m), (1200000m, 1600000m, 25m), (1600000m, 0m, 30m)];
            foreach (var (lower, upper, rate) in bands)
            {
                var band = new LumpsumTaxBand(NewId(), "WITHDRAWAL", lower);
                band.Set(upper, rate);
                await Repo<LumpsumTaxBand>().InsertAsync(band);
            }
        }

        if (await IsEmptyAsync<ExitReason>())
        {
            var withdrawal = new ExitReason(NewId(), "WITHDRAWAL", "Withdrawal on leaving employment");
            withdrawal.Set(ExitPaymentOption.PayEmployeeAndEmployer, 50m, "WITHDRAWAL", false, MemberStatus.Deferred);
            withdrawal.SetVesting(true);
            await Repo<ExitReason>().InsertAsync(withdrawal);

            var retirement = new ExitReason(NewId(), "RETIREMENT", "Normal retirement");
            retirement.Set(ExitPaymentOption.PayEmployeeAndEmployer, 100m, null, true, MemberStatus.Inactive);
            await Repo<ExitReason>().InsertAsync(retirement);

            var death = new ExitReason(NewId(), "DEATH", "Death in service");
            death.Set(ExitPaymentOption.PayEmployeeAndEmployer, 100m, null, true, MemberStatus.DeathInService);
            await Repo<ExitReason>().InsertAsync(death);
        }
    }
}
