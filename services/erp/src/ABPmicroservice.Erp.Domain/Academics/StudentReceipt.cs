using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Academics;

/// <summary>
/// A fee receipt: money a student pays into a bank account. It may settle a bill or be paid ahead
/// of one; whatever it does not settle stays on the student's account as a prepayment, which the
/// next bill is charged against.
/// </summary>
public class StudentReceipt : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string StudentNo { get; private set; }
    public string StudentName { get; private set; }
    public DateTime PostingDate { get; private set; }

    /// <summary>The bank account the money is received into.</summary>
    public string BankAccountNo { get; private set; }

    /// <summary>How the student paid: a Payment Method code (cash, cheque, transfer, mobile money).</summary>
    public string PayMode { get; private set; }

    /// <summary>The payer's reference: the deposit slip, cheque or mobile money transaction number.</summary>
    public string ExternalDocumentNo { get; private set; }

    public decimal Amount { get; private set; }

    /// <summary>The bill the receipt settles; blank leaves the whole amount on account.</summary>
    public string AppliesToBillNo { get; private set; }

    public string Description { get; private set; }

    public AcademicDocumentStatus Status { get; private set; }
    public DateTime? PostedDate { get; private set; }
    public string PostedBy { get; private set; }

    protected StudentReceipt() { }

    public StudentReceipt(Guid id, string no, Student student, DateTime postingDate, decimal amount)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        SetStudent(student);
        SetAmount(postingDate, amount);
    }

    public bool IsOpen => Status == AcademicDocumentStatus.Open;

    public void SetStudent(Student student)
    {
        EnsureOpen();
        StudentNo = student.No;
        StudentName = student.FullName;
    }

    public void SetAmount(DateTime postingDate, decimal amount)
    {
        EnsureOpen();
        if (amount <= 0m)
        {
            throw new BusinessException(ErpErrorCodes.Academics.AmountNotPositive).WithData("documentNo", No);
        }

        PostingDate = postingDate.Date;
        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public void SetPayment(string bankAccountNo, string payMode, string externalDocumentNo, string appliesToBillNo, string description)
    {
        EnsureOpen();
        BankAccountNo = bankAccountNo.IsNullOrWhiteSpace() ? null : Check.Length(bankAccountNo.Trim(), nameof(bankAccountNo), ErpDomainConsts.MaxNoLength);
        PayMode = CodeTableEntity.NormalizeCode(Check.Length(payMode, nameof(payMode), ErpDomainConsts.MaxCodeLength));
        ExternalDocumentNo = externalDocumentNo.IsNullOrWhiteSpace()
            ? null
            : Check.Length(externalDocumentNo.Trim(), nameof(externalDocumentNo), ErpDomainConsts.MaxExternalDocumentNoLength);
        AppliesToBillNo = CodeTableEntity.NormalizeCode(Check.Length(appliesToBillNo, nameof(appliesToBillNo), ErpDomainConsts.MaxDocumentNoLength));
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    internal void MarkPosted(DateTime when, string by)
    {
        Status = AcademicDocumentStatus.Posted;
        PostedDate = when;
        PostedBy = by;
    }

    /// <summary>Only an open receipt may be changed; a posted one is the source of ledger entries.</summary>
    public void EnsureOpen()
    {
        if (Status != AcademicDocumentStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }
}

/// <summary>
/// A refund of money a student has paid ahead. Approving it raises a payment voucher on the
/// student's account for the amount, which is then approved and paid like any other voucher.
/// </summary>
public class StudentRefund : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string StudentNo { get; private set; }
    public string StudentName { get; private set; }
    public DateTime DocumentDate { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; }

    public AcademicRequestStatus Status { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public string ApprovedBy { get; private set; }

    /// <summary>The payment voucher raised for the refund.</summary>
    public string PaymentVoucherNo { get; private set; }

    protected StudentRefund() { }

    public StudentRefund(Guid id, string no, Student student, DateTime documentDate, decimal amount)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        Set(student, documentDate, amount, null);
    }

    public bool IsOpen => Status == AcademicRequestStatus.Open;

    public void Set(Student student, DateTime documentDate, decimal amount, string reason)
    {
        EnsureOpen();
        if (amount <= 0m)
        {
            throw new BusinessException(ErpErrorCodes.Academics.AmountNotPositive).WithData("documentNo", No);
        }

        StudentNo = student.No;
        StudentName = student.FullName;
        DocumentDate = documentDate.Date;
        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Reason = Check.Length(reason, nameof(reason), ErpDomainConsts.MaxDescriptionLength);
    }

    internal void MarkApproved(string paymentVoucherNo, DateTime when, string by)
    {
        Status = AcademicRequestStatus.Approved;
        PaymentVoucherNo = paymentVoucherNo;
        ApprovedDate = when;
        ApprovedBy = by;
    }

    public void EnsureOpen()
    {
        if (Status != AcademicRequestStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }
}

/// <summary>
/// Posts fee receipts and approves refunds.
/// <para>
/// A receipt debits the bank account and credits the student's customer account as a payment,
/// applied to the bill it names. A credit balance on that account is the student's prepayment:
/// it is what a refund may pay back, and a refund is paid through a payment voucher.
/// </para>
/// </summary>
public class StudentAccountEngine : DomainService
{
    public const string ReceiptSourceCode = "STUDRCPT";
    public const string RefundSourceType = "StudentRefund";

    private readonly IRepository<StudentReceipt, Guid> _receipts;
    private readonly IRepository<StudentRefund, Guid> _refunds;
    private readonly IRepository<Student, Guid> _students;
    private readonly IRepository<StudentBillHeader, Guid> _bills;
    private readonly IRepository<BankAccount, Guid> _bankAccounts;
    private readonly IRepository<PaymentVoucherHeader, Guid> _vouchers;
    private readonly StudentManager _studentManager;
    private readonly PaymentVoucherFactory _voucherFactory;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly ICurrentUser _currentUser;

    public StudentAccountEngine(
        IRepository<StudentReceipt, Guid> receipts,
        IRepository<StudentRefund, Guid> refunds,
        IRepository<Student, Guid> students,
        IRepository<StudentBillHeader, Guid> bills,
        IRepository<BankAccount, Guid> bankAccounts,
        IRepository<PaymentVoucherHeader, Guid> vouchers,
        StudentManager studentManager,
        PaymentVoucherFactory voucherFactory,
        GenJnlPostLine genJnlPostLine,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager,
        ICurrentUser currentUser
    )
    {
        _receipts = receipts;
        _refunds = refunds;
        _students = students;
        _bills = bills;
        _bankAccounts = bankAccounts;
        _vouchers = vouchers;
        _studentManager = studentManager;
        _voucherFactory = voucherFactory;
        _genJnlPostLine = genJnlPostLine;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
        _currentUser = currentUser;
    }

    /// <summary>What the student has paid ahead: the credit balance of the student's account, zero when the student owes.</summary>
    public static decimal PrepaymentOf(Customer customer) => customer == null ? 0m : Math.Max(0m, -customer.Balance);

    /// <summary>The bill a receipt may be applied to: a posted bill of the same student.</summary>
    public async Task EnsureBillOfStudentAsync(string billNo, string studentNo)
    {
        if (billNo.IsNullOrWhiteSpace())
        {
            return;
        }

        var no = billNo.Trim().ToUpperInvariant();
        if (!await _bills.AnyAsync(b => b.No == no && b.StudentNo == studentNo && b.Status == AcademicDocumentStatus.Posted))
        {
            throw new BusinessException(ErpErrorCodes.Academics.BillNotOfStudent).WithData("billNo", no).WithData("studentNo", studentNo);
        }
    }

    public async Task PostReceiptAsync(StudentReceipt receipt)
    {
        receipt.EnsureOpen();
        await _glSetupManager.CheckPostingDateAsync(receipt.PostingDate);

        if (receipt.BankAccountNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.AccountMissing).WithData("field", "Bank Account No.").WithData("setup", $"Receipt {receipt.No}");
        }

        var bankAccount = await _bankAccounts.FirstOrDefaultAsync(b => b.No == receipt.BankAccountNo)
            ?? throw new BusinessException(ErpErrorCodes.CashManagement.BankAccountNotFound).WithData("accountNo", receipt.BankAccountNo);
        if (bankAccount.Blocked)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.BankAccountBlocked).WithData("accountNo", bankAccount.No);
        }

        var student = await GetStudentAsync(receipt.StudentNo);
        await EnsureBillOfStudentAsync(receipt.AppliesToBillNo, student.No);

        var customer = await _studentManager.EnsureCustomerAsync(student);
        await _students.UpdateAsync(student, autoSave: true);

        var register = await _registerManager.OpenAsync(receipt.PostingDate, ReceiptSourceCode, receipt.No);
        var context = new GLPostingContext(register, ReceiptSourceCode);
        var description = receipt.Description.IsNullOrWhiteSpace() ? $"Fees received from {student.FullName}" : receipt.Description;

        await _genJnlPostLine.PostLineAsync(
            NewJournalLine(receipt, 10000, GenJournalAccountType.Customer, customer.No, description, -receipt.Amount, receipt.AppliesToBillNo),
            context
        );
        await _genJnlPostLine.PostLineAsync(
            NewJournalLine(receipt, 20000, GenJournalAccountType.BankAccount, bankAccount.No, description, receipt.Amount, null),
            context
        );

        await _registerManager.CloseAsync(register);

        receipt.MarkPosted(Clock.Now, _currentUser.UserName);
        await _receipts.UpdateAsync(receipt, autoSave: true);
    }

    /// <summary>
    /// Approves the refund and raises its payment voucher. A student can be refunded only what
    /// they have paid ahead and is not already promised in another approved refund still to be paid.
    /// </summary>
    public async Task ApproveRefundAsync(StudentRefund refund)
    {
        refund.EnsureOpen();

        var student = await GetStudentAsync(refund.StudentNo);
        var customer = await _studentManager.EnsureCustomerAsync(student);
        await _students.UpdateAsync(student, autoSave: true);

        var available = PrepaymentOf(customer) - await GetUnpaidRefundsAsync(student.No);
        if (refund.Amount > available)
        {
            throw new BusinessException(ErpErrorCodes.Academics.RefundExceedsPrepayment)
                .WithData("studentNo", student.No)
                .WithData("prepayment", Math.Max(0m, available).ToString("N2"));
        }

        var description = refund.Reason.IsNullOrWhiteSpace() ? $"Fee refund {refund.No}" : refund.Reason;
        var voucher = await _voucherFactory.CreateAsync(
            RefundSourceType,
            refund.No,
            refund.DocumentDate,
            student.FullName,
            $"Fee refund {refund.No} {student.No}",
            [new PaymentVoucherRequestLine(GenJournalAccountType.Customer, customer.No, description, refund.Amount)]
        );

        refund.MarkApproved(voucher.No, Clock.Now, _currentUser.UserName);
        await _refunds.UpdateAsync(refund, autoSave: true);
    }

    /// <summary>Refunds approved for the student whose vouchers have not been paid yet, so the same money is not promised twice.</summary>
    private async Task<decimal> GetUnpaidRefundsAsync(string studentNo)
    {
        var total = 0m;
        foreach (var approved in await _refunds.GetListAsync(r => r.StudentNo == studentNo && r.Status == AcademicRequestStatus.Approved))
        {
            var voucher = await _vouchers.FirstOrDefaultAsync(v => v.No == approved.PaymentVoucherNo);
            if (voucher != null && voucher.PostedDate == null)
            {
                total += approved.Amount;
            }
        }

        return total;
    }

    private GenJournalLine NewJournalLine(
        StudentReceipt receipt,
        int lineNo,
        GenJournalAccountType accountType,
        string accountNo,
        string description,
        decimal amount,
        string appliesToDocNo
    )
    {
        var line = new GenJournalLine(
            GuidGenerator.Create(),
            Guid.Empty,
            lineNo,
            receipt.PostingDate,
            GLEntryDocumentType.Payment,
            receipt.No,
            accountType,
            accountNo,
            description.Truncate(ErpDomainConsts.MaxDescriptionLength),
            amount
        );

        line.Update(
            receipt.PostingDate,
            GLEntryDocumentType.Payment,
            receipt.No,
            accountType,
            accountNo,
            description.Truncate(ErpDomainConsts.MaxDescriptionLength),
            amount,
            externalDocumentNo: receipt.ExternalDocumentNo,
            appliesToDocNo: appliesToDocNo
        );

        return line;
    }

    private async Task<Student> GetStudentAsync(string studentNo)
    {
        return await _students.FirstOrDefaultAsync(s => s.No == studentNo)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student").WithData("code", studentNo ?? string.Empty);
    }
}

/// <summary>
/// A change of a student's standing: a deferment, a readmission, a suspension, a discontinuation
/// or graduation. It is requested (Open) and approved; approving it is what changes the student.
/// </summary>
public class StudentStatusChange : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string StudentNo { get; private set; }
    public string StudentName { get; private set; }
    public StudentChangeType ChangeType { get; private set; }
    public DateTime EffectiveDate { get; private set; }

    /// <summary>When a deferred or suspended student is expected back.</summary>
    public DateTime? ResumeDate { get; private set; }

    public string Reason { get; private set; }

    public AcademicRequestStatus Status { get; private set; }

    /// <summary>The student's status before the change, kept so the record shows what it changed.</summary>
    public StudentStatus PreviousStatus { get; private set; }

    public StudentStatus NewStatus { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public string ApprovedBy { get; private set; }

    protected StudentStatusChange() { }

    public StudentStatusChange(Guid id, string no, Student student, StudentChangeType changeType, DateTime effectiveDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        Set(student, changeType, effectiveDate, null, null);
    }

    public bool IsOpen => Status == AcademicRequestStatus.Open;

    public void Set(Student student, StudentChangeType changeType, DateTime effectiveDate, DateTime? resumeDate, string reason)
    {
        EnsureOpen();
        StudentNo = student.No;
        StudentName = student.FullName;
        ChangeType = changeType;
        EffectiveDate = effectiveDate.Date;
        ResumeDate = changeType is StudentChangeType.Deferment or StudentChangeType.Suspension ? resumeDate?.Date : null;
        Reason = Check.Length(reason, nameof(reason), ErpDomainConsts.MaxDescriptionLength);
        PreviousStatus = student.Status;
        NewStatus = StatusAfter(changeType);
    }

    /// <summary>The status a change leaves the student in.</summary>
    public static StudentStatus StatusAfter(StudentChangeType changeType) =>
        changeType switch
        {
            StudentChangeType.Deferment => StudentStatus.Deferred,
            StudentChangeType.Readmission => StudentStatus.Current,
            StudentChangeType.Suspension => StudentStatus.Suspended,
            StudentChangeType.Discontinuation => StudentStatus.Discontinued,
            _ => StudentStatus.Alumni,
        };

    internal void MarkApproved(StudentStatus previousStatus, DateTime when, string by)
    {
        PreviousStatus = previousStatus;
        Status = AcademicRequestStatus.Approved;
        ApprovedDate = when;
        ApprovedBy = by;
    }

    public void EnsureOpen()
    {
        if (Status != AcademicRequestStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }
}

/// <summary>Approves changes of a student's standing, after checking that the student is where the change starts from.</summary>
public class StudentStatusChangeManager : DomainService
{
    private readonly IRepository<StudentStatusChange, Guid> _changes;
    private readonly IRepository<Student, Guid> _students;
    private readonly IRepository<StudentUnit, Guid> _studentUnits;
    private readonly IRepository<SemesterRegistration, Guid> _registrations;
    private readonly IRepository<Customer, Guid> _customers;
    private readonly ICurrentUser _currentUser;

    public StudentStatusChangeManager(
        IRepository<StudentStatusChange, Guid> changes,
        IRepository<Student, Guid> students,
        IRepository<StudentUnit, Guid> studentUnits,
        IRepository<SemesterRegistration, Guid> registrations,
        IRepository<Customer, Guid> customers,
        ICurrentUser currentUser
    )
    {
        _changes = changes;
        _students = students;
        _studentUnits = studentUnits;
        _registrations = registrations;
        _customers = customers;
        _currentUser = currentUser;
    }

    public async Task ApproveAsync(StudentStatusChange change)
    {
        change.EnsureOpen();

        var student = await _students.FirstOrDefaultAsync(s => s.No == change.StudentNo)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student").WithData("code", change.StudentNo);

        var allowed = change.ChangeType switch
        {
            // Only a student who is away can come back, and only one who is here can leave.
            StudentChangeType.Readmission => student.Status is StudentStatus.Deferred or StudentStatus.Suspended or StudentStatus.Discontinued,
            _ => student.IsActive,
        };

        if (!allowed)
        {
            throw new BusinessException(ErpErrorCodes.Academics.StatusChangeNotAllowed)
                .WithData("studentNo", student.No)
                .WithData("status", student.Status)
                .WithData("changeType", change.ChangeType);
        }

        if (change.ChangeType == StudentChangeType.Graduation)
        {
            await EnsureClearedAsync(student);
        }

        var previous = student.Status;
        student.SetStatus(change.NewStatus);
        await _students.UpdateAsync(student, autoSave: true);

        change.MarkApproved(previous, Clock.Now, _currentUser.UserName);
        await _changes.UpdateAsync(change, autoSave: true);
    }

    /// <summary>
    /// A student graduates once every unit they registered for is passed (on its latest sitting)
    /// and nothing is owed in fees.
    /// </summary>
    private async Task EnsureClearedAsync(Student student)
    {
        var submitted = (await _registrations.GetListAsync(r => r.StudentNo == student.No && r.Status == RegistrationStatus.Submitted))
            .ConvertAll(r => r.No);
        var units = (await _studentUnits.GetListAsync(u => u.StudentNo == student.No)).FindAll(u => submitted.Contains(u.RegistrationNo));

        if (units.Count == 0)
        {
            throw NotCleared(student, "no units taken");
        }

        var passed = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        foreach (var unit in units)
        {
            if (unit.Passed)
            {
                passed.Add(unit.UnitCode);
            }
        }

        var outstanding = units.Find(u => !passed.Contains(u.UnitCode));
        if (outstanding != null)
        {
            throw NotCleared(student, $"unit {outstanding.UnitCode} not passed");
        }

        var customer = student.CustomerNo == null ? null : await _customers.FirstOrDefaultAsync(c => c.No == student.CustomerNo);
        if (customer != null && customer.Balance > 0m)
        {
            throw NotCleared(student, $"fee balance of {customer.Balance:N2}");
        }
    }

    private static BusinessException NotCleared(Student student, string reason) =>
        new BusinessException(ErpErrorCodes.Academics.StudentNotCleared).WithData("studentNo", student.No).WithData("reason", reason);
}
