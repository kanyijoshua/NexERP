using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Finance;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Academics;

public class StudentAccounts_Tests : ErpApplicationTestBase
{
    private static readonly DateTime Today = new(2026, 1, 28);

    private readonly IAcademicYearAppService _years;
    private readonly ISemesterAppService _semesters;
    private readonly IProgrammeAppService _programmes;
    private readonly IProgrammeStageAppService _stages;
    private readonly ICourseUnitAppService _courseUnits;
    private readonly IFeeStructureLineAppService _feeStructure;
    private readonly IStudentAppService _students;
    private readonly ISemesterRegistrationAppService _registrations;
    private readonly IStudentReceiptAppService _receipts;
    private readonly IStudentRefundAppService _refunds;
    private readonly IStudentStatusChangeAppService _statusChanges;
    private readonly IExamResultAppService _examResults;
    private readonly IExamResultLineAppService _examResultLines;

    public StudentAccounts_Tests()
    {
        _years = GetRequiredService<IAcademicYearAppService>();
        _semesters = GetRequiredService<ISemesterAppService>();
        _programmes = GetRequiredService<IProgrammeAppService>();
        _stages = GetRequiredService<IProgrammeStageAppService>();
        _courseUnits = GetRequiredService<ICourseUnitAppService>();
        _feeStructure = GetRequiredService<IFeeStructureLineAppService>();
        _students = GetRequiredService<IStudentAppService>();
        _registrations = GetRequiredService<ISemesterRegistrationAppService>();
        _receipts = GetRequiredService<IStudentReceiptAppService>();
        _refunds = GetRequiredService<IStudentRefundAppService>();
        _statusChanges = GetRequiredService<IStudentStatusChangeAppService>();
        _examResults = GetRequiredService<IExamResultAppService>();
        _examResultLines = GetRequiredService<IExamResultLineAppService>();
    }

    /// <summary>A one-stage, one-unit programme costing 30,000 a semester, and a student of it.</summary>
    private async Task<StudentDto> SetUpStudentAsync(string programme)
    {
        await _years.CreateAsync(new CreateUpdateAcademicYearDto { Code = "2026", Description = "2026", Current = true });
        await _semesters.CreateAsync(new CreateUpdateSemesterDto { Code = "SEM1", Description = "Semester 1", AcademicYearCode = "2026", Current = true });
        await _programmes.CreateAsync(new CreateUpdateProgrammeDto { Code = programme, Description = programme, ExamCategoryCode = "GENERAL" });
        await _stages.CreateAsync(new CreateUpdateProgrammeStageDto { ProgrammeCode = programme, Code = "Y1", Sequence = 1, FinalStage = true });
        await _courseUnits.CreateAsync(new CreateUpdateCourseUnitDto { ProgrammeCode = programme, Code = "U101", Description = "Introduction", StageCode = "Y1" });
        await _feeStructure.CreateAsync(new CreateUpdateFeeStructureLineDto { ProgrammeCode = programme, StageCode = "Y1", FeeItemCode = "TUITION", Amount = 30000m });

        return await _students.CreateAsync(new CreateUpdateStudentDto { ProgrammeCode = programme, FirstName = "Mumbua", LastName = "Mutua", AdmissionDate = Today });
    }

    private async Task<StudentReceiptDto> ReceiveAsync(StudentDto student, decimal amount, string billNo = null)
    {
        var receipt = await _receipts.CreateAsync(new CreateUpdateStudentReceiptDto
        {
            StudentNo = student.No,
            PostingDate = Today,
            BankAccountNo = "WWB-OPERATING",
            PayMode = "BANK",
            ExternalDocumentNo = "SLIP-" + amount.ToString("0"),
            Amount = amount,
            AppliesToBillNo = billNo,
        });

        return await _receipts.RunPostingAsync(receipt.Id);
    }

    private async Task<SemesterRegistrationDto> RegisterAsync(StudentDto student)
    {
        var registration = await _registrations.CreateAsync(new CreateUpdateSemesterRegistrationDto { StudentNo = student.No, StageCode = "Y1", RegistrationDate = Today });
        await _registrations.FillUnitsAsync(registration.Id);
        return await _registrations.SubmitAsync(registration.Id);
    }

    [Fact]
    public async Task Money_Paid_Ahead_Is_A_Prepayment_That_The_Bill_Uses_Up_And_A_Refund_Pays_Back()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var student = await SetUpStudentAsync("DPM");

            // 50,000 paid before anything is billed sits on the account as a prepayment.
            var receipt = await ReceiveAsync(student, 50000m);
            receipt.No.ShouldBe("SR-00001");
            receipt.Status.ShouldBe(AcademicDocumentStatus.Posted);

            var balance = await _students.GetBalanceAsync(student.Id);
            balance.Balance.ShouldBe(-50000m);
            balance.Prepayment.ShouldBe(50000m);

            var glEntries = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.DocumentNo == receipt.No);
            glEntries.Sum(e => (double)e.Amount).ShouldBe(0d);
            glEntries.Single(e => e.GLAccountNo == "1020").Amount.ShouldBe(50000m);
            glEntries.Single(e => e.GLAccountNo == "1200").Amount.ShouldBe(-50000m);

            // Registration bills 30,000, which the prepayment covers; 20,000 is still paid ahead.
            await RegisterAsync(student);
            balance = await _students.GetBalanceAsync(student.Id);
            balance.TotalBilled.ShouldBe(30000m);
            balance.Prepayment.ShouldBe(20000m);

            // Only what is paid ahead can be refunded.
            var tooMuch = await _refunds.CreateAsync(new CreateUpdateStudentRefundDto { StudentNo = student.No, DocumentDate = Today, Amount = 25000m });
            var refused = await Should.ThrowAsync<BusinessException>(() => _refunds.ApproveAsync(tooMuch.Id));
            refused.Code.ShouldBe(ErpErrorCodes.Academics.RefundExceedsPrepayment);

            var refund = await _refunds.CreateAsync(new CreateUpdateStudentRefundDto { StudentNo = student.No, DocumentDate = Today, Amount = 20000m, Reason = "Overpayment" });
            refund = await _refunds.ApproveAsync(refund.Id);
            refund.Status.ShouldBe(AcademicRequestStatus.Approved);
            refund.PaymentVoucherNo.ShouldBe("PV-00001");

            // The refund is a voucher on the student's account; the same money cannot be promised twice.
            var voucherLine = (await GetRequiredService<IRepository<PaymentVoucherLine, Guid>>().GetListAsync(l => l.DocumentNo == refund.PaymentVoucherNo)).Single();
            voucherLine.AccountType.ShouldBe(GenJournalAccountType.Customer);
            voucherLine.AccountNo.ShouldBe(student.No);
            voucherLine.Amount.ShouldBe(20000m);

            var second = await _refunds.CreateAsync(new CreateUpdateStudentRefundDto { StudentNo = student.No, DocumentDate = Today, Amount = 1000m });
            var promised = await Should.ThrowAsync<BusinessException>(() => _refunds.ApproveAsync(second.Id));
            promised.Code.ShouldBe(ErpErrorCodes.Academics.RefundExceedsPrepayment);

            // Paying the voucher takes the prepayment off the account.
            var vouchers = GetRequiredService<IPaymentVoucherAppService>();
            var voucher = (await vouchers.GetListAsync(new GetPaymentVoucherListInput { Filter = refund.No })).Items.Single();
            await vouchers.UpdateAsync(voucher.Id, new CreateUpdatePaymentVoucherHeaderDto
            {
                DocumentDate = Today,
                PayMode = "BANK",
                PayingBankAccountNo = "WWB-OPERATING",
                Payee = voucher.Payee,
            });
            await vouchers.RunPostingAsync(voucher.Id);

            balance = await _students.GetBalanceAsync(student.Id);
            balance.Balance.ShouldBe(0m);
            balance.Prepayment.ShouldBe(0m);
        });
    }

    [Fact]
    public async Task A_Status_Change_Moves_The_Student_And_Graduation_Needs_Clearance()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var student = await SetUpStudentAsync("DHR");
            await RegisterAsync(student);

            // Units not passed and fees owed: not cleared.
            var graduation = await _statusChanges.CreateAsync(new CreateUpdateStudentStatusChangeDto
            {
                StudentNo = student.No,
                ChangeType = StudentChangeType.Graduation,
                EffectiveDate = Today,
            });
            graduation.No.ShouldBe("SC-00001");
            graduation.NewStatus.ShouldBe(StudentStatus.Alumni);

            var notCleared = await Should.ThrowAsync<BusinessException>(() => _statusChanges.ApproveAsync(graduation.Id));
            notCleared.Code.ShouldBe(ErpErrorCodes.Academics.StudentNotCleared);

            // A deferment takes the student out, and only then can a readmission bring them back.
            var early = await _statusChanges.CreateAsync(new CreateUpdateStudentStatusChangeDto { StudentNo = student.No, ChangeType = StudentChangeType.Readmission, EffectiveDate = Today });
            var notAway = await Should.ThrowAsync<BusinessException>(() => _statusChanges.ApproveAsync(early.Id));
            notAway.Code.ShouldBe(ErpErrorCodes.Academics.StatusChangeNotAllowed);

            var deferment = await _statusChanges.CreateAsync(new CreateUpdateStudentStatusChangeDto
            {
                StudentNo = student.No,
                ChangeType = StudentChangeType.Deferment,
                EffectiveDate = Today,
                ResumeDate = new DateTime(2026, 9, 1),
                Reason = "Medical",
            });
            deferment = await _statusChanges.ApproveAsync(deferment.Id);
            deferment.Status.ShouldBe(AcademicRequestStatus.Approved);
            deferment.PreviousStatus.ShouldBe(StudentStatus.Current);
            (await _students.GetAsync(student.Id)).Status.ShouldBe(StudentStatus.Deferred);

            await _statusChanges.ApproveAsync(early.Id);
            (await _students.GetAsync(student.Id)).Status.ShouldBe(StudentStatus.Current);

            // With the unit passed and the fees paid, the student graduates.
            foreach (var (examType, mark) in new[] { (ExamType.Cat, 20m), (ExamType.FinalExam, 50m) })
            {
                var document = await _examResults.CreateAsync(new CreateUpdateExamResultHeaderDto { ProgrammeCode = "DHR", UnitCode = "U101", ExamType = examType, DocumentDate = Today });
                await _examResults.SuggestLinesAsync(document.Id);
                var line = (await _examResultLines.GetListAsync(new GetDocumentLineListInput { DocumentNo = document.No })).Items.Single();
                await _examResultLines.UpdateAsync(line.Id, new CreateUpdateExamResultLineDto { DocumentNo = document.No, StudentNo = student.No, Mark = mark });
                await _examResults.RunPostingAsync(document.Id);
            }

            var bill = (await _registrations.GetListAsync(new GetSemesterRegistrationListInput { StudentNo = student.No })).Items.Single().BillNo;
            await ReceiveAsync(student, 30000m, bill);

            await _statusChanges.ApproveAsync(graduation.Id);
            (await _students.GetAsync(student.Id)).Status.ShouldBe(StudentStatus.Alumni);
        });
    }
}
