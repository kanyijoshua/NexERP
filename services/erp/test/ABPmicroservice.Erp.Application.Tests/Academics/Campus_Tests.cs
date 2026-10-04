using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Academics;

public class Campus_Tests : ErpApplicationTestBase
{
    private static readonly DateTime Today = new(2026, 1, 28);

    private readonly IAcademicYearAppService _years;
    private readonly ISemesterAppService _semesters;
    private readonly IProgrammeAppService _programmes;
    private readonly IProgrammeStageAppService _stages;
    private readonly ICourseUnitAppService _courseUnits;
    private readonly IStudentAppService _students;
    private readonly ISemesterRegistrationAppService _registrations;

    public Campus_Tests()
    {
        _years = GetRequiredService<IAcademicYearAppService>();
        _semesters = GetRequiredService<ISemesterAppService>();
        _programmes = GetRequiredService<IProgrammeAppService>();
        _stages = GetRequiredService<IProgrammeStageAppService>();
        _courseUnits = GetRequiredService<ICourseUnitAppService>();
        _students = GetRequiredService<IStudentAppService>();
        _registrations = GetRequiredService<ISemesterRegistrationAppService>();
    }

    /// <summary>A current semester and a one-stage programme of two units with no fees.</summary>
    private async Task SetUpProgrammeAsync(string programme)
    {
        if ((await _years.GetListAsync(new Companies.GetCodeTableListInput { MaxResultCount = 10 })).TotalCount == 0)
        {
            await _years.CreateAsync(new CreateUpdateAcademicYearDto { Code = "2026", Description = "2026", Current = true });
            await _semesters.CreateAsync(new CreateUpdateSemesterDto { Code = "SEM1", Description = "Semester 1", AcademicYearCode = "2026", Current = true });
        }

        await _programmes.CreateAsync(new CreateUpdateProgrammeDto { Code = programme, Description = programme, ExamCategoryCode = "GENERAL" });
        await _stages.CreateAsync(new CreateUpdateProgrammeStageDto { ProgrammeCode = programme, Code = "Y1", Sequence = 1, FinalStage = true });
        await _courseUnits.CreateAsync(new CreateUpdateCourseUnitDto { ProgrammeCode = programme, Code = "U101", Description = "Introduction", StageCode = "Y1" });
        await _courseUnits.CreateAsync(new CreateUpdateCourseUnitDto { ProgrammeCode = programme, Code = "U102", Description = "Methods", StageCode = "Y1" });
    }

    private async Task<StudentDto> AdmitAndRegisterAsync(string programme, string firstName, StudentGender gender)
    {
        var student = await _students.CreateAsync(new CreateUpdateStudentDto
        {
            ProgrammeCode = programme,
            FirstName = firstName,
            LastName = "Student",
            Gender = gender,
            AdmissionDate = Today,
        });

        var registration = await _registrations.CreateAsync(new CreateUpdateSemesterRegistrationDto { StudentNo = student.No, StageCode = "Y1", RegistrationDate = Today });
        await _registrations.FillUnitsAsync(registration.Id);
        await _registrations.SubmitAsync(registration.Id);
        return student;
    }

    [Fact]
    public async Task A_Timetable_Refuses_Clashes_And_Rooms_Too_Small_For_The_Class()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpProgrammeAsync("TTA");
            await SetUpProgrammeAsync("TTB");
            await AdmitAndRegisterAsync("TTA", "Amina", StudentGender.Female);
            await AdmitAndRegisterAsync("TTA", "Baraka", StudentGender.Male);

            var rooms = GetRequiredService<ILectureRoomAppService>();
            await rooms.CreateAsync(new CreateUpdateLectureRoomDto { Code = "HALL", Description = "Main hall", MaximumCapacity = 100 });
            await rooms.CreateAsync(new CreateUpdateLectureRoomDto { Code = "TUTOR", Description = "Tutorial room", MaximumCapacity = 1 });

            var timetable = GetRequiredService<ITimetableEntryAppService>();
            var entry = await timetable.CreateAsync(new CreateUpdateTimetableEntryDto
            {
                ProgrammeCode = "TTA",
                UnitCode = "U101",
                Day = TimetableDay.Monday,
                StartTime = "8:00",
                EndTime = "10:00",
                RoomCode = "HALL",
            });
            entry.SemesterCode.ShouldBe("SEM1");
            entry.StartTime.ShouldBe("08:00");

            // The same class cannot be in two lessons at once.
            var classClash = await Should.ThrowAsync<BusinessException>(() => timetable.CreateAsync(new CreateUpdateTimetableEntryDto
            {
                ProgrammeCode = "TTA", UnitCode = "U102", Day = TimetableDay.Monday, StartTime = "09:00", EndTime = "11:00", RoomCode = "TUTOR",
            }));
            classClash.Code.ShouldBe(ErpErrorCodes.Academics.TimetableClash);

            // Another class cannot use the room while it is taken; back to back is fine.
            var roomClash = await Should.ThrowAsync<BusinessException>(() => timetable.CreateAsync(new CreateUpdateTimetableEntryDto
            {
                ProgrammeCode = "TTB", UnitCode = "U101", Day = TimetableDay.Monday, StartTime = "09:30", EndTime = "10:30", RoomCode = "HALL",
            }));
            roomClash.Code.ShouldBe(ErpErrorCodes.Academics.TimetableClash);

            await timetable.CreateAsync(new CreateUpdateTimetableEntryDto
            {
                ProgrammeCode = "TTB", UnitCode = "U101", Day = TimetableDay.Monday, StartTime = "10:00", EndTime = "12:00", RoomCode = "HALL",
            });

            // Two students do not fit a room for one.
            var tooSmall = await Should.ThrowAsync<BusinessException>(() => timetable.CreateAsync(new CreateUpdateTimetableEntryDto
            {
                ProgrammeCode = "TTA", UnitCode = "U102", Day = TimetableDay.Tuesday, StartTime = "08:00", EndTime = "10:00", RoomCode = "TUTOR",
            }));
            tooSmall.Code.ShouldBe(ErpErrorCodes.Academics.ClassExceedsRoom);

            var noDate = await Should.ThrowAsync<BusinessException>(() => timetable.CreateAsync(new CreateUpdateTimetableEntryDto
            {
                ProgrammeCode = "TTA", UnitCode = "U102", TimetableType = TimetableType.Exam, StartTime = "08:00", EndTime = "10:00",
            }));
            noDate.Code.ShouldBe(ErpErrorCodes.Academics.ExamDateRequired);

            var exam = await timetable.CreateAsync(new CreateUpdateTimetableEntryDto
            {
                ProgrammeCode = "TTA", UnitCode = "U102", TimetableType = TimetableType.Exam, ExamDate = new DateTime(2026, 4, 15), StartTime = "08:00", EndTime = "10:00",
            });
            exam.Day.ShouldBe(TimetableDay.Wednesday);

            // Every student of U101 also sits U102, so their exams cannot be at the same time.
            var studentClash = await Should.ThrowAsync<BusinessException>(() => timetable.CreateAsync(new CreateUpdateTimetableEntryDto
            {
                ProgrammeCode = "TTA", UnitCode = "U101", TimetableType = TimetableType.Exam, ExamDate = new DateTime(2026, 4, 15), StartTime = "09:00", EndTime = "11:00",
            }));
            studentClash.Code.ShouldBe(ErpErrorCodes.Academics.TimetableClash);
        });
    }

    [Fact]
    public async Task Attendance_Builds_Up_On_The_Unit_And_Too_Little_Bars_The_Final_Exam()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpProgrammeAsync("ATT");
            var student = await AdmitAndRegisterAsync("ATT", "Chebet", StudentGender.Female);

            var registers = GetRequiredService<IAttendanceRegisterAppService>();
            var lines = GetRequiredService<IAttendanceLineAppService>();

            foreach (var (day, mark) in new[] { (5, AttendanceMark.Absent), (12, AttendanceMark.Present), (19, AttendanceMark.Excused) })
            {
                var register = await registers.CreateAsync(new CreateUpdateAttendanceRegisterDto { ProgrammeCode = "ATT", UnitCode = "U101", LessonDate = new DateTime(2026, 1, day), Hours = 2 });
                register = await registers.SuggestLinesAsync(register.Id);
                register.NoOfStudents.ShouldBe(1);

                var line = (await lines.GetListAsync(new GetDocumentLineListInput { DocumentNo = register.No })).Items.Single();
                await lines.UpdateAsync(line.Id, new CreateUpdateAttendanceLineDto { DocumentNo = register.No, StudentNo = student.No, Mark = mark });
                register = await registers.RunPostingAsync(register.Id);
                register.Status.ShouldBe(AcademicDocumentStatus.Posted);
            }

            // One of two sessions attended; the excused one is not counted.
            var unit = (await GetRequiredService<IRepository<StudentUnit, Guid>>().GetListAsync(u => u.StudentNo == student.No && u.UnitCode == "U101")).Single();
            unit.SessionsHeld.ShouldBe(2);
            unit.SessionsAttended.ShouldBe(1);
            unit.AttendancePct.ShouldBe(50m);

            var setupService = GetRequiredService<IAcademicSetupAppService>();
            var setup = await setupService.GetAsync();
            setup.MinAttendancePct = 75m;
            await setupService.UpdateAsync(setup);

            var examResults = GetRequiredService<IExamResultAppService>();
            var examLines = GetRequiredService<IExamResultLineAppService>();

            // A CAT is not held back by attendance; the final exam is.
            foreach (var (type, mark) in new[] { (ExamType.Cat, 20m), (ExamType.FinalExam, 50m) })
            {
                var document = await examResults.CreateAsync(new CreateUpdateExamResultHeaderDto { ProgrammeCode = "ATT", UnitCode = "U101", ExamType = type, DocumentDate = Today });
                await examResults.SuggestLinesAsync(document.Id);
                var line = (await examLines.GetListAsync(new GetDocumentLineListInput { DocumentNo = document.No })).Items.Single();
                await examLines.UpdateAsync(line.Id, new CreateUpdateExamResultLineDto { DocumentNo = document.No, StudentNo = student.No, Mark = mark });

                if (type == ExamType.Cat)
                {
                    await examResults.RunPostingAsync(document.Id);
                    continue;
                }

                var barred = await Should.ThrowAsync<BusinessException>(() => examResults.RunPostingAsync(document.Id));
                barred.Code.ShouldBe(ErpErrorCodes.Academics.NotEligibleForExam);
            }
        });
    }

    [Fact]
    public async Task A_Hostel_Place_Is_Billed_Fills_A_Bed_And_Is_Freed_On_Clearance()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpProgrammeAsync("HOS");
            var first = await AdmitAndRegisterAsync("HOS", "Dorcas", StudentGender.Female);
            var second = await AdmitAndRegisterAsync("HOS", "Esther", StudentGender.Female);
            var third = await AdmitAndRegisterAsync("HOS", "Felix", StudentGender.Male);

            await GetRequiredService<IHostelAppService>().CreateAsync(new CreateUpdateHostelDto
            {
                Code = "MARY", Description = "Mary Hall", Gender = StudentGender.Female, CostPerOccupant = 9000m, FeeItemCode = "HOSTEL",
            });
            var rooms = GetRequiredService<IHostelRoomAppService>();
            var room = await rooms.CreateAsync(new CreateUpdateHostelRoomDto { HostelCode = "MARY", RoomNo = "101", BedSpaces = 1 });

            var allocations = GetRequiredService<IHostelAllocationAppService>();
            var allocation = await allocations.CreateAsync(new CreateUpdateHostelAllocationDto { StudentNo = first.No, HostelCode = "MARY", RoomNo = "101", AllocationDate = Today });
            allocation.No.ShouldBe("HA-00001");
            allocation.Status.ShouldBe(HostelAllocationStatus.Booking);

            allocation = await allocations.AllocateAsync(allocation.Id);
            allocation.Status.ShouldBe(HostelAllocationStatus.Allocated);
            allocation.Charges.ShouldBe(9000m);
            allocation.BillNo.ShouldNotBeNull();
            (await _students.GetBalanceAsync(first.Id)).Balance.ShouldBe(9000m);
            (await rooms.GetAsync(room.Id)).VacantSpaces.ShouldBe(0);

            var full = await allocations.CreateAsync(new CreateUpdateHostelAllocationDto { StudentNo = second.No, HostelCode = "MARY", RoomNo = "101", AllocationDate = Today });
            (await Should.ThrowAsync<BusinessException>(() => allocations.AllocateAsync(full.Id))).Code.ShouldBe(ErpErrorCodes.Academics.RoomFull);

            var wrongHall = await allocations.CreateAsync(new CreateUpdateHostelAllocationDto { StudentNo = third.No, HostelCode = "MARY", RoomNo = "101", AllocationDate = Today });
            (await Should.ThrowAsync<BusinessException>(() => allocations.AllocateAsync(wrongHall.Id))).Code.ShouldBe(ErpErrorCodes.Academics.HostelGenderMismatch);

            // Clearing the first student frees the bed for the second.
            allocation = await allocations.ClearAsync(allocation.Id);
            allocation.Status.ShouldBe(HostelAllocationStatus.Cleared);
            (await allocations.AllocateAsync(full.Id)).Status.ShouldBe(HostelAllocationStatus.Allocated);
        });
    }

    [Fact]
    public async Task An_Infirmary_Visit_Issues_Drugs_From_Stock_And_Bills_The_Student()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await SetUpProgrammeAsync("CLN");
            var student = await AdmitAndRegisterAsync("CLN", "Grace", StudentGender.Female);

            var visits = GetRequiredService<IClinicVisitAppService>();
            var visit = await visits.CreateAsync(new CreateUpdateClinicVisitDto
            {
                PatientType = PatientType.Student,
                PatientNo = student.No,
                VisitDate = Today,
                Complaint = "Fever",
                Diagnosis = "Malaria",
                Treatment = "Antimalarials",
                OffDutyFrom = Today,
                OffDutyTo = Today.AddDays(2),
                Charge = 500m,
            });
            visit.No.ShouldBe("CV-00001");
            visit.PatientName.ShouldBe("Grace Student");
            visit.OffDutyDays.ShouldBe(3);

            await GetRequiredService<IClinicPrescriptionAppService>().CreateAsync(new CreateUpdateClinicPrescriptionDto
            {
                DocumentNo = visit.No, ItemNo = "1000", Quantity = 2, Dosage = "1 x 3 for 3 days",
            });

            var noStock = await Should.ThrowAsync<BusinessException>(() => visits.CompleteAsync(visit.Id));
            noStock.Code.ShouldBe(ErpErrorCodes.Academics.ItemNotInStock);

            var items = GetRequiredService<IRepository<Item, Guid>>();
            await WithUnitOfWorkAsync(async () =>
            {
                var item = await items.SingleAsync(i => i.No == "1000");
                await GetRequiredService<ItemJnlPostLine>().PostItemEntryAsync(item.Id, item.No, Today, ItemLedgerEntryType.PositiveAdjmt, "OPEN-1", "Opening stock", 10, item.UnitCost);
            });

            visit = await visits.CompleteAsync(visit.Id);
            visit.Status.ShouldBe(ClinicVisitStatus.Completed);
            visit.BillNo.ShouldNotBeNull();

            (await items.SingleAsync(i => i.No == "1000")).Inventory.ShouldBe(8m);
            (await _students.GetBalanceAsync(student.Id)).Balance.ShouldBe(500m);
        });
    }

    [Fact]
    public async Task A_Laundry_Order_Is_Priced_With_The_Express_Charge_And_Discount_And_Invoiced()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var orders = GetRequiredService<ILaundryOrderAppService>();
            var lines = GetRequiredService<ILaundryOrderLineAppService>();

            var order = await orders.CreateAsync(new CreateUpdateLaundryOrderDto { CustomerNo = "C00010", ReceivedDate = Today, Express = true, DiscountPct = 10m });
            order.No.ShouldBe("LDY-00001");

            await lines.CreateAsync(new CreateUpdateLaundryOrderLineDto { DocumentNo = order.No, LaundryItemCode = "SHIRT", Quantity = 2 });
            await lines.CreateAsync(new CreateUpdateLaundryOrderLineDto { DocumentNo = order.No, LaundryItemCode = "BLANKET", Quantity = 1 });

            // (2 x 50 + 200) x 1.5 express, less 10%.
            order = await orders.GetAsync(order.Id);
            order.TotalAmount.ShouldBe(405m);

            order = await orders.InvoiceAsync(order.Id);
            order.Status.ShouldBe(LaundryStatus.Invoiced);

            var entries = await GetRequiredService<IRepository<GLEntry, Guid>>().GetListAsync(e => e.DocumentNo == order.No);
            entries.Sum(e => (double)e.Amount).ShouldBe(0d);
            entries.Where(e => e.GLAccountNo == "4230").Sum(e => e.Amount).ShouldBe(-405m);

            (await Should.ThrowAsync<BusinessException>(() => orders.InvoiceAsync(order.Id))).Code.ShouldBe(ErpErrorCodes.Academics.DocumentStatusWrong);

            await orders.MarkReadyAsync(order.Id);
            (await orders.MarkCollectedAsync(order.Id)).Status.ShouldBe(LaundryStatus.Collected);
        });
    }

    [Fact]
    public async Task A_Corporate_Short_Course_Application_Bills_The_Sponsor_For_Each_Participant()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await GetRequiredService<IShortCourseAppService>().CreateAsync(new CreateUpdateShortCourseDto
            {
                Code = "FIRST-AID", Description = "First aid at work", DurationDays = 3, FeePerParticipant = 8000m, GLAccountNo = "4230",
            });

            var applications = GetRequiredService<IShortCourseApplicationAppService>();
            var participants = GetRequiredService<IShortCourseParticipantAppService>();

            var application = await applications.CreateAsync(new CreateUpdateShortCourseApplicationDto
            {
                ApplicationType = ShortCourseApplicationType.Corporate,
                CourseCode = "FIRST-AID",
                ApplicationDate = Today,
                StartDate = new DateTime(2026, 2, 2),
                CustomerNo = "C00010",
            });
            application.No.ShouldBe("SCA-00001");
            application.EndDate.ShouldBe(new DateTime(2026, 2, 4));

            await participants.CreateAsync(new CreateUpdateShortCourseParticipantDto { DocumentNo = application.No, Name = "Hassan Ali" });
            await participants.CreateAsync(new CreateUpdateShortCourseParticipantDto { DocumentNo = application.No, Name = "Irene Wairimu" });

            await applications.SubmitAsync(application.Id);
            // A rejection needs a reason.
            await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(() => applications.RejectAsync(application.Id, new RejectShortCourseApplicationInput()));

            await applications.ApproveAsync(application.Id);
            application = await applications.RegisterAsync(application.Id);
            application.Status.ShouldBe(ShortCourseApplicationStatus.Registered);
            application.BilledAmount.ShouldBe(16000m);

            var enrolled = (await participants.GetListAsync(new GetDocumentLineListInput { DocumentNo = application.No })).Items;
            enrolled.Select(p => p.CertificateNo).ShouldBe(["SCA-00001-001", "SCA-00001-002"]);

            // An individual application is one person.
            var individual = await applications.CreateAsync(new CreateUpdateShortCourseApplicationDto { CourseCode = "FIRST-AID", ApplicationDate = Today });
            await participants.CreateAsync(new CreateUpdateShortCourseParticipantDto { DocumentNo = individual.No, Name = "Joy" });
            await participants.CreateAsync(new CreateUpdateShortCourseParticipantDto { DocumentNo = individual.No, Name = "Kevin" });
            (await Should.ThrowAsync<BusinessException>(() => applications.SubmitAsync(individual.Id))).Code.ShouldBe(ErpErrorCodes.Academics.ParticipantCountWrong);
        });
    }
}
