using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Academics;

/// <summary>A short course: how long it runs, what it costs each participant and where the fee is credited.</summary>
public class ShortCourse : CodeTableEntity
{
    public int DurationDays { get; private set; }
    public decimal FeePerParticipant { get; private set; }
    public string GLAccountNo { get; private set; }

    /// <summary>The most participants one application may enrol; 0 is no limit.</summary>
    public int MaxParticipants { get; private set; }

    /// <summary>A course that is not active takes no applications.</summary>
    public bool Active { get; private set; } = true;

    protected ShortCourse() { }

    public ShortCourse(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(int durationDays, decimal feePerParticipant, string glAccountNo, int maxParticipants, bool active)
    {
        if (durationDays < 0 || feePerParticipant < 0 || maxParticipants < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", feePerParticipant < 0 ? "Fee Per Participant" : "Duration Days");
        }

        DurationDays = durationDays;
        FeePerParticipant = feePerParticipant;
        GLAccountNo = glAccountNo.IsNullOrWhiteSpace() ? null : Check.Length(glAccountNo.Trim(), nameof(glAccountNo), ErpDomainConsts.MaxNoLength);
        MaxParticipants = maxParticipants;
        Active = active;
    }
}

/// <summary>
/// An application for a short course: one person applying for themselves, or a sponsor (an
/// existing customer) sending several participants. It is submitted, approved or rejected, and
/// registered, which bills the fee and gives each participant a certificate number.
/// </summary>
public class ShortCourseApplication : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public ShortCourseApplicationType ApplicationType { get; private set; }
    public string CourseCode { get; private set; }
    public string CourseDescription { get; private set; }
    public DateTime ApplicationDate { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    /// <summary>The sponsor billed for a corporate application; on an individual one, the account opened for the applicant.</summary>
    public string CustomerNo { get; private set; }

    public string CustomerName { get; private set; }

    /// <summary>Zero takes the course's fee.</summary>
    public decimal FeePerParticipant { get; private set; }

    public string Remarks { get; private set; }
    public string RejectionReason { get; private set; }

    public ShortCourseApplicationStatus Status { get; private set; }
    public int NoOfParticipants { get; internal set; }
    public decimal BilledAmount { get; private set; }
    public DateTime? RegisteredDate { get; private set; }
    public string ProcessedBy { get; private set; }

    protected ShortCourseApplication() { }

    public ShortCourseApplication(Guid id, string no, ShortCourseApplicationType applicationType, ShortCourse course, DateTime applicationDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        Set(applicationType, course, applicationDate, applicationDate, null, null, 0m, null);
    }

    public bool IsOpen => Status == ShortCourseApplicationStatus.Open;

    /// <param name="endDate">Blank runs the course for its duration from the start date.</param>
    public void Set(
        ShortCourseApplicationType applicationType,
        ShortCourse course,
        DateTime applicationDate,
        DateTime startDate,
        DateTime? endDate,
        Customer sponsor,
        decimal feePerParticipant,
        string remarks
    )
    {
        EnsureStatus(ShortCourseApplicationStatus.Open);
        if (feePerParticipant < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Fee Per Participant");
        }

        var end = endDate?.Date ?? startDate.Date.AddDays(Math.Max(0, course.DurationDays - 1));
        if (end < startDate.Date)
        {
            throw new BusinessException(ErpErrorCodes.Academics.PeriodReversed).WithData("code", No);
        }

        ApplicationType = applicationType;
        CourseCode = course.Code;
        CourseDescription = course.Description;
        ApplicationDate = applicationDate.Date;
        StartDate = startDate.Date;
        EndDate = end;
        CustomerNo = sponsor?.No;
        CustomerName = sponsor?.Name;
        FeePerParticipant = feePerParticipant;
        Remarks = Check.Length(remarks, nameof(remarks), ErpDomainConsts.MaxDescriptionLength);
    }

    public void Submit()
    {
        EnsureStatus(ShortCourseApplicationStatus.Open);
        Status = ShortCourseApplicationStatus.Submitted;
    }

    public void Approve()
    {
        EnsureStatus(ShortCourseApplicationStatus.Submitted);
        Status = ShortCourseApplicationStatus.Approved;
    }

    public void Reject(string reason)
    {
        if (Status is not (ShortCourseApplicationStatus.Submitted or ShortCourseApplicationStatus.Approved))
        {
            throw StatusWrong();
        }

        if (reason.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Academics.ReasonRequired).WithData("documentNo", No);
        }

        RejectionReason = Check.Length(reason.Trim(), nameof(reason), ErpDomainConsts.MaxDescriptionLength);
        Status = ShortCourseApplicationStatus.Rejected;
    }

    /// <summary>Back to Open, so a submitted or rejected application can be corrected.</summary>
    public void Reopen()
    {
        if (Status is not (ShortCourseApplicationStatus.Submitted or ShortCourseApplicationStatus.Approved or ShortCourseApplicationStatus.Rejected))
        {
            throw StatusWrong();
        }

        Status = ShortCourseApplicationStatus.Open;
        RejectionReason = null;
    }

    internal void MarkRegistered(string customerNo, string customerName, decimal billedAmount, DateTime when, string by)
    {
        CustomerNo = customerNo;
        CustomerName = customerName;
        BilledAmount = billedAmount;
        RegisteredDate = when;
        ProcessedBy = by;
        Status = ShortCourseApplicationStatus.Registered;
    }

    public void EnsureStatus(ShortCourseApplicationStatus status)
    {
        if (Status != status)
        {
            throw StatusWrong();
        }
    }

    private BusinessException StatusWrong() =>
        new BusinessException(ErpErrorCodes.Academics.DocumentStatusWrong).WithData("documentNo", No ?? string.Empty).WithData("status", Status);
}

/// <summary>One person enrolled on a short course application.</summary>
public class ShortCourseParticipant : CompanyEntity
{
    public string DocumentNo { get; private set; }
    public int LineNo { get; private set; }
    public string Name { get; private set; }
    public string NationalId { get; private set; }
    public string PhoneNo { get; private set; }
    public string Email { get; private set; }

    /// <summary>Given on registration.</summary>
    public string CertificateNo { get; internal set; }

    protected ShortCourseParticipant() { }

    public ShortCourseParticipant(Guid id, string documentNo, int lineNo, string name)
        : base(id)
    {
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        LineNo = lineNo;
        Set(name, null, null, null);
    }

    public void Set(string name, string nationalId, string phoneNo, string email)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength * 2).Trim();
        NationalId = Check.Length(nationalId?.Trim(), nameof(nationalId), ErpDomainConsts.MaxCodeLength * 2);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
        Email = Check.Length(email, nameof(email), ErpDomainConsts.MaxEmailLength);
    }
}

/// <summary>Submits and registers short course applications.</summary>
public class ShortCourseManager : DomainService
{
    public const string SourceCode = "SHORTCRS";

    private readonly IRepository<ShortCourseApplication, Guid> _applications;
    private readonly IRepository<ShortCourseParticipant, Guid> _participants;
    private readonly IRepository<ShortCourse, Guid> _courses;
    private readonly IRepository<Customer, Guid> _customers;
    private readonly AcademicSetupManager _setupManager;
    private readonly CustomerChargePoster _poster;
    private readonly ICurrentUser _currentUser;

    public ShortCourseManager(
        IRepository<ShortCourseApplication, Guid> applications,
        IRepository<ShortCourseParticipant, Guid> participants,
        IRepository<ShortCourse, Guid> courses,
        IRepository<Customer, Guid> customers,
        AcademicSetupManager setupManager,
        CustomerChargePoster poster,
        ICurrentUser currentUser
    )
    {
        _applications = applications;
        _participants = participants;
        _courses = courses;
        _customers = customers;
        _setupManager = setupManager;
        _poster = poster;
        _currentUser = currentUser;
    }

    public async Task UpdateTotalsAsync(ShortCourseApplication application)
    {
        application.NoOfParticipants = await _participants.CountAsync(p => p.DocumentNo == application.No);
        await _applications.UpdateAsync(application, autoSave: true);
    }

    /// <summary>
    /// Submits the application once it is complete: an individual application has exactly one
    /// participant, a corporate one at least one and a sponsor, and no more than the course takes.
    /// </summary>
    public async Task SubmitAsync(ShortCourseApplication application)
    {
        var course = await GetActiveCourseAsync(application.CourseCode);
        var count = await _participants.CountAsync(p => p.DocumentNo == application.No);

        var valid = application.ApplicationType == ShortCourseApplicationType.Individual
            ? count == 1
            : count >= 1 && !application.CustomerNo.IsNullOrWhiteSpace();

        if (!valid || (course.MaxParticipants > 0 && count > course.MaxParticipants))
        {
            throw new BusinessException(ErpErrorCodes.Academics.ParticipantCountWrong).WithData("documentNo", application.No);
        }

        application.Submit();
        application.NoOfParticipants = count;
        await _applications.UpdateAsync(application, autoSave: true);
    }

    /// <summary>
    /// Registers an approved application: an individual applicant is given a customer account under
    /// the application's number, the fee for every participant is invoiced to the account, and each
    /// participant gets a certificate number.
    /// </summary>
    public async Task RegisterAsync(ShortCourseApplication application, DateTime postingDate)
    {
        application.EnsureStatus(ShortCourseApplicationStatus.Approved);

        var course = await GetActiveCourseAsync(application.CourseCode);
        var participants = (await _participants.GetListAsync(p => p.DocumentNo == application.No)).OrderBy(p => p.LineNo).ToList();
        if (participants.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.ParticipantCountWrong).WithData("documentNo", application.No);
        }

        var customer = application.ApplicationType == ShortCourseApplicationType.Corporate
            ? await _customers.FirstOrDefaultAsync(c => c.No == application.CustomerNo)
                ?? throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", application.CustomerNo ?? string.Empty)
            : await EnsureApplicantCustomerAsync(application, participants[0]);

        var fee = application.FeePerParticipant > 0m ? application.FeePerParticipant : course.FeePerParticipant;
        var billed = 0m;
        if (fee > 0m)
        {
            billed = await _poster.PostAsync(
                SourceCode,
                application.No,
                postingDate,
                customer.No,
                $"{course.Description} {application.StartDate:dd MMM yyyy}".Trim(),
                participants.Select(p => new CustomerChargeLine(course.GLAccountNo, $"{course.Code} {p.Name}", fee)).ToList()
            );
        }

        var number = 0;
        foreach (var participant in participants)
        {
            participant.CertificateNo = $"{application.No}-{++number:000}";
            await _participants.UpdateAsync(participant, autoSave: true);
        }

        application.NoOfParticipants = participants.Count;
        application.MarkRegistered(customer.No, customer.Name, billed, Clock.Now, _currentUser.UserName);
        await _applications.UpdateAsync(application, autoSave: true);
    }

    public async Task<ShortCourse> GetActiveCourseAsync(string courseCode)
    {
        var course = await _courses.FirstOrDefaultAsync(c => c.Code == courseCode)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Short Course").WithData("code", courseCode ?? string.Empty);

        return course.Active ? course : throw new BusinessException(ErpErrorCodes.Academics.ProgrammeNotActive).WithData("programme", course.Code);
    }

    private async Task<Customer> EnsureApplicantCustomerAsync(ShortCourseApplication application, ShortCourseParticipant applicant)
    {
        var customer = await _customers.FirstOrDefaultAsync(c => c.No == application.No);
        if (customer != null)
        {
            return customer;
        }

        var setup = await _setupManager.GetAsync();
        customer = new Customer(GuidGenerator.Create(), application.No, applicant.Name);
        customer.SetPostingGroups(setup.StudentPostingGroup, setup.StudentGenBusPostingGroup);
        customer.SetContact(applicant.PhoneNo, applicant.Email);
        return await _customers.InsertAsync(customer, autoSave: true);
    }
}
