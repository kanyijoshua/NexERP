using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Academics;

/// <summary>
/// A student bill: the fees charged to one student for a stage and semester. It is prepared
/// (Open) and then posted to the student's customer account and the fee income accounts.
/// </summary>
public class StudentBillHeader : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string StudentNo { get; private set; }
    public string StudentName { get; private set; }
    public DateTime PostingDate { get; private set; }

    public string ProgrammeCode { get; private set; }
    public string StageCode { get; private set; }
    public string SemesterCode { get; private set; }
    public string AcademicYearCode { get; private set; }

    public string Description { get; private set; }

    /// <summary>The semester registration that raised the bill; blank on a bill entered by hand.</summary>
    public string RegistrationNo { get; internal set; }

    public AcademicDocumentStatus Status { get; private set; }
    public DateTime? PostedDate { get; private set; }
    public string PostedBy { get; private set; }

    /// <summary>Total of the lines; kept on the header so a list of bills needs no join.</summary>
    public decimal TotalAmount { get; internal set; }

    protected StudentBillHeader() { }

    public StudentBillHeader(Guid id, string no, Student student, DateTime postingDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        SetStudent(student);
        PostingDate = postingDate.Date;
    }

    public bool IsOpen => Status == AcademicDocumentStatus.Open;

    public void SetStudent(Student student)
    {
        EnsureOpen();
        StudentNo = student.No;
        StudentName = student.FullName;
        ProgrammeCode = student.ProgrammeCode;
    }

    public void SetDetails(DateTime postingDate, string stageCode, string semesterCode, string academicYearCode, string description)
    {
        EnsureOpen();
        PostingDate = postingDate.Date;
        StageCode = CodeTableEntity.NormalizeCode(Check.Length(stageCode, nameof(stageCode), ErpDomainConsts.MaxCodeLength));
        SemesterCode = CodeTableEntity.NormalizeCode(Check.Length(semesterCode, nameof(semesterCode), ErpDomainConsts.MaxCodeLength));
        AcademicYearCode = CodeTableEntity.NormalizeCode(Check.Length(academicYearCode, nameof(academicYearCode), ErpDomainConsts.MaxCodeLength));
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    internal void MarkPosted(DateTime when, string by)
    {
        Status = AcademicDocumentStatus.Posted;
        PostedDate = when;
        PostedBy = by;
    }

    /// <summary>Only an open bill may be changed; a posted one is the source of ledger entries.</summary>
    public void EnsureOpen()
    {
        if (Status != AcademicDocumentStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }
}

/// <summary>One thing a student is charged for outside the fee structure, under a fee item.</summary>
public record StudentCharge(string FeeItemCode, string Description, decimal Amount);

/// <summary>One fee item charged on a student bill.</summary>
public class StudentBillLine : CompanyEntity
{
    public string DocumentNo { get; private set; }
    public int LineNo { get; private set; }
    public string FeeItemCode { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }

    protected StudentBillLine() { }

    public StudentBillLine(Guid id, string documentNo, int lineNo, FeeItem feeItem, decimal amount)
        : base(id)
    {
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        LineNo = lineNo;
        Set(feeItem, null, amount);
    }

    /// <param name="description">Blank takes the fee item's.</param>
    public void Set(FeeItem feeItem, string description, decimal amount)
    {
        if (amount < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Amount");
        }

        FeeItemCode = feeItem.Code;
        Description = description.IsNullOrWhiteSpace()
            ? feeItem.Description
            : Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Amount = amount;
    }
}

/// <summary>
/// Prepares and posts student bills.
/// <para>
/// Posting debits the student's customer account with the total as an invoice and credits each
/// fee item's income account with its line, so what a student owes is followed in the receivables
/// ledger and a fee payment is an ordinary receipt applied to it.
/// </para>
/// </summary>
public class StudentBillingEngine : DomainService
{
    public const string SourceCode = "STUDBILL";

    private readonly IRepository<StudentBillHeader, Guid> _headers;
    private readonly IRepository<StudentBillLine, Guid> _lines;
    private readonly IRepository<Student, Guid> _students;
    private readonly IRepository<FeeItem, Guid> _feeItems;
    private readonly IRepository<FeeStructureLine, Guid> _feeStructure;
    private readonly StudentManager _studentManager;
    private readonly AcademicSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly ICurrentUser _currentUser;

    public StudentBillingEngine(
        IRepository<StudentBillHeader, Guid> headers,
        IRepository<StudentBillLine, Guid> lines,
        IRepository<Student, Guid> students,
        IRepository<FeeItem, Guid> feeItems,
        IRepository<FeeStructureLine, Guid> feeStructure,
        StudentManager studentManager,
        AcademicSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        GenJnlPostLine genJnlPostLine,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager,
        ICurrentUser currentUser
    )
    {
        _headers = headers;
        _lines = lines;
        _students = students;
        _feeItems = feeItems;
        _feeStructure = feeStructure;
        _studentManager = studentManager;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _genJnlPostLine = genJnlPostLine;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Bills a student for something outside the fee structure (a hostel place, an infirmary
    /// visit) and posts the bill at once, so the charge is on the student's account like any fee.
    /// </summary>
    public async Task<StudentBillHeader> ChargeAsync(Student student, DateTime postingDate, string semesterCode, string description, IReadOnlyList<StudentCharge> charges)
    {
        var setup = await _setupManager.GetAsync();
        var no = (await _noSeriesManager.ResolveNoAsync(setup.BillingNos, null, postingDate)).ToUpperInvariant();

        var bill = new StudentBillHeader(GuidGenerator.Create(), no, student, postingDate);
        bill.SetDetails(postingDate, student.CurrentStageCode, semesterCode ?? student.CurrentSemesterCode, null, description);
        await _headers.InsertAsync(bill, autoSave: true);

        var lineNo = 0;
        foreach (var charge in charges.Where(c => c.Amount > 0m))
        {
            var feeItem = await _feeItems.FirstOrDefaultAsync(f => f.Code == charge.FeeItemCode)
                ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Fee Item").WithData("code", charge.FeeItemCode ?? string.Empty);

            lineNo += 10000;
            var line = new StudentBillLine(GuidGenerator.Create(), bill.No, lineNo, feeItem, charge.Amount);
            line.Set(feeItem, charge.Description, charge.Amount);
            await _lines.InsertAsync(line, autoSave: true);
        }

        await PostAsync(bill);
        return bill;
    }

    /// <summary>Stores the total of the lines on the header.</summary>
    public async Task UpdateTotalsAsync(StudentBillHeader header)
    {
        var lines = await _lines.GetListAsync(l => l.DocumentNo == header.No);
        header.TotalAmount = lines.Sum(l => l.Amount);
        await _headers.UpdateAsync(header, autoSave: true);
    }

    /// <summary>Whether the fee structure charges anything for a stage and semester to a student of a mode of study.</summary>
    public async Task<bool> HasFeesAsync(string programmeCode, string stageCode, string semesterCode, StudyMode studyMode)
    {
        return (await _feeStructure.GetListAsync(f => f.ProgrammeCode == programmeCode && f.StageCode == stageCode)).Any(f =>
            f.AppliesTo(semesterCode, studyMode) && f.Amount > 0m
        );
    }

    /// <summary>
    /// Fills the bill from the fee structure of the bill's programme and stage: every line that
    /// applies to the bill's semester and the student's mode of study. Fee items already on the
    /// bill are left as they are. Returns the number of lines added.
    /// </summary>
    public async Task<int> SuggestLinesAsync(StudentBillHeader header)
    {
        header.EnsureOpen();

        if (header.StageCode.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Academics.StageRequired).WithData("documentNo", header.No);
        }

        var student = await GetStudentAsync(header.StudentNo);
        var existing = await _lines.GetListAsync(l => l.DocumentNo == header.No);
        var onBill = existing.Select(l => l.FeeItemCode).ToHashSet(StringComparer.Ordinal);
        var lineNo = existing.Count == 0 ? 0 : existing.Max(l => l.LineNo);
        var added = 0;

        var structure = (await _feeStructure.GetListAsync(f => f.ProgrammeCode == header.ProgrammeCode && f.StageCode == header.StageCode))
            .Where(f => f.AppliesTo(header.SemesterCode, student.StudyMode) && f.Amount > 0m)
            .GroupBy(f => f.FeeItemCode, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal);

        var feeItems = (await _feeItems.GetListAsync()).ToDictionary(f => f.Code, StringComparer.Ordinal);

        foreach (var group in structure)
        {
            if (onBill.Contains(group.Key) || !feeItems.TryGetValue(group.Key, out var feeItem))
            {
                continue;
            }

            lineNo += 10000;
            await _lines.InsertAsync(new StudentBillLine(GuidGenerator.Create(), header.No, lineNo, feeItem, group.Sum(f => f.Amount)), autoSave: true);
            added++;
        }

        await UpdateTotalsAsync(header);
        return added;
    }

    public async Task PostAsync(StudentBillHeader header)
    {
        header.EnsureOpen();
        await _glSetupManager.CheckPostingDateAsync(header.PostingDate);

        var lines = (await _lines.GetListAsync(l => l.DocumentNo == header.No)).Where(l => l.Amount != 0m).OrderBy(l => l.LineNo).ToList();
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NothingToPost).WithData("documentNo", header.No);
        }

        var student = await GetStudentAsync(header.StudentNo);
        var customer = await _studentManager.EnsureCustomerAsync(student);
        await _students.UpdateAsync(student, autoSave: true);

        // Checked before anything is written: a bill is posted whole or not at all.
        var accounts = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var code in lines.Select(l => l.FeeItemCode).Distinct())
        {
            var feeItem = await _feeItems.FirstOrDefaultAsync(f => f.Code == code)
                ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Fee Item").WithData("code", code);

            accounts[code] = feeItem.GLAccountNo.IsNullOrWhiteSpace()
                ? throw new BusinessException(ErpErrorCodes.PostingSetup.AccountMissing).WithData("field", "G/L Account No.").WithData("setup", $"Fee Item {code}")
                : feeItem.GLAccountNo;
        }

        var register = await _registerManager.OpenAsync(header.PostingDate, SourceCode, header.No);
        var context = new GLPostingContext(register, SourceCode);
        var description = header.Description.IsNullOrWhiteSpace() ? $"Fees {header.StageCode} {header.SemesterCode}".Trim() : header.Description;

        foreach (var line in lines)
        {
            await _genJnlPostLine.PostGLDirectAsync(
                accounts[line.FeeItemCode],
                header.PostingDate,
                GLEntryDocumentType.None,
                header.No,
                line.Description.IsNullOrWhiteSpace() ? description : line.Description,
                -line.Amount,
                student.No,
                context: context
            );
        }

        var total = lines.Sum(l => l.Amount);

        await _genJnlPostLine.PostLineAsync(
            new GenJournalLine(
                GuidGenerator.Create(),
                Guid.Empty,
                1,
                header.PostingDate,
                GLEntryDocumentType.Invoice,
                header.No,
                GenJournalAccountType.Customer,
                customer.No,
                description,
                total
            ),
            context
        );

        await _registerManager.CloseAsync(register);

        header.TotalAmount = total;
        header.MarkPosted(Clock.Now, _currentUser.UserName);
        await _headers.UpdateAsync(header, autoSave: true);
    }

    private async Task<Student> GetStudentAsync(string studentNo)
    {
        return await _students.FirstOrDefaultAsync(s => s.No == studentNo)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student").WithData("code", studentNo ?? string.Empty);
    }
}
