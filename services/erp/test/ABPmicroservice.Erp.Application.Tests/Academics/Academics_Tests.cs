using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Academics;

public class Academics_Tests : ErpApplicationTestBase
{
    private static readonly DateTime Today = new(2026, 1, 28);

    private readonly IAcademicYearAppService _years;
    private readonly ISemesterAppService _semesters;
    private readonly IProgrammeAppService _programmes;
    private readonly IProgrammeStageAppService _stages;
    private readonly ICourseUnitAppService _courseUnits;
    private readonly IFeeStructureLineAppService _feeStructure;
    private readonly IStudentApplicationAppService _applications;
    private readonly IStudentAppService _students;
    private readonly ISemesterRegistrationAppService _registrations;
    private readonly IStudentUnitAppService _studentUnits;
    private readonly IStudentBillAppService _bills;
    private readonly IExamResultAppService _examResults;
    private readonly IExamResultLineAppService _examResultLines;

    public Academics_Tests()
    {
        _years = GetRequiredService<IAcademicYearAppService>();
        _semesters = GetRequiredService<ISemesterAppService>();
        _programmes = GetRequiredService<IProgrammeAppService>();
        _stages = GetRequiredService<IProgrammeStageAppService>();
        _courseUnits = GetRequiredService<ICourseUnitAppService>();
        _feeStructure = GetRequiredService<IFeeStructureLineAppService>();
        _applications = GetRequiredService<IStudentApplicationAppService>();
        _students = GetRequiredService<IStudentAppService>();
        _registrations = GetRequiredService<ISemesterRegistrationAppService>();
        _studentUnits = GetRequiredService<IStudentUnitAppService>();
        _bills = GetRequiredService<IStudentBillAppService>();
        _examResults = GetRequiredService<IExamResultAppService>();
        _examResultLines = GetRequiredService<IExamResultLineAppService>();
    }

    /// <summary>
    /// A programme with one stage of two units, examined under the seeded grading scheme (CAT out
    /// of 30 and a final exam out of 70), costing 30,000 tuition and 2,000 exam fee a semester.
    /// </summary>
    private async Task SetUpProgrammeAsync(string programme)
    {
        if ((await _years.GetListAsync(new())).TotalCount == 0)
        {
            await _years.CreateAsync(new CreateUpdateAcademicYearDto { Code = "2026", Description = "2026", Current = true });
            await _semesters.CreateAsync(new CreateUpdateSemesterDto { Code = "SEM1", Description = "Semester 1", AcademicYearCode = "2026", Current = true });
        }

        await _programmes.CreateAsync(new CreateUpdateProgrammeDto
        {
            Code = programme,
            Description = programme + " Diploma",
            Level = ProgrammeLevel.Diploma,
            ExamCategoryCode = "GENERAL",
        });
        await _stages.CreateAsync(new CreateUpdateProgrammeStageDto { ProgrammeCode = programme, Code = "Y1", Description = "Year 1", Sequence = 1 });

        await _courseUnits.CreateAsync(new CreateUpdateCourseUnitDto { ProgrammeCode = programme, Code = "U101", Description = "Introduction", StageCode = "Y1", CreditHours = 3m });
        await _courseUnits.CreateAsync(new CreateUpdateCourseUnitDto { ProgrammeCode = programme, Code = "U102", Description = "Mathematics", StageCode = "Y1", CreditHours = 3m });

        await _feeStructure.CreateAsync(new CreateUpdateFeeStructureLineDto { ProgrammeCode = programme, StageCode = "Y1", FeeItemCode = "TUITION", Amount = 30000m });
        await _feeStructure.CreateAsync(new CreateUpdateFeeStructureLineDto { ProgrammeCode = programme, StageCode = "Y1", FeeItemCode = "EXAM", Amount = 2000m });
    }

    private Task<StudentDto> NewStudentAsync(string programme, string firstName, string lastName)
    {
        return _students.CreateAsync(new CreateUpdateStudentDto { ProgrammeCode = programme, FirstName = firstName, LastName = lastName, AdmissionDate = Today });
    }

    private async Task<SemesterRegistrationDto> RegisterAsync(StudentDto student)
    {
        var registration = await _registrations.CreateAsync(new CreateUpdateSemesterRegistrationDto { StudentNo = student.No, StageCode = "Y1", RegistrationDate = Today });
        await _registrations.FillUnitsAsync(registration.Id);
        return await _registrations.SubmitAsync(registration.Id);
    }

    private async Task<ExamResultHeaderDto> PostMarksAsync(string programme, string unit, ExamType examType, params (string StudentNo, decimal Mark)[] marks)
    {
        var document = await _examResults.CreateAsync(new CreateUpdateExamResultHeaderDto { ProgrammeCode = programme, UnitCode = unit, ExamType = examType, DocumentDate = Today });
        document = await _examResults.SuggestLinesAsync(document.Id);

        var lines = await _examResultLines.GetListAsync(new GetDocumentLineListInput { DocumentNo = document.No });
        foreach (var (studentNo, mark) in marks)
        {
            var line = lines.Items.Single(l => l.StudentNo == studentNo);
            await _examResultLines.UpdateAsync(line.Id, new CreateUpdateExamResultLineDto { DocumentNo = document.No, StudentNo = studentNo, Mark = mark });
        }

        return await _examResults.RunPostingAsync(document.Id);
    }

    [Fact]
    public async Task Admitting_An_Application_Makes_A_Student_With_A_Customer_Account()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpProgrammeAsync("DIT");

            var application = await _applications.CreateAsync(new CreateUpdateStudentApplicationDto
            {
                ProgrammeCode = "dit",
                FirstName = "Achieng",
                LastName = "Otieno",
                ApplicationDate = Today,
                PhoneNo = "0700000001",
            });
            application.No.ShouldBe("APP-00001");
            application.ProgrammeCode.ShouldBe("DIT");
            application.AcademicYearCode.ShouldBe("2026");

            // Only an approved application can be admitted.
            var early = await Should.ThrowAsync<BusinessException>(() => _applications.AdmitAsync(application.Id, new AdmitApplicationInput()));
            early.Code.ShouldBe(ErpErrorCodes.Academics.ApplicationStatusWrong);

            await _applications.SubmitAsync(application.Id);
            await _applications.ApproveAsync(application.Id);
            application = await _applications.AdmitAsync(application.Id, new AdmitApplicationInput { AdmissionDate = Today });

            application.Status.ShouldBe(ApplicationStatus.Admitted);
            application.StudentNo.ShouldBe("S000010");

            var student = (await _students.GetListAsync(new GetStudentListInput { Filter = "S000010" })).Items.Single();
            student.FullName.ShouldBe("Achieng Otieno");
            student.Status.ShouldBe(StudentStatus.Registration);
            student.ApplicationNo.ShouldBe(application.No);
            student.CustomerNo.ShouldBe("S000010");

            var customer = await GetRequiredService<IRepository<Customer, Guid>>().GetAsync(c => c.No == "S000010");
            customer.Name.ShouldBe("Achieng Otieno");
            customer.CustomerPostingGroup.ShouldBe("DOMESTIC");

            // An admitted application is a record: it can be neither changed nor deleted.
            var locked = await Should.ThrowAsync<BusinessException>(() => _applications.DeleteAsync(application.Id));
            locked.Code.ShouldBe(ErpErrorCodes.Academics.ApplicationStatusWrong);
        });
    }

    [Fact]
    public async Task Submitting_A_Registration_Bills_The_Semester_To_The_Students_Account()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpProgrammeAsync("DBM");
            var student = await NewStudentAsync("DBM", "Kamau", "Njoroge");

            var registration = await _registrations.CreateAsync(new CreateUpdateSemesterRegistrationDto { StudentNo = student.No, StageCode = "Y1", RegistrationDate = Today });
            registration.SemesterCode.ShouldBe("SEM1");
            registration.AcademicYearCode.ShouldBe("2026");

            var empty = await Should.ThrowAsync<BusinessException>(() => _registrations.SubmitAsync(registration.Id));
            empty.Code.ShouldBe(ErpErrorCodes.Academics.NoUnitsSelected);

            registration = await _registrations.FillUnitsAsync(registration.Id);
            registration.NoOfUnits.ShouldBe(2);

            registration = await _registrations.SubmitAsync(registration.Id);
            registration.Status.ShouldBe(RegistrationStatus.Submitted);
            registration.BilledAmount.ShouldBe(32000m);

            var bill = (await _bills.GetListAsync(new GetStudentBillListInput { StudentNo = student.No })).Items.Single();
            bill.No.ShouldBe(registration.BillNo);
            bill.Status.ShouldBe(AcademicDocumentStatus.Posted);
            bill.RegistrationNo.ShouldBe(registration.No);

            // Fee income is credited by fee item and the student's account debited with the total.
            var glEntries = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.DocumentNo == bill.No);
            glEntries.Sum(e => (double)e.Amount).ShouldBe(0d);
            glEntries.Single(e => e.GLAccountNo == "4210").Amount.ShouldBe(-30000m);
            glEntries.Single(e => e.GLAccountNo == "4220").Amount.ShouldBe(-2000m);
            glEntries.Single(e => e.GLAccountNo == "1200").Amount.ShouldBe(32000m);

            var balance = await _students.GetBalanceAsync(student.Id);
            balance.Balance.ShouldBe(32000m);
            balance.TotalBilled.ShouldBe(32000m);

            student = await _students.GetAsync(student.Id);
            student.Status.ShouldBe(StudentStatus.Current);
            student.CurrentStageCode.ShouldBe("Y1");
            student.CurrentSemesterCode.ShouldBe("SEM1");

            // A submitted registration is closed, and the same units cannot be taken twice in the semester.
            var closed = await Should.ThrowAsync<BusinessException>(() => _registrations.DeleteAsync(registration.Id));
            closed.Code.ShouldBe(ErpErrorCodes.Academics.DocumentNotOpen);

            var again = await _registrations.CreateAsync(new CreateUpdateSemesterRegistrationDto { StudentNo = student.No, StageCode = "Y1", RegistrationDate = Today });
            await _registrations.FillUnitsAsync(again.Id);
            var twice = await Should.ThrowAsync<BusinessException>(() => _registrations.SubmitAsync(again.Id));
            twice.Code.ShouldBe(ErpErrorCodes.Academics.UnitAlreadyRegistered);
        });
    }

    [Fact]
    public async Task Posted_Marks_Are_Weighted_And_Graded_Once_Every_Part_Is_In()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpProgrammeAsync("DCS");
            var first = await NewStudentAsync("DCS", "Wanjiru", "Kariuki");
            var second = await NewStudentAsync("DCS", "Omondi", "Okoth");
            await RegisterAsync(first);
            await RegisterAsync(second);

            // A mark above what the part is out of is refused, and nothing is assigned.
            var tooHigh = await Should.ThrowAsync<BusinessException>(() => PostMarksAsync("DCS", "U101", ExamType.Cat, (first.No, 31m), (second.No, 10m)));
            tooHigh.Code.ShouldBe(ErpErrorCodes.Academics.MarkAboveMaximum);

            var open = (await _examResults.GetListAsync(new GetExamResultListInput { ProgrammeCode = "DCS" })).Items.Single();
            await _examResults.DeleteAsync(open.Id);

            var cat = await PostMarksAsync("DCS", "U101", ExamType.Cat, (first.No, 24m), (second.No, 10m));
            cat.Status.ShouldBe(AcademicDocumentStatus.Posted);
            cat.NoOfStudents.ShouldBe(2);

            // With the final exam still to come the unit has a score so far but no grade.
            var unit = (await _studentUnits.GetListAsync(new GetStudentUnitListInput { StudentNo = first.No, UnitCode = "U101" })).Items.Single();
            unit.CatMark.ShouldBe(24m);
            unit.FinalScore.ShouldBe(24m);
            unit.Grade.ShouldBeNull();

            await PostMarksAsync("DCS", "U101", ExamType.FinalExam, (first.No, 49m), (second.No, 20m));

            unit = (await _studentUnits.GetListAsync(new GetStudentUnitListInput { StudentNo = first.No, UnitCode = "U101" })).Items.Single();
            unit.FinalScore.ShouldBe(73m);
            unit.Grade.ShouldBe("A");
            unit.Passed.ShouldBeTrue();

            var failed = (await _studentUnits.GetListAsync(new GetStudentUnitListInput { StudentNo = second.No, UnitCode = "U101" })).Items.Single();
            failed.FinalScore.ShouldBe(30m);
            failed.Grade.ShouldBe("E");
            failed.Passed.ShouldBeFalse();

            // The marks are in: a second CAT document finds nobody left to mark.
            var repeat = await _examResults.CreateAsync(new CreateUpdateExamResultHeaderDto { ProgrammeCode = "DCS", UnitCode = "U101", ExamType = ExamType.Cat, DocumentDate = Today });
            (await _examResults.SuggestLinesAsync(repeat.Id)).NoOfStudents.ShouldBe(0);

            var transcript = await _students.GetTranscriptAsync(first.Id);
            transcript.UnitsTaken.ShouldBe(1);
            transcript.UnitsPassed.ShouldBe(1);
            transcript.MeanScore.ShouldBe(73m);
            transcript.MeanPoints.ShouldBe(4m);
        });
    }
}
