using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Academics;

/// <summary>
/// Academic Setup: one row per company with the number series the academic documents take their
/// numbers from, the posting groups a student's receivable account is opened with, and the rules
/// registration and grading follow.
/// </summary>
public class AcademicSetup : CompanyEntity
{
    public string ApplicationNos { get; private set; }
    public string StudentNos { get; private set; }
    public string RegistrationNos { get; private set; }
    public string BillingNos { get; private set; }
    public string ExamResultNos { get; private set; }

    /// <summary>The customer posting group of the account opened for a student on admission.</summary>
    public string StudentPostingGroup { get; private set; }

    public string StudentGenBusPostingGroup { get; private set; }

    /// <summary>A student owing more than <see cref="MaxFeeBalanceToRegister"/> cannot register when this is on.</summary>
    public bool CheckStudentBalance { get; private set; }

    public decimal MaxFeeBalanceToRegister { get; private set; }

    /// <summary>Submitting a registration bills the student from the fee structure when this is on.</summary>
    public bool BillOnRegistration { get; private set; } = true;

    /// <summary>Final scores are rounded to this many decimals before they are graded.</summary>
    public int ExamRoundingDecimals { get; private set; }

    protected AcademicSetup() { }

    public AcademicSetup(Guid id)
        : base(id) { }

    public void SetNumbering(string applicationNos, string studentNos, string registrationNos, string billingNos, string examResultNos)
    {
        ApplicationNos = Series(applicationNos, nameof(applicationNos));
        StudentNos = Series(studentNos, nameof(studentNos));
        RegistrationNos = Series(registrationNos, nameof(registrationNos));
        BillingNos = Series(billingNos, nameof(billingNos));
        ExamResultNos = Series(examResultNos, nameof(examResultNos));
    }

    public string ReceiptNos { get; private set; }
    public string RefundNos { get; private set; }
    public string StatusChangeNos { get; private set; }

    /// <summary>The series of the documents around a student's account and standing: receipts, refunds and status changes.</summary>
    public void SetStudentDocumentNumbering(string receiptNos, string refundNos, string statusChangeNos)
    {
        ReceiptNos = Series(receiptNos, nameof(receiptNos));
        RefundNos = Series(refundNos, nameof(refundNos));
        StatusChangeNos = Series(statusChangeNos, nameof(statusChangeNos));
    }

    public string AttendanceNos { get; private set; }
    public string HostelAllocationNos { get; private set; }
    public string ClinicVisitNos { get; private set; }
    public string LaundryNos { get; private set; }
    public string ShortCourseNos { get; private set; }

    /// <summary>The series of the campus documents: class attendance, hostel allocations, infirmary visits, laundry and short courses.</summary>
    public void SetCampusNumbering(string attendanceNos, string hostelAllocationNos, string clinicVisitNos, string laundryNos, string shortCourseNos)
    {
        AttendanceNos = Series(attendanceNos, nameof(attendanceNos));
        HostelAllocationNos = Series(hostelAllocationNos, nameof(hostelAllocationNos));
        ClinicVisitNos = Series(clinicVisitNos, nameof(clinicVisitNos));
        LaundryNos = Series(laundryNos, nameof(laundryNos));
        ShortCourseNos = Series(shortCourseNos, nameof(shortCourseNos));
    }

    /// <summary>A student who attended less of a unit's classes than this may not sit its final exam; 0 does not check.</summary>
    public decimal MinAttendancePct { get; private set; }

    /// <summary>What an express laundry order adds to the price of each item.</summary>
    public decimal LaundryExpressChargePct { get; private set; }

    /// <summary>The fee item an infirmary visit's charge is billed under.</summary>
    public string MedicalFeeItemCode { get; private set; }

    public void SetCampusOptions(decimal minAttendancePct, decimal laundryExpressChargePct, string medicalFeeItemCode)
    {
        if (minAttendancePct is < 0 or > 100 || laundryExpressChargePct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", minAttendancePct is < 0 or > 100 ? minAttendancePct : laundryExpressChargePct);
        }

        MinAttendancePct = minAttendancePct;
        LaundryExpressChargePct = laundryExpressChargePct;
        MedicalFeeItemCode = CodeTableEntity.NormalizeCode(Check.Length(medicalFeeItemCode, nameof(medicalFeeItemCode), ErpDomainConsts.MaxCodeLength));
    }

    public void SetPostingGroups(string studentPostingGroup, string studentGenBusPostingGroup)
    {
        StudentPostingGroup = CodeTableEntity.NormalizeCode(
            Check.Length(studentPostingGroup, nameof(studentPostingGroup), ErpDomainConsts.MaxPostingGroupLength)
        );
        StudentGenBusPostingGroup = CodeTableEntity.NormalizeCode(
            Check.Length(studentGenBusPostingGroup, nameof(studentGenBusPostingGroup), ErpDomainConsts.MaxPostingGroupLength)
        );
    }

    public void SetOptions(bool checkStudentBalance, decimal maxFeeBalanceToRegister, bool billOnRegistration, int examRoundingDecimals)
    {
        if (maxFeeBalanceToRegister < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Max. Fee Balance To Register");
        }

        CheckStudentBalance = checkStudentBalance;
        MaxFeeBalanceToRegister = maxFeeBalanceToRegister;
        BillOnRegistration = billOnRegistration;
        ExamRoundingDecimals = Math.Clamp(examRoundingDecimals, 0, 2);
    }

    private static string Series(string value, string name) =>
        value.IsNullOrWhiteSpace() ? null : Check.Length(value.Trim(), name, ErpDomainConsts.MaxNoSeriesCodeLength);
}

public class AcademicSetupManager : DomainService
{
    private readonly IRepository<AcademicSetup, Guid> _repository;

    public AcademicSetupManager(IRepository<AcademicSetup, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>The current company's setup, created blank on first use.</summary>
    public async Task<AcademicSetup> GetAsync()
    {
        return await _repository.FirstOrDefaultAsync()
            ?? await _repository.InsertAsync(new AcademicSetup(GuidGenerator.Create()), autoSave: true);
    }
}

/// <summary>What the calendar tables share: a first and a last day, and a mark on the one in progress.</summary>
public abstract class AcademicPeriodEntity : CodeTableEntity
{
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    /// <summary>The period in progress; documents created without one take it.</summary>
    public bool Current { get; private set; }

    protected AcademicPeriodEntity() { }

    /// <summary>Another period has become the current one.</summary>
    public void ClearCurrent() => Current = false;

    protected AcademicPeriodEntity(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetPeriod(DateTime? startDate, DateTime? endDate, bool current)
    {
        if (startDate.HasValue && endDate.HasValue && endDate.Value.Date < startDate.Value.Date)
        {
            throw new BusinessException(ErpErrorCodes.Academics.PeriodReversed).WithData("code", Code);
        }

        StartDate = startDate?.Date;
        EndDate = endDate?.Date;
        Current = current;
    }
}

public class AcademicYear : AcademicPeriodEntity
{
    protected AcademicYear() { }

    public AcademicYear(Guid id, string code, string description)
        : base(id, code, description) { }
}

/// <summary>A teaching term of an academic year, and the days on which students may register for it.</summary>
public class Semester : AcademicPeriodEntity
{
    public string AcademicYearCode { get; private set; }
    public DateTime? RegistrationFrom { get; private set; }
    public DateTime? RegistrationTo { get; private set; }

    protected Semester() { }

    public Semester(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(string academicYearCode, DateTime? registrationFrom, DateTime? registrationTo)
    {
        if (registrationFrom.HasValue && registrationTo.HasValue && registrationTo.Value.Date < registrationFrom.Value.Date)
        {
            throw new BusinessException(ErpErrorCodes.Academics.PeriodReversed).WithData("code", Code);
        }

        AcademicYearCode = NormalizeCode(Check.Length(academicYearCode, nameof(academicYearCode), ErpDomainConsts.MaxCodeLength));
        RegistrationFrom = registrationFrom?.Date;
        RegistrationTo = registrationTo?.Date;
    }

    /// <summary>Whether registration is open on a day; a semester with no registration dates is always open.</summary>
    public bool IsRegistrationOpen(DateTime date) =>
        (!RegistrationFrom.HasValue || date.Date >= RegistrationFrom.Value) && (!RegistrationTo.HasValue || date.Date <= RegistrationTo.Value);
}

/// <summary>A group of students admitted together, e.g. the September intake.</summary>
public class Intake : AcademicPeriodEntity
{
    public string AcademicYearCode { get; private set; }

    protected Intake() { }

    public Intake(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(string academicYearCode)
    {
        AcademicYearCode = NormalizeCode(Check.Length(academicYearCode, nameof(academicYearCode), ErpDomainConsts.MaxCodeLength));
    }
}

/// <summary>
/// An exam category: the grading scheme a programme is examined under. Its grades are the
/// <see cref="GradingBand"/>s and its assessment parts the <see cref="ExamComponent"/>s of the same code.
/// </summary>
public class ExamCategory : CodeTableEntity
{
    /// <summary>No more results may be entered for programmes of the category.</summary>
    public bool BlockResultsEntry { get; private set; }

    protected ExamCategory() { }

    public ExamCategory(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(bool blockResultsEntry) => BlockResultsEntry = blockResultsEntry;
}

/// <summary>One grade of an exam category: the range of final scores that earn it.</summary>
public class GradingBand : CompanyEntity
{
    public string ExamCategoryCode { get; private set; }
    public string Grade { get; private set; }
    public string Description { get; private set; }
    public decimal FromMark { get; private set; }
    public decimal ToMark { get; private set; }

    /// <summary>Grade points, for a mean on a points scale.</summary>
    public decimal Points { get; private set; }

    /// <summary>What a result slip says against the grade, e.g. "Distinction".</summary>
    public string Remarks { get; private set; }

    public bool Passed { get; private set; }

    protected GradingBand() { }

    public GradingBand(Guid id, string examCategoryCode, string grade)
        : base(id)
    {
        SetKey(examCategoryCode, grade);
    }

    public void SetKey(string examCategoryCode, string grade)
    {
        ExamCategoryCode = Check.NotNullOrWhiteSpace(examCategoryCode, nameof(examCategoryCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        Grade = Check.NotNullOrWhiteSpace(grade, nameof(grade), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
    }

    public void Set(string description, decimal fromMark, decimal toMark, decimal points, string remarks, bool passed)
    {
        if (fromMark < 0 || toMark < fromMark)
        {
            throw new BusinessException(ErpErrorCodes.Academics.InvalidGradingBand).WithData("grade", Grade);
        }

        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        FromMark = fromMark;
        ToMark = toMark;
        Points = points;
        Remarks = Check.Length(remarks, nameof(remarks), ErpDomainConsts.MaxNameLength);
        Passed = passed;
    }

    public bool Covers(decimal score) => score >= FromMark && score <= ToMark;
}

/// <summary>
/// One part of the assessment of an exam category: what it is marked out of and the share of the
/// final score it contributes. The shares of a category's parts are expected to add up to 100.
/// </summary>
public class ExamComponent : CompanyEntity
{
    public string ExamCategoryCode { get; private set; }
    public ExamType ExamType { get; private set; }
    public string Description { get; private set; }
    public decimal MaxScore { get; private set; }
    public decimal ContributionPct { get; private set; }

    protected ExamComponent() { }

    public ExamComponent(Guid id, string examCategoryCode, ExamType examType)
        : base(id)
    {
        SetKey(examCategoryCode, examType);
    }

    public void SetKey(string examCategoryCode, ExamType examType)
    {
        ExamCategoryCode = Check.NotNullOrWhiteSpace(examCategoryCode, nameof(examCategoryCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        ExamType = examType;
    }

    public void Set(string description, decimal maxScore, decimal contributionPct)
    {
        if (maxScore <= 0 || contributionPct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.Academics.InvalidExamComponent).WithData("examType", ExamType);
        }

        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        MaxScore = maxScore;
        ContributionPct = contributionPct;
    }
}
