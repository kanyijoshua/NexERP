using System.Threading.Tasks;
using ABPmicroservice.Erp.Academics;
using ABPmicroservice.Erp.Finance;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// What the academic module needs before a first programme can be set up: number series, the
/// posting groups a student's account is opened with, fee income accounts with the usual fee
/// items, and a grading scheme to start from.
/// </summary>
public partial class ErpDataSeederContributor
{
    public const string DefaultExamCategoryCode = "GENERAL";

    private async Task SeedAcademicsAsync()
    {
        await EnsureSeriesAsync("ACA-APP", "Student Applications", "APP-00001");
        await EnsureSeriesAsync("ACA-STUD", "Students", "S000010");
        await EnsureSeriesAsync("ACA-REG", "Semester Registrations", "REG-00001");
        await EnsureSeriesAsync("ACA-BILL", "Student Bills", "SB-00001");
        await EnsureSeriesAsync("ACA-EXAM", "Exam Results", "EX-00001");
        await EnsureSeriesAsync("ACA-RCPT", "Student Receipts", "SR-00001");
        await EnsureSeriesAsync("ACA-RFND", "Student Refunds", "RF-00001");
        await EnsureSeriesAsync("ACA-CHG", "Student Status Changes", "SC-00001");
        await EnsureSeriesAsync("ACA-ATT", "Class Attendance", "ATT-00001");
        await EnsureSeriesAsync("ACA-HOS", "Hostel Allocations", "HA-00001");
        await EnsureSeriesAsync("ACA-CLN", "Infirmary Visits", "CV-00001");
        await EnsureSeriesAsync("ACA-LDY", "Laundry Orders", "LDY-00001");
        await EnsureSeriesAsync("ACA-SC", "Short Course Applications", "SCA-00001");

        await EnsureAccountAsync("4210", "Tuition Fees", GLAccountCategory.Income, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("4220", "Examination Fees", GLAccountCategory.Income, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("4230", "Other Student Fees", GLAccountCategory.Income, IncomeBalanceType.IncomeStatement);

        if (await IsEmptyAsync<AcademicSetup>())
        {
            var setup = new AcademicSetup(NewId());
            setup.SetNumbering("ACA-APP", "ACA-STUD", "ACA-REG", "ACA-BILL", "ACA-EXAM");
            setup.SetStudentDocumentNumbering("ACA-RCPT", "ACA-RFND", "ACA-CHG");
            setup.SetPostingGroups("DOMESTIC", "DOMESTIC");
            setup.SetOptions(checkStudentBalance: false, maxFeeBalanceToRegister: 0m, billOnRegistration: true, examRoundingDecimals: 0);
            setup.SetCampusNumbering("ACA-ATT", "ACA-HOS", "ACA-CLN", "ACA-LDY", "ACA-SC");
            setup.SetCampusOptions(minAttendancePct: 0m, laundryExpressChargePct: 50m, medicalFeeItemCode: "MEDICAL");
            await Repo<AcademicSetup>().InsertAsync(setup);
        }

        if (await IsEmptyAsync<FeeItem>())
        {
            (string Code, string Description, string Account, bool Tuition)[] items =
            [
                ("TUITION", "Tuition Fee", "4210", true),
                ("EXAM", "Examination Fee", "4220", false),
                ("REGISTRATION", "Registration Fee", "4230", false),
                ("LIBRARY", "Library Fee", "4230", false),
                ("STUDENT-ID", "Student ID Fee", "4230", false),
                ("HOSTEL", "Accommodation Fee", "4230", false),
                ("MEDICAL", "Medical Charges", "4230", false),
            ];

            foreach (var (code, description, account, tuition) in items)
            {
                var item = new FeeItem(NewId(), code, description);
                item.Set(FeeType.NormalCharge, account, 0m, tuition);
                await Repo<FeeItem>().InsertAsync(item);
            }
        }

        if (await IsEmptyAsync<LaundryItem>())
        {
            (string Code, string Description, decimal Rate)[] items =
            [
                ("SHIRT", "Shirt / blouse", 50m),
                ("TROUSER", "Trousers / skirt", 60m),
                ("BEDSHEET", "Bed sheet", 80m),
                ("BLANKET", "Blanket", 200m),
            ];

            foreach (var (code, description, rate) in items)
            {
                var item = new LaundryItem(NewId(), code, description);
                item.Set(rate, "4230");
                await Repo<LaundryItem>().InsertAsync(item);
            }
        }

        if (await IsEmptyAsync<ExamCategory>())
        {
            await Repo<ExamCategory>().InsertAsync(new ExamCategory(NewId(), DefaultExamCategoryCode, "General grading"));

            (ExamType Type, string Description, decimal Max, decimal Pct)[] components =
            [
                (ExamType.Cat, "Continuous assessment test", 30m, 30m),
                (ExamType.FinalExam, "Final examination", 70m, 70m),
            ];

            foreach (var (type, description, max, pct) in components)
            {
                var component = new ExamComponent(NewId(), DefaultExamCategoryCode, type);
                component.Set(description, max, pct);
                await Repo<ExamComponent>().InsertAsync(component);
            }

            (string Grade, decimal From, decimal To, decimal Points, string Remarks, bool Passed)[] bands =
            [
                ("A", 70m, 100m, 4m, "Distinction", true),
                ("B", 60m, 69.99m, 3m, "Credit", true),
                ("C", 50m, 59.99m, 2m, "Pass", true),
                ("D", 40m, 49.99m, 1m, "Pass", true),
                ("E", 0m, 39.99m, 0m, "Fail", false),
            ];

            foreach (var (grade, from, to, points, remarks, passed) in bands)
            {
                var band = new GradingBand(NewId(), DefaultExamCategoryCode, grade);
                band.Set(remarks, from, to, points, remarks, passed);
                await Repo<GradingBand>().InsertAsync(band);
            }
        }
    }
}
