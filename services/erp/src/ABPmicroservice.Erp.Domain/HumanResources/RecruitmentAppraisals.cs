using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>Job Vacancy: positions to fill, open for applications until it is filled or closed.</summary>
public class JobVacancy : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Title { get; private set; }
    public string Department { get; private set; }
    public string Description { get; private set; }
    public int Positions { get; private set; } = 1;
    public int HiredCount { get; private set; }
    public DateTime? OpeningDate { get; private set; }
    public DateTime? ClosingDate { get; private set; }
    public JobVacancyStatus Status { get; private set; }

    protected JobVacancy() { }

    public JobVacancy(Guid id, string no)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
    }

    public void Set(string title, string department, string description, int positions, DateTime? openingDate, DateTime? closingDate)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), ErpDomainConsts.MaxDescriptionLength).Trim();
        Department = Check.Length(department, nameof(department), ErpDomainConsts.MaxNameLength);
        Description = Check.Length(description, nameof(description), 2000);
        Positions = Math.Max(1, positions);
        OpeningDate = openingDate?.Date;
        ClosingDate = closingDate?.Date;
    }

    public void Open() => Status = JobVacancyStatus.Open;

    public void Close() => Status = JobVacancyStatus.Closed;

    public void EnsureOpen()
    {
        if (Status != JobVacancyStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.VacancyNotOpen).WithData("no", No);
        }
    }

    /// <summary>One more position filled; the vacancy closes once every position is.</summary>
    internal void RecordHire()
    {
        HiredCount++;
        if (HiredCount >= Positions)
        {
            Status = JobVacancyStatus.Closed;
        }
    }
}

/// <summary>Job Applicant: someone applying for a vacancy, moved through the stages until hired or rejected.</summary>
public class JobApplicant : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string VacancyNo { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNo { get; private set; }
    public DateTime AppliedDate { get; private set; }
    public ApplicantStage Stage { get; private set; }

    /// <summary>How the applicant was rated in screening and interviews, 0 to 5.</summary>
    public int Rating { get; private set; }

    public decimal ExpectedSalary { get; private set; }
    public string Notes { get; private set; }

    /// <summary>The employee the applicant became.</summary>
    public string EmployeeNo { get; private set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    protected JobApplicant() { }

    public JobApplicant(Guid id, string no, string vacancyNo, DateTime appliedDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        VacancyNo = Check.NotNullOrWhiteSpace(vacancyNo, nameof(vacancyNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        AppliedDate = appliedDate.Date;
        Stage = ApplicantStage.New;
    }

    public void Set(string firstName, string lastName, string email, string phoneNo, int rating, decimal expectedSalary, string notes)
    {
        FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), 30).Trim();
        LastName = Check.Length(lastName?.Trim(), nameof(lastName), 30);
        Email = Check.Length(email?.Trim(), nameof(email), 80);
        PhoneNo = Check.Length(phoneNo?.Trim(), nameof(phoneNo), 30);
        Rating = Math.Clamp(rating, 0, 5);
        ExpectedSalary = Math.Max(0m, expectedSalary);
        Notes = Check.Length(notes, nameof(notes), 2000);
    }

    /// <summary>Moves to another stage; a hired or rejected applicant stays where they are, and hiring goes through <see cref="RecruitmentManager.HireAsync"/>.</summary>
    public void MoveTo(ApplicantStage stage)
    {
        if (Stage is ApplicantStage.Hired or ApplicantStage.Rejected || stage == ApplicantStage.Hired)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.ApplicantStageInvalid)
                .WithData("no", No)
                .WithData("stage", Stage.ToString())
                .WithData("target", stage.ToString());
        }

        Stage = stage;
    }

    internal void Hire(string employeeNo)
    {
        if (Stage is ApplicantStage.Hired or ApplicantStage.Rejected)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.ApplicantStageInvalid)
                .WithData("no", No)
                .WithData("stage", Stage.ToString())
                .WithData("target", nameof(ApplicantStage.Hired));
        }

        EmployeeNo = employeeNo;
        Stage = ApplicantStage.Hired;
    }
}

/// <summary>Appraisal Period: the review cycle appraisals belong to; a closed period takes no new appraisals.</summary>
public class AppraisalPeriod : CodeTableEntity
{
    public DateTime StartingDate { get; private set; }
    public DateTime EndingDate { get; private set; }
    public bool Closed { get; private set; }

    protected AppraisalPeriod() { }

    public AppraisalPeriod(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(DateTime startingDate, DateTime endingDate, bool closed)
    {
        if (endingDate.Date < startingDate.Date)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.InvalidDateRange);
        }

        StartingDate = startingDate.Date;
        EndingDate = endingDate.Date;
        Closed = closed;
    }
}

/// <summary>
/// Appraisal: an employee's objectives for a period, weighed and scored first by the employee and
/// then by the reviewer. The overall score is the reviewer's scores averaged by weight.
/// </summary>
public class Appraisal : CompanyAggregateRoot, IHasNo
{
    public const decimal MaxScore = 5m;

    public string No { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string EmployeeNo { get; private set; }
    public string EmployeeName { get; private set; }
    public string PeriodCode { get; private set; }
    public string ReviewerEmployeeNo { get; private set; }
    public AppraisalStatus Status { get; private set; }
    public decimal OverallScore { get; private set; }
    public string EmployeeComment { get; private set; }
    public string ReviewerComment { get; private set; }
    public DateTime? CompletedDate { get; private set; }

    public Collection<AppraisalLine> Lines { get; private set; } = new();

    protected Appraisal() { }

    public Appraisal(Guid id, string no, Employee employee, string periodCode)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        EmployeeId = employee.Id;
        EmployeeNo = employee.No;
        EmployeeName = employee.FullName;
        PeriodCode = CodeTableEntity.NormalizeCode(periodCode);
        Status = AppraisalStatus.Draft;
    }

    public void Set(string reviewerEmployeeNo, string employeeComment, string reviewerComment)
    {
        EnsureNotCompleted();
        ReviewerEmployeeNo = CodeTableEntity.NormalizeCode(Check.Length(reviewerEmployeeNo, nameof(reviewerEmployeeNo), ErpDomainConsts.MaxNoLength));
        EmployeeComment = Check.Length(employeeComment, nameof(employeeComment), 2000);
        ReviewerComment = Check.Length(reviewerComment, nameof(reviewerComment), 2000);
    }

    public AppraisalLine AddLine(Guid id, string objective, decimal weight, string target)
    {
        EnsureStatus(AppraisalStatus.Draft, AppraisalStatus.SelfReview);
        var line = new AppraisalLine(id, Id, (Lines.Count == 0 ? 0 : Lines.Max(l => l.LineNo)) + 10000);
        line.SetObjective(objective, weight, target);
        Lines.Add(line);
        return line;
    }

    /// <summary>The employee has set the objectives and now scores them.</summary>
    public void StartSelfReview()
    {
        EnsureStatus(AppraisalStatus.Draft);
        if (Lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.AppraisalHasNoLines).WithData("no", No);
        }

        Status = AppraisalStatus.SelfReview;
    }

    public void SubmitToReviewer()
    {
        EnsureStatus(AppraisalStatus.SelfReview);
        Status = AppraisalStatus.ManagerReview;
    }

    public void Complete(DateTime date)
    {
        EnsureStatus(AppraisalStatus.ManagerReview);
        var weight = Lines.Sum(l => l.Weight);
        OverallScore = weight == 0m ? 0m : Math.Round(Lines.Sum(l => l.Weight * l.ManagerScore) / weight, 2, MidpointRounding.AwayFromZero);
        CompletedDate = date.Date;
        Status = AppraisalStatus.Completed;
    }

    public void Reopen()
    {
        EnsureStatus(AppraisalStatus.SelfReview, AppraisalStatus.ManagerReview);
        Status = Status == AppraisalStatus.ManagerReview ? AppraisalStatus.SelfReview : AppraisalStatus.Draft;
    }

    public void EnsureStatus(params AppraisalStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.AppraisalStatusInvalid).WithData("no", No).WithData("status", Status.ToString());
        }
    }

    private void EnsureNotCompleted() => EnsureStatus(AppraisalStatus.Draft, AppraisalStatus.SelfReview, AppraisalStatus.ManagerReview);
}

public class AppraisalLine : Entity<Guid>
{
    public Guid AppraisalId { get; private set; }
    public int LineNo { get; private set; }
    public string Objective { get; private set; }

    /// <summary>How much the objective counts towards the overall score.</summary>
    public decimal Weight { get; private set; }

    public string Target { get; private set; }
    public decimal SelfScore { get; private set; }
    public decimal ManagerScore { get; private set; }
    public string Comment { get; private set; }

    protected AppraisalLine() { }

    public AppraisalLine(Guid id, Guid appraisalId, int lineNo)
        : base(id)
    {
        AppraisalId = appraisalId;
        LineNo = lineNo;
    }

    public void SetObjective(string objective, decimal weight, string target)
    {
        Objective = Check.NotNullOrWhiteSpace(objective, nameof(objective), ErpDomainConsts.MaxDescriptionLength).Trim();
        Weight = Math.Max(0m, weight);
        Target = Check.Length(target, nameof(target), ErpDomainConsts.MaxDescriptionLength);
    }

    public void SetScores(decimal selfScore, decimal managerScore, string comment)
    {
        if (selfScore < 0m || selfScore > Appraisal.MaxScore || managerScore < 0m || managerScore > Appraisal.MaxScore)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.AppraisalScoreInvalid).WithData("lineNo", LineNo);
        }

        SelfScore = selfScore;
        ManagerScore = managerScore;
        Comment = Check.Length(comment, nameof(comment), ErpDomainConsts.MaxDescriptionLength);
    }
}

/// <summary>Hires applicants: the employee card is made from the applicant and the vacancy.</summary>
public class RecruitmentManager : DomainService
{
    private readonly IRepository<JobApplicant, Guid> _applicants;
    private readonly IRepository<JobVacancy, Guid> _vacancies;
    private readonly IRepository<Employee, Guid> _employees;

    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public RecruitmentManager(IRepository<JobApplicant, Guid> applicants, IRepository<JobVacancy, Guid> vacancies, IRepository<Employee, Guid> employees)
    {
        _applicants = applicants;
        _vacancies = vacancies;
        _employees = employees;
    }

    public async Task<string> NextNoAsync(string prefix, string sequence) => $"{prefix}{await EntryNos.NextAsync(sequence):D5}";

    /// <summary>Makes the employee, numbered from the HR Setup's employee series (or typed), and marks the applicant hired.</summary>
    public async Task<Employee> HireAsync(Guid applicantId, DateTime employmentDate, string employeeNo)
    {
        var applicant = await _applicants.GetAsync(applicantId);
        var vacancyNo = applicant.VacancyNo;
        var vacancy = await _vacancies.FirstAsync(v => v.No == vacancyNo);
        vacancy.EnsureOpen();

        var setup = await LazyServiceProvider.LazyGetRequiredService<HumanResourcesSetupManager>().GetAsync();
        var no = !setup.EmployeeNos.IsNullOrWhiteSpace()
            ? await LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>().ResolveNoAsync(setup.EmployeeNos, employeeNo, employmentDate)
            : employeeNo.IsNullOrWhiteSpace() ? $"E-{applicant.No}" : employeeNo.Trim().ToUpperInvariant();
        if (await _employees.AnyAsync(e => e.No == no))
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.EmployeeAlreadyExists).WithData("no", no);
        }

        var employee = new Employee(GuidGenerator.Create(), no, applicant.FirstName, applicant.LastName ?? "");
        employee.SetJobTitle(vacancy.Title);
        employee.SetContact(applicant.PhoneNo, null, applicant.Email, null);
        employee.SetEmployment(employmentDate, null, null);
        await _employees.InsertAsync(employee, autoSave: true);

        applicant.Hire(employee.No);
        vacancy.RecordHire();
        await _applicants.UpdateAsync(applicant, autoSave: true);
        await _vacancies.UpdateAsync(vacancy, autoSave: true);
        return employee;
    }
}
