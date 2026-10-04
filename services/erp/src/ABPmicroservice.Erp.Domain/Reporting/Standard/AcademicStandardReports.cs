using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Academics;
using ABPmicroservice.Erp.Sales;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>What the academic reports share. Their "No. Filter" is on the student number.</summary>
public abstract class AcademicReportBase : StandardReportBase
{
    protected IRepository<Student, Guid> Students => LazyServiceProvider.LazyGetRequiredService<IRepository<Student, Guid>>();

    protected static StandardReportDefinition Define(int id, string code, string name, StandardReportParameters parameters) =>
        new(id, code, name, StandardReportAreas.Academics, parameters | StandardReportParameters.NoFilter);

    protected async Task<List<Student>> GetStudentsAsync(StandardReportRequest request)
    {
        var filter = Filter(request);

        return (await Students.GetListAsync())
            .Where(s => filter.Matches(s.No))
            .OrderBy(s => s.ProgrammeCode, StringComparer.Ordinal)
            .ThenBy(s => s.No, StringComparer.Ordinal)
            .ToList();
    }
}

/// <summary>Student List: the students of each programme, with where they are in it and how to reach them.</summary>
public class StudentListReport : AcademicReportBase
{
    public override StandardReportDefinition Definition { get; } = Define(51521001, "StudentList", "Student List", StandardReportParameters.None);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "Student No.");
        Text(result, "name", "Name");
        Text(result, "stage", "Stage");
        Text(result, "semester", "Semester");
        Text(result, "intake", "Intake");
        Text(result, "studyMode", "Mode of Study");
        Text(result, "status", "Status");
        Text(result, "phoneNo", "Phone No.");
        Text(result, "email", "Email");

        foreach (var programme in (await GetStudentsAsync(request)).GroupBy(s => s.ProgrammeCode, StringComparer.Ordinal))
        {
            BoldRow(result, ("no", programme.Key), ("name", $"{programme.Count()} students"));

            foreach (var student in programme)
            {
                Row(
                    result,
                    ("no", student.No),
                    ("name", student.FullName),
                    ("stage", student.CurrentStageCode),
                    ("semester", student.CurrentSemesterCode),
                    ("intake", student.IntakeCode),
                    ("studyMode", ErpEntityFieldNames.Humanize(student.StudyMode)),
                    ("status", ErpEntityFieldNames.Humanize(student.Status)),
                    ("phoneNo", student.PhoneNo),
                    ("email", student.Email)
                ).Indentation = 1;
            }
        }

        return result;
    }
}

/// <summary>
/// Student Fee Balances: what each student was charged and has paid up to a date, from the
/// student's customer account, and what is still owed.
/// </summary>
public class StudentFeeBalancesReport : AcademicReportBase
{
    private readonly IRepository<CustomerLedgerEntry, Guid> _entries;

    public StudentFeeBalancesReport(IRepository<CustomerLedgerEntry, Guid> entries)
    {
        _entries = entries;
    }

    public override StandardReportDefinition Definition { get; } =
        Define(51521002, "StudentFeeBalances", "Student Fee Balances", StandardReportParameters.AsOfDate);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "Student No.");
        Text(result, "name", "Name");
        Text(result, "programme", "Programme");
        Text(result, "stage", "Stage");
        Number(result, "charged", "Charged");
        Number(result, "paid", "Paid");
        Number(result, "balance", "Balance");

        var students = (await GetStudentsAsync(request)).Where(s => s.CustomerNo != null).ToList();
        var customers = students.Select(s => s.CustomerNo).ToList();
        var to = request.To;
        var entries = (await _entries.GetListAsync(e => e.PostingDate <= to && customers.Contains(e.CustomerNo))).ToLookup(e => e.CustomerNo, StringComparer.Ordinal);

        decimal totalCharged = 0m,
            totalPaid = 0m;

        foreach (var student in students)
        {
            var charged = entries[student.CustomerNo].Where(e => e.AmountLcy > 0m).Sum(e => e.AmountLcy);
            var paid = -entries[student.CustomerNo].Where(e => e.AmountLcy < 0m).Sum(e => e.AmountLcy);
            if (charged == 0m && paid == 0m)
            {
                continue;
            }

            totalCharged += charged;
            totalPaid += paid;

            Row(
                result,
                ("no", student.No),
                ("name", student.FullName),
                ("programme", student.ProgrammeCode),
                ("stage", student.CurrentStageCode),
                ("charged", charged),
                ("paid", paid),
                ("balance", charged - paid)
            );
        }

        BoldRow(result, ("name", "Total"), ("charged", totalCharged), ("paid", totalPaid), ("balance", totalCharged - totalPaid));
        return result;
    }
}

/// <summary>Student Transcript: every unit a student took, with its marks, final score and grade, and the mean score.</summary>
public class StudentTranscriptReport : AcademicReportBase
{
    private readonly IRepository<StudentUnit, Guid> _studentUnits;
    private readonly IRepository<SemesterRegistration, Guid> _registrations;

    public StudentTranscriptReport(IRepository<StudentUnit, Guid> studentUnits, IRepository<SemesterRegistration, Guid> registrations)
    {
        _studentUnits = studentUnits;
        _registrations = registrations;
    }

    public override StandardReportDefinition Definition { get; } = Define(51521003, "StudentTranscript", "Student Transcript", StandardReportParameters.None);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "academicYear", "Academic Year");
        Text(result, "semester", "Semester");
        Text(result, "unit", "Unit");
        Text(result, "description", "Description");
        Number(result, "cat", "CAT");
        Number(result, "exam", "Exam");
        Number(result, "finalScore", "Final Score");
        Text(result, "grade", "Grade");
        Text(result, "remarks", "Remarks");

        var submitted = (await _registrations.GetListAsync(r => r.Status == RegistrationStatus.Submitted)).Select(r => r.No).ToHashSet(StringComparer.Ordinal);
        var units = (await _studentUnits.GetListAsync()).Where(u => submitted.Contains(u.RegistrationNo)).ToLookup(u => u.StudentNo, StringComparer.Ordinal);

        foreach (var student in await GetStudentsAsync(request))
        {
            var taken = units[student.No]
                .OrderBy(u => u.AcademicYearCode, StringComparer.Ordinal)
                .ThenBy(u => u.SemesterCode, StringComparer.Ordinal)
                .ThenBy(u => u.UnitCode, StringComparer.Ordinal)
                .ToList();

            if (taken.Count == 0)
            {
                continue;
            }

            BoldRow(result, ("unit", student.No), ("description", $"{student.FullName} ({student.ProgrammeCode})"));

            foreach (var unit in taken)
            {
                Row(
                    result,
                    ("academicYear", unit.AcademicYearCode),
                    ("semester", unit.SemesterCode),
                    ("unit", unit.UnitCode),
                    ("description", unit.UnitDescription),
                    ("cat", (unit.AssignmentMark ?? 0m) + (unit.CatMark ?? 0m) + (unit.Cat2Mark ?? 0m)),
                    ("exam", unit.ExamMark),
                    ("finalScore", unit.FinalScore),
                    ("grade", unit.Grade ?? "Incomplete"),
                    ("remarks", unit.ResultRemarks)
                ).Indentation = 1;
            }

            var graded = taken.Where(u => u.Grade != null).ToList();
            if (graded.Count > 0)
            {
                BoldRow(
                    result,
                    ("description", $"Mean score ({graded.Count(u => u.Passed)} of {graded.Count} units passed)"),
                    ("finalScore", Math.Round(graded.Average(u => u.FinalScore), 2, MidpointRounding.AwayFromZero))
                ).Indentation = 1;
            }
        }

        return result;
    }
}
