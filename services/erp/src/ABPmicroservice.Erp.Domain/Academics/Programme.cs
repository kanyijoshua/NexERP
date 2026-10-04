using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Academics;

/// <summary>A programme of study: the course a student is admitted to and progresses through stage by stage.</summary>
public class Programme : CodeTableEntity
{
    public ProgrammeLevel Level { get; private set; }

    /// <summary>The grading scheme the programme's units are examined under.</summary>
    public string ExamCategoryCode { get; private set; }

    public int DurationMonths { get; private set; }
    public int MinimumCapacity { get; private set; }
    public int MaximumCapacity { get; private set; }

    /// <summary>The series its students are numbered from; blank uses the Academic Setup's.</summary>
    public string StudentNos { get; private set; }

    public string StudentNoPrefix { get; private set; }
    public string StudentNoSuffix { get; private set; }

    /// <summary>A programme that is not active takes no applications and no registrations.</summary>
    public bool Active { get; private set; } = true;

    protected Programme() { }

    public Programme(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(ProgrammeLevel level, string examCategoryCode, int durationMonths, int minimumCapacity, int maximumCapacity, bool active)
    {
        if (durationMonths < 0 || minimumCapacity < 0 || maximumCapacity < 0 || (maximumCapacity > 0 && minimumCapacity > maximumCapacity))
        {
            throw new BusinessException(ErpErrorCodes.Academics.InvalidCapacity).WithData("code", Code);
        }

        Level = level;
        ExamCategoryCode = NormalizeCode(Check.Length(examCategoryCode, nameof(examCategoryCode), ErpDomainConsts.MaxCodeLength));
        DurationMonths = durationMonths;
        MinimumCapacity = minimumCapacity;
        MaximumCapacity = maximumCapacity;
        Active = active;
    }

    public void SetStudentNumbering(string studentNos, string studentNoPrefix, string studentNoSuffix)
    {
        StudentNos = studentNos.IsNullOrWhiteSpace() ? null : Check.Length(studentNos.Trim(), nameof(studentNos), ErpDomainConsts.MaxNoSeriesCodeLength);
        StudentNoPrefix = Affix(studentNoPrefix, nameof(studentNoPrefix));
        StudentNoSuffix = Affix(studentNoSuffix, nameof(studentNoSuffix));
    }

    /// <summary>A student number of the programme: the number from the series between the programme's prefix and suffix.</summary>
    public string FormatStudentNo(string no) => $"{StudentNoPrefix}{no}{StudentNoSuffix}".ToUpperInvariant();

    private static string Affix(string value, string name) =>
        value.IsNullOrWhiteSpace() ? null : Check.Length(value.Trim(), name, AcademicConsts.MaxStudentNoAffixLength).ToUpperInvariant();
}

public static class AcademicConsts
{
    /// <summary>Short enough that prefix, series number and suffix still fit a No. field.</summary>
    public const int MaxStudentNoAffixLength = 5;
}

/// <summary>A stage of a programme (a year or a module), in the order students go through them.</summary>
public class ProgrammeStage : CompanyEntity
{
    public string ProgrammeCode { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }

    /// <summary>The position of the stage in the programme; the first stage has the lowest.</summary>
    public int Sequence { get; private set; }

    public bool FinalStage { get; private set; }

    /// <summary>The fewest and the most units a student may register for in one semester of the stage; 0 is no limit.</summary>
    public int MinimumUnits { get; private set; }

    public int MaximumUnits { get; private set; }

    protected ProgrammeStage() { }

    public ProgrammeStage(Guid id, string programmeCode, string code)
        : base(id)
    {
        SetKey(programmeCode, code);
    }

    public void SetKey(string programmeCode, string code)
    {
        ProgrammeCode = Check.NotNullOrWhiteSpace(programmeCode, nameof(programmeCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
    }

    public void Set(string description, int sequence, bool finalStage, int minimumUnits, int maximumUnits)
    {
        if (minimumUnits < 0 || maximumUnits < 0 || (maximumUnits > 0 && minimumUnits > maximumUnits))
        {
            throw new BusinessException(ErpErrorCodes.Academics.InvalidCapacity).WithData("code", Code);
        }

        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Sequence = sequence;
        FinalStage = finalStage;
        MinimumUnits = minimumUnits;
        MaximumUnits = maximumUnits;
    }
}

/// <summary>A unit (subject) taught in a stage of a programme.</summary>
public class CourseUnit : CompanyEntity
{
    public string ProgrammeCode { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public string StageCode { get; private set; }

    /// <summary>The semester the unit is taught in; blank is every semester of its stage.</summary>
    public string SemesterCode { get; private set; }

    public UnitType UnitType { get; private set; }
    public decimal CreditHours { get; private set; }

    /// <summary>A unit of the same programme that must have been passed before this one is taken.</summary>
    public string PrerequisiteUnitCode { get; private set; }

    public bool Blocked { get; private set; }

    protected CourseUnit() { }

    public CourseUnit(Guid id, string programmeCode, string code, string stageCode)
        : base(id)
    {
        SetKey(programmeCode, code);
        StageCode = Check.NotNullOrWhiteSpace(stageCode, nameof(stageCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
    }

    public void SetKey(string programmeCode, string code)
    {
        ProgrammeCode = Check.NotNullOrWhiteSpace(programmeCode, nameof(programmeCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
    }

    public void Set(
        string description,
        string stageCode,
        string semesterCode,
        UnitType unitType,
        decimal creditHours,
        string prerequisiteUnitCode,
        bool blocked
    )
    {
        if (creditHours < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Credit Hours");
        }

        var prerequisite = CodeTableEntity.NormalizeCode(Check.Length(prerequisiteUnitCode, nameof(prerequisiteUnitCode), ErpDomainConsts.MaxCodeLength));
        if (prerequisite == Code)
        {
            throw new BusinessException(ErpErrorCodes.Academics.UnitIsOwnPrerequisite).WithData("unit", Code);
        }

        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        StageCode = Check.NotNullOrWhiteSpace(stageCode, nameof(stageCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        SemesterCode = CodeTableEntity.NormalizeCode(Check.Length(semesterCode, nameof(semesterCode), ErpDomainConsts.MaxCodeLength));
        UnitType = unitType;
        CreditHours = creditHours;
        PrerequisiteUnitCode = prerequisite;
        Blocked = blocked;
    }

    /// <summary>Whether the unit is on offer in a semester.</summary>
    public bool IsTaughtIn(string semesterCode) => SemesterCode == null || SemesterCode == semesterCode;
}

/// <summary>Something students are charged for, and the income account the charge is credited to.</summary>
public class FeeItem : CodeTableEntity
{
    public FeeType FeeType { get; private set; }
    public string GLAccountNo { get; private set; }

    /// <summary>What a bill line for the item starts with when the fee structure has no amount for it.</summary>
    public decimal DefaultAmount { get; private set; }

    public bool TuitionFee { get; private set; }

    protected FeeItem() { }

    public FeeItem(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(FeeType feeType, string glAccountNo, decimal defaultAmount, bool tuitionFee)
    {
        if (defaultAmount < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Default Amount");
        }

        FeeType = feeType;
        GLAccountNo = glAccountNo.IsNullOrWhiteSpace() ? null : Check.Length(glAccountNo.Trim(), nameof(glAccountNo), ErpDomainConsts.MaxNoLength);
        DefaultAmount = defaultAmount;
        TuitionFee = tuitionFee;
    }
}

/// <summary>
/// A line of the fee structure: what one fee item costs for a stage of a programme. A blank
/// semester is charged in every semester of the stage, and a mode of study of None to every student.
/// </summary>
public class FeeStructureLine : CompanyEntity
{
    public string ProgrammeCode { get; private set; }
    public string StageCode { get; private set; }
    public string SemesterCode { get; private set; }
    public StudyMode StudyMode { get; private set; }
    public string FeeItemCode { get; private set; }
    public decimal Amount { get; private set; }

    protected FeeStructureLine() { }

    public FeeStructureLine(Guid id, string programmeCode, string stageCode, string feeItemCode)
        : base(id)
    {
        SetKey(programmeCode, stageCode, null, StudyMode.None, feeItemCode);
    }

    public void SetKey(string programmeCode, string stageCode, string semesterCode, StudyMode studyMode, string feeItemCode)
    {
        ProgrammeCode = Check.NotNullOrWhiteSpace(programmeCode, nameof(programmeCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        StageCode = Check.NotNullOrWhiteSpace(stageCode, nameof(stageCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        SemesterCode = CodeTableEntity.NormalizeCode(Check.Length(semesterCode, nameof(semesterCode), ErpDomainConsts.MaxCodeLength));
        StudyMode = studyMode;
        FeeItemCode = Check.NotNullOrWhiteSpace(feeItemCode, nameof(feeItemCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
    }

    public void SetAmount(decimal amount)
    {
        if (amount < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Amount");
        }

        Amount = amount;
    }

    /// <summary>Whether the line is charged to a student of a mode of study in a semester.</summary>
    public bool AppliesTo(string semesterCode, StudyMode studyMode) =>
        (SemesterCode == null || SemesterCode == semesterCode) && (StudyMode == StudyMode.None || StudyMode == studyMode);
}
