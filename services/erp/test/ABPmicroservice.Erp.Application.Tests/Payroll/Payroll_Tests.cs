using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Reporting;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Payroll;

public class Payroll_Tests : ErpApplicationTestBase
{
    private static readonly DateTime PayDay = new(2026, 1, 28);

    private readonly IEmployeePayItemAppService _payItems;
    private readonly IPayrollRunAppService _runs;
    private readonly IPayslipAppService _payslips;

    public Payroll_Tests()
    {
        _payItems = GetRequiredService<IEmployeePayItemAppService>();
        _runs = GetRequiredService<IPayrollRunAppService>();
        _payslips = GetRequiredService<IPayslipAppService>();
    }

    private async Task HireAsync(string no, decimal basicPay)
    {
        var employee = new Employee(Guid.NewGuid(), no, "Achieng", "Otieno");
        employee.SetPayment("EMPLOYEES", "0123456789", null, null);
        await GetRequiredService<IRepository<Employee, Guid>>().InsertAsync(employee, autoSave: true);

        await _payItems.CreateAsync(new CreateUpdateEmployeePayItemDto { EmployeeNo = no, ItemType = PayItemType.Earning, Code = "BASIC", Amount = basicPay });
        await _payItems.CreateAsync(new CreateUpdateEmployeePayItemDto { EmployeeNo = no, ItemType = PayItemType.Earning, Code = "HOUSE" });
        await _payItems.CreateAsync(new CreateUpdateEmployeePayItemDto { EmployeeNo = no, ItemType = PayItemType.Deduction, Code = "PENSION" });
    }

    [Fact]
    public async Task A_Payroll_Run_Taxes_Pay_Posts_It_And_Pays_The_Net_By_Voucher()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await HireAsync("E-PAY1", 100000m);

            var run = await _runs.CreateAsync(new CreateUpdatePayrollRunDto { PayPeriod = new DateTime(2026, 1, 15), PostingDate = PayDay });
            run.No.ShouldBe("PR-00001");
            run.PayPeriod.ShouldBe(new DateTime(2026, 1, 1));

            var notCalculated = await Should.ThrowAsync<BusinessException>(() => _runs.RunPostingAsync(run.Id));
            notCalculated.Code.ShouldBe(ErpErrorCodes.Payroll.RunStatusWrong);

            run = await _runs.CalculateAsync(run.Id);
            run.Status.ShouldBe(PayrollRunStatus.Calculated);
            run.NoOfEmployees.ShouldBe(1);

            // Basic 100,000 and 15% house allowance; the 5% pension comes off before tax.
            var payslip = (await _payslips.GetListAsync(new GetPayslipListInput { PayrollRunNo = run.No })).Items.Single();
            payslip.GrossPay.ShouldBe(115000m);
            payslip.TaxablePay.ShouldBe(110000m);

            // 2,400 + 2,083.25 + 23,300.10 on the bands, less 2,400 relief.
            payslip.IncomeTax.ShouldBe(25383.35m);
            payslip.TotalDeductions.ShouldBe(30383.35m);
            payslip.NetPay.ShouldBe(84616.65m);
            payslip.EmployerContributions.ShouldBe(5000m);

            // Calculating again replaces the payslips rather than adding to them.
            run = await _runs.CalculateAsync(run.Id);
            (await _payslips.GetListAsync(new GetPayslipListInput { PayrollRunNo = run.No })).TotalCount.ShouldBe(1);
            run.TotalNet.ShouldBe(84616.65m);

            run = await _runs.RunPostingAsync(run.Id);
            run.Status.ShouldBe(PayrollRunStatus.Posted);

            var entries = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.DocumentNo == run.No);
            entries.Sum(e => (double)e.Amount).ShouldBe(0d);
            entries.Where(e => e.GLAccountNo == "6200").Sum(e => e.Amount).ShouldBe(115000m);
            entries.Where(e => e.GLAccountNo == "6210").Sum(e => e.Amount).ShouldBe(5000m);
            entries.Where(e => e.GLAccountNo == "2380").Sum(e => e.Amount).ShouldBe(-25383.35m);
            entries.Where(e => e.GLAccountNo == "2390").Sum(e => e.Amount).ShouldBe(-10000m);
            entries.Where(e => e.GLAccountNo == "2140").Sum(e => e.Amount).ShouldBe(-84616.65m);

            // A month is paid once.
            var again = await _runs.CreateAsync(new CreateUpdatePayrollRunDto { PayPeriod = new DateTime(2026, 1, 1), PostingDate = PayDay });
            await _runs.CalculateAsync(again.Id);
            (await Should.ThrowAsync<BusinessException>(() => _runs.RunPostingAsync(again.Id))).Code.ShouldBe(ErpErrorCodes.Payroll.PeriodAlreadyPosted);

            run = await _runs.RaisePaymentVoucherAsync(run.Id);
            var voucherLine = (await GetRequiredService<IRepository<PaymentVoucherLine, Guid>>().GetListAsync(l => l.DocumentNo == run.PaymentVoucherNo)).Single();
            voucherLine.AccountType.ShouldBe(GenJournalAccountType.Employee);
            voucherLine.AccountNo.ShouldBe("E-PAY1");
            voucherLine.Amount.ShouldBe(84616.65m);

            (await Should.ThrowAsync<BusinessException>(() => _runs.RaisePaymentVoucherAsync(run.Id))).Code.ShouldBe(ErpErrorCodes.Payroll.VoucherAlreadyRaised);
        });
    }

    [Fact]
    public async Task The_Payroll_Reports_Show_The_Posted_Run()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await HireAsync("E-PAY2", 50000m);

            var run = await _runs.CreateAsync(new CreateUpdatePayrollRunDto { PayPeriod = new DateTime(2026, 2, 1), PostingDate = new DateTime(2026, 2, 27) });
            await _runs.CalculateAsync(run.Id);

            var reports = GetRequiredService<IStandardReportAppService>();
            RunStandardReportInput Request(string code) => new() { Code = code, FromDate = new DateTime(2026, 2, 1), ToDate = new DateTime(2026, 2, 28) };

            foreach (var code in new[] { "PayrollSummary", "Payslips", "PayrollRegister", "PayrollDeductionSchedule", "PayrollNetPaySchedule" })
            {
                var result = await reports.RunAsync(Request(code));
                result.Rows.ShouldNotBeEmpty(code);
            }

            var register = await reports.RunAsync(Request("PayrollRegister"));
            register.Rows.First().Values["no"].ShouldBe("E-PAY2");
            (await reports.GetListAsync()).Count(r => r.Area == "Payroll").ShouldBe(5);
        });
    }

    [Fact]
    public async Task A_Pay_Item_Cannot_Be_Doubled_And_Tax_Bands_Cannot_Overlap()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await HireAsync("E-PAY3", 40000m);

            var doubled = await Should.ThrowAsync<BusinessException>(() =>
                _payItems.CreateAsync(new CreateUpdateEmployeePayItemDto { EmployeeNo = "E-PAY3", ItemType = PayItemType.Earning, Code = "BASIC", Amount = 1000m })
            );
            doubled.Code.ShouldBe(ErpErrorCodes.Payroll.PayItemDuplicated);

            var overlap = await Should.ThrowAsync<BusinessException>(() =>
                GetRequiredService<IPayrollTaxBandAppService>().CreateAsync(new CreateUpdatePayrollTaxBandDto { LowerLimit = 10000m, UpperLimit = 20000m, RatePct = 5m })
            );
            overlap.Code.ShouldBe(ErpErrorCodes.Payroll.InvalidTaxBand);
        });
    }
}
