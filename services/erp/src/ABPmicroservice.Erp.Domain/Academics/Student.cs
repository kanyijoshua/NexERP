using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Academics;

/// <summary>
/// An application for admission to a programme. It is filled in (Open), submitted, and approved
/// or rejected; admitting an approved application turns the applicant into a student.
/// </summary>
public class StudentApplication : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public DateTime ApplicationDate { get; private set; }

    public string FirstName { get; private set; }
    public string OtherName { get; private set; }
    public string LastName { get; private set; }

    /// <summary>"First Other Last", kept so that lists can search and sort on one column.</summary>
    public string FullName { get; private set; }

    public StudentGender Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string NationalId { get; private set; }

    public string PhoneNo { get; private set; }
    public string Email { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }

    public string ProgrammeCode { get; private set; }
    public string IntakeCode { get; private set; }
    public string AcademicYearCode { get; private set; }
    public StudyMode StudyMode { get; private set; } = StudyMode.FullTime;

    public string FormerSchool { get; private set; }

    /// <summary>The applicant's index number in the examination they qualified with.</summary>
    public string IndexNumber { get; private set; }

    public string MeanGrade { get; private set; }

    public StudentSponsorship Sponsorship { get; private set; }
    public string GuardianName { get; private set; }
    public string GuardianPhoneNo { get; private set; }

    public ApplicationStatus Status { get; private set; }
    public string RejectionReason { get; private set; }

    /// <summary>The student the application was admitted as.</summary>
    public string StudentNo { get; private set; }

    public DateTime? AdmissionDate { get; private set; }

    protected StudentApplication() { }

    public StudentApplication(Guid id, string no, DateTime applicationDate, string firstName, string lastName, string programmeCode)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        ApplicationDate = applicationDate.Date;
        SetName(firstName, null, lastName);
        ProgrammeCode = Check.NotNullOrWhiteSpace(programmeCode, nameof(programmeCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
    }

    public bool IsOpen => Status == ApplicationStatus.Open;

    public void SetName(string firstName, string otherName, string lastName)
    {
        EnsureOpen();
        (FirstName, OtherName, LastName, FullName) = Student.NameOf(firstName, otherName, lastName);
    }

    public void SetPersonal(StudentGender gender, DateTime? dateOfBirth, string nationalId)
    {
        EnsureOpen();
        Gender = gender;
        DateOfBirth = dateOfBirth?.Date;
        NationalId = Check.Length(nationalId?.Trim(), nameof(nationalId), ErpDomainConsts.MaxCodeLength * 2);
    }

    public void SetContact(string phoneNo, string email, string address, string city)
    {
        EnsureOpen();
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
        Email = Check.Length(email, nameof(email), ErpDomainConsts.MaxEmailLength);
        Address = Check.Length(address, nameof(address), ErpDomainConsts.MaxAddressLength);
        City = Check.Length(city, nameof(city), ErpDomainConsts.MaxCityLength);
    }

    public void SetCourse(DateTime applicationDate, string programmeCode, string intakeCode, string academicYearCode, StudyMode studyMode)
    {
        EnsureOpen();
        ApplicationDate = applicationDate.Date;
        ProgrammeCode = Check.NotNullOrWhiteSpace(programmeCode, nameof(programmeCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        IntakeCode = CodeTableEntity.NormalizeCode(Check.Length(intakeCode, nameof(intakeCode), ErpDomainConsts.MaxCodeLength));
        AcademicYearCode = CodeTableEntity.NormalizeCode(Check.Length(academicYearCode, nameof(academicYearCode), ErpDomainConsts.MaxCodeLength));
        StudyMode = studyMode == StudyMode.None ? StudyMode.FullTime : studyMode;
    }

    public void SetBackground(
        string formerSchool,
        string indexNumber,
        string meanGrade,
        StudentSponsorship sponsorship,
        string guardianName,
        string guardianPhoneNo
    )
    {
        EnsureOpen();
        FormerSchool = Check.Length(formerSchool, nameof(formerSchool), ErpDomainConsts.MaxNameLength);
        IndexNumber = Check.Length(indexNumber?.Trim(), nameof(indexNumber), ErpDomainConsts.MaxExternalDocumentNoLength);
        MeanGrade = Check.Length(meanGrade?.Trim(), nameof(meanGrade), ErpDomainConsts.MaxCodeLength);
        Sponsorship = sponsorship;
        GuardianName = Check.Length(guardianName, nameof(guardianName), ErpDomainConsts.MaxNameLength);
        GuardianPhoneNo = Check.Length(guardianPhoneNo, nameof(guardianPhoneNo), ErpDomainConsts.MaxPhoneLength);
    }

    public void Submit()
    {
        EnsureOpen();
        Status = ApplicationStatus.Submitted;
    }

    public void Approve()
    {
        EnsureStatus(ApplicationStatus.Submitted);
        Status = ApplicationStatus.Approved;
        RejectionReason = null;
    }

    public void Reject(string reason)
    {
        EnsureStatus(ApplicationStatus.Submitted);
        Status = ApplicationStatus.Rejected;
        RejectionReason = Check.Length(reason, nameof(reason), ErpDomainConsts.MaxDescriptionLength);
    }

    /// <summary>Back to Open, to be corrected and submitted again. An admitted application stays as it is.</summary>
    public void Reopen()
    {
        if (Status is ApplicationStatus.Open or ApplicationStatus.Admitted)
        {
            throw new BusinessException(ErpErrorCodes.Academics.ApplicationStatusWrong).WithData("applicationNo", No).WithData("status", Status);
        }

        Status = ApplicationStatus.Open;
    }

    internal void MarkAdmitted(string studentNo, DateTime admissionDate)
    {
        EnsureStatus(ApplicationStatus.Approved);
        Status = ApplicationStatus.Admitted;
        StudentNo = studentNo;
        AdmissionDate = admissionDate.Date;
    }

    /// <summary>Only an open application may be changed.</summary>
    public void EnsureOpen() => EnsureStatus(ApplicationStatus.Open);

    private void EnsureStatus(ApplicationStatus expected)
    {
        if (Status != expected)
        {
            throw new BusinessException(ErpErrorCodes.Academics.ApplicationStatusWrong).WithData("applicationNo", No ?? string.Empty).WithData("status", Status);
        }
    }
}

/// <summary>
/// A student. What the student owes is not stored here: fees are charged to the student's
/// customer account, so the balance is the receivables ledger's and a payment is an ordinary receipt.
/// </summary>
public class Student : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }

    public string FirstName { get; private set; }
    public string OtherName { get; private set; }
    public string LastName { get; private set; }
    public string FullName { get; private set; }

    public StudentGender Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string NationalId { get; private set; }

    public string PhoneNo { get; private set; }
    public string Email { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }

    public string ProgrammeCode { get; private set; }

    /// <summary>The stage and semester of the student's latest registration.</summary>
    public string CurrentStageCode { get; internal set; }

    public string CurrentSemesterCode { get; internal set; }

    public string IntakeCode { get; private set; }

    /// <summary>The academic year the student was admitted in.</summary>
    public string AcademicYearCode { get; private set; }

    public StudyMode StudyMode { get; private set; } = StudyMode.FullTime;
    public StudentStatus Status { get; private set; }
    public DateTime? AdmissionDate { get; private set; }

    /// <summary>The receivable account the student's fees are charged to.</summary>
    public string CustomerNo { get; internal set; }

    public string ApplicationNo { get; internal set; }

    public StudentSponsorship Sponsorship { get; private set; }
    public string GuardianName { get; private set; }
    public string GuardianPhoneNo { get; private set; }

    protected Student() { }

    public Student(Guid id, string no, string firstName, string lastName, string programmeCode)
        : base(id)
    {
        SetNo(no);
        SetName(firstName, null, lastName);
        ProgrammeCode = Check.NotNullOrWhiteSpace(programmeCode, nameof(programmeCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
    }

    public void SetNo(string no) => No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();

    public void SetName(string firstName, string otherName, string lastName)
    {
        (FirstName, OtherName, LastName, FullName) = NameOf(firstName, otherName, lastName);
    }

    public void SetPersonal(StudentGender gender, DateTime? dateOfBirth, string nationalId)
    {
        Gender = gender;
        DateOfBirth = dateOfBirth?.Date;
        NationalId = Check.Length(nationalId?.Trim(), nameof(nationalId), ErpDomainConsts.MaxCodeLength * 2);
    }

    public void SetContact(string phoneNo, string email, string address, string city)
    {
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
        Email = Check.Length(email, nameof(email), ErpDomainConsts.MaxEmailLength);
        Address = Check.Length(address, nameof(address), ErpDomainConsts.MaxAddressLength);
        City = Check.Length(city, nameof(city), ErpDomainConsts.MaxCityLength);
    }

    public void SetCourse(string programmeCode, string intakeCode, string academicYearCode, StudyMode studyMode, DateTime? admissionDate)
    {
        ProgrammeCode = Check.NotNullOrWhiteSpace(programmeCode, nameof(programmeCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        IntakeCode = CodeTableEntity.NormalizeCode(Check.Length(intakeCode, nameof(intakeCode), ErpDomainConsts.MaxCodeLength));
        AcademicYearCode = CodeTableEntity.NormalizeCode(Check.Length(academicYearCode, nameof(academicYearCode), ErpDomainConsts.MaxCodeLength));
        StudyMode = studyMode == StudyMode.None ? StudyMode.FullTime : studyMode;
        AdmissionDate = admissionDate?.Date;
    }

    public void SetSponsor(StudentSponsorship sponsorship, string guardianName, string guardianPhoneNo)
    {
        Sponsorship = sponsorship;
        GuardianName = Check.Length(guardianName, nameof(guardianName), ErpDomainConsts.MaxNameLength);
        GuardianPhoneNo = Check.Length(guardianPhoneNo, nameof(guardianPhoneNo), ErpDomainConsts.MaxPhoneLength);
    }

    public void SetStatus(StudentStatus status) => Status = status;

    /// <summary>A student who may register for a semester and be billed.</summary>
    public bool IsActive => Status is StudentStatus.Registration or StudentStatus.Current;

    /// <summary>What a submitted registration does to the student: it is where the student now is.</summary>
    internal void Register(string stageCode, string semesterCode)
    {
        CurrentStageCode = stageCode;
        CurrentSemesterCode = semesterCode;
        if (Status == StudentStatus.Registration)
        {
            Status = StudentStatus.Current;
        }
    }

    internal static (string First, string Other, string Last, string Full) NameOf(string firstName, string otherName, string lastName)
    {
        var first = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), ErpDomainConsts.MaxNameLength / 2).Trim();
        var other = Check.Length(otherName?.Trim(), nameof(otherName), ErpDomainConsts.MaxNameLength / 2);
        var last = Check.Length(lastName?.Trim(), nameof(lastName), ErpDomainConsts.MaxNameLength / 2);
        var full = string.Join(" ", new[] { first, other, last }.Where(part => !part.IsNullOrWhiteSpace()));

        return (first, other, last, full);
    }
}

/// <summary>
/// Numbers students, opens their receivable accounts and admits applicants.
/// <para>
/// A student's number comes from the programme's own series when it has one, between the
/// programme's prefix and suffix, otherwise from the Academic Setup's. The student's customer
/// account takes the same number, which is what ties a fee statement to a student.
/// </para>
/// </summary>
public class StudentManager : DomainService
{
    private readonly IRepository<Student, Guid> _students;
    private readonly IRepository<StudentApplication, Guid> _applications;
    private readonly IRepository<Programme, Guid> _programmes;
    private readonly IRepository<Customer, Guid> _customers;
    private readonly AcademicSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;

    public StudentManager(
        IRepository<Student, Guid> students,
        IRepository<StudentApplication, Guid> applications,
        IRepository<Programme, Guid> programmes,
        IRepository<Customer, Guid> customers,
        AcademicSetupManager setupManager,
        NoSeriesManager noSeriesManager
    )
    {
        _students = students;
        _applications = applications;
        _programmes = programmes;
        _customers = customers;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
    }

    /// <summary>A programme that still takes applications and registrations.</summary>
    public async Task<Programme> GetActiveProgrammeAsync(string programmeCode)
    {
        var code = CodeTableEntity.NormalizeCode(programmeCode);
        var programme = await _programmes.FirstOrDefaultAsync(p => p.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Programme").WithData("code", code ?? string.Empty);

        return programme.Active
            ? programme
            : throw new BusinessException(ErpErrorCodes.Academics.ProgrammeNotActive).WithData("programme", programme.Code);
    }

    /// <summary>The number of a new student of a programme: the one typed, or the next of the series.</summary>
    public async Task<string> ResolveStudentNoAsync(Programme programme, string typedNo, DateTime date)
    {
        string no;
        if (!typedNo.IsNullOrWhiteSpace() || programme.StudentNos.IsNullOrWhiteSpace())
        {
            var setup = await _setupManager.GetAsync();
            no = (await _noSeriesManager.ResolveNoAsync(setup.StudentNos, typedNo, date)).ToUpperInvariant();
        }
        else
        {
            no = programme.FormatStudentNo(await _noSeriesManager.GetNextNoAsync(programme.StudentNos, date));
        }

        if (await _students.AnyAsync(s => s.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Student").WithData("key", no);
        }

        return no;
    }

    /// <summary>
    /// Makes sure the student has a customer account under the student's own number, opened with
    /// the posting groups of the Academic Setup, and keeps its name and contact details in step.
    /// </summary>
    public async Task<Customer> EnsureCustomerAsync(Student student)
    {
        var customerNo = student.CustomerNo ?? student.No;
        var customer = await _customers.FirstOrDefaultAsync(c => c.No == customerNo);

        if (customer == null)
        {
            var setup = await _setupManager.GetAsync();
            customer = new Customer(GuidGenerator.Create(), customerNo, student.FullName);
            customer.SetPostingGroups(setup.StudentPostingGroup, setup.StudentGenBusPostingGroup);
            customer.SetAddress(student.Address, student.City, null, null);
            customer.SetContact(student.PhoneNo, student.Email);
            await _customers.InsertAsync(customer, autoSave: true);
        }
        else if (customer.Name != student.FullName || customer.PhoneNo != student.PhoneNo || customer.Email != student.Email)
        {
            customer.SetName(student.FullName);
            customer.SetContact(student.PhoneNo, student.Email);
            await _customers.UpdateAsync(customer, autoSave: true);
        }

        student.CustomerNo = customer.No;
        return customer;
    }

    /// <summary>Admits an approved application: the applicant becomes a student with a customer account.</summary>
    public async Task<Student> AdmitAsync(StudentApplication application, DateTime admissionDate)
    {
        if (application.Status != ApplicationStatus.Approved)
        {
            throw new BusinessException(ErpErrorCodes.Academics.ApplicationStatusWrong).WithData("applicationNo", application.No).WithData("status", application.Status);
        }

        var programme = await GetActiveProgrammeAsync(application.ProgrammeCode);
        var no = await ResolveStudentNoAsync(programme, null, admissionDate);

        var student = new Student(GuidGenerator.Create(), no, application.FirstName, application.LastName, programme.Code);
        student.SetName(application.FirstName, application.OtherName, application.LastName);
        student.SetPersonal(application.Gender, application.DateOfBirth, application.NationalId);
        student.SetContact(application.PhoneNo, application.Email, application.Address, application.City);
        student.SetCourse(programme.Code, application.IntakeCode, application.AcademicYearCode, application.StudyMode, admissionDate);
        student.SetSponsor(application.Sponsorship, application.GuardianName, application.GuardianPhoneNo);
        student.ApplicationNo = application.No;

        await EnsureCustomerAsync(student);
        await _students.InsertAsync(student, autoSave: true);

        application.MarkAdmitted(student.No, admissionDate);
        await _applications.UpdateAsync(application, autoSave: true);

        return student;
    }
}
