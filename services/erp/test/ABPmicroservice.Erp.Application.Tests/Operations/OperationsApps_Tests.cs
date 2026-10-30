using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Crm;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Fleet;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.MaintenanceManagement;
using ABPmicroservice.Erp.PointOfSale;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Quality;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Subscriptions;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Operations;

/// <summary>
/// Recruitment and appraisals, customer subscriptions, point of sale, maintenance, quality checks,
/// fleet and marketing segments with mass mailing.
/// </summary>
public class OperationsApps_Tests : ErpApplicationTestBase
{
    private static readonly DateTime PostingDate = new(2026, 3, 1);

    [Fact]
    public async Task An_Applicant_Is_Hired_As_An_Employee_And_Appraised()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var vacancies = GetRequiredService<IJobVacancyAppService>();
            var vacancy = await vacancies.CreateAsync(new CreateUpdateJobVacancyDto { Title = "Accountant", Positions = 1 });
            await vacancies.OpenAsync(vacancy.Id);

            var applicants = GetRequiredService<IJobApplicantAppService>();
            var applicant = await applicants.CreateAsync(new CreateUpdateJobApplicantDto { VacancyNo = vacancy.No, FirstName = "Amina", LastName = "Otieno", Email = "amina@example.com" });
            await applicants.ChangeStageAsync(applicant.Id, new ChangeApplicantStageInput { Stage = ApplicantStage.Interview });
            (await Should.ThrowAsync<BusinessException>(() => applicants.ChangeStageAsync(applicant.Id, new ChangeApplicantStageInput { Stage = ApplicantStage.Hired })))
                .Code.ShouldBe(ErpErrorCodes.HumanResources.ApplicantStageInvalid);

            var hired = await applicants.HireAsync(applicant.Id, new HireApplicantInput { EmploymentDate = PostingDate, EmployeeNo = "E-HIRE-1" });
            hired.Stage.ShouldBe(ApplicantStage.Hired);
            hired.EmployeeNo.ShouldBe("E-HIRE-1");
            (await vacancies.GetAsync(vacancy.Id)).Status.ShouldBe(JobVacancyStatus.Closed);
            var employee = await GetRequiredService<IRepository<Employee, Guid>>().FirstAsync(e => e.No == "E-HIRE-1");
            employee.JobTitle.ShouldBe("Accountant");

            await GetRequiredService<IAppraisalPeriodAppService>().CreateAsync(new CreateUpdateAppraisalPeriodDto { Code = "2026", Description = "2026", StartingDate = new DateTime(2026, 1, 1), EndingDate = new DateTime(2026, 12, 31) });
            var appraisals = GetRequiredService<IAppraisalAppService>();
            var appraisal = await appraisals.CreateAsync(new CreateUpdateAppraisalDto { EmployeeNo = "E-HIRE-1", PeriodCode = "2026" });
            var lines = GetRequiredService<IAppraisalLineAppService>();
            var closeBooks = await lines.CreateAsync(new CreateUpdateAppraisalLineDto { AppraisalId = appraisal.Id, Objective = "Close the books on time", Weight = 2 });
            var reports = await lines.CreateAsync(new CreateUpdateAppraisalLineDto { AppraisalId = appraisal.Id, Objective = "Monthly reports", Weight = 1 });

            await appraisals.StartSelfReviewAsync(appraisal.Id);
            await appraisals.SubmitToReviewerAsync(appraisal.Id);
            await lines.UpdateAsync(closeBooks.Id, new CreateUpdateAppraisalLineDto { AppraisalId = appraisal.Id, Objective = closeBooks.Objective, Weight = 2, ManagerScore = 4 });
            await lines.UpdateAsync(reports.Id, new CreateUpdateAppraisalLineDto { AppraisalId = appraisal.Id, Objective = reports.Objective, Weight = 1, ManagerScore = 1 });
            var completed = await appraisals.CompleteAsync(appraisal.Id);
            completed.Status.ShouldBe(AppraisalStatus.Completed);
            completed.OverallScore.ShouldBe(3m); // (2 x 4 + 1 x 1) / 3
        });
    }

    [Fact]
    public async Task A_Subscription_Is_Invoiced_Every_Period_That_Has_Started()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await GetRequiredService<ISubscriptionPlanAppService>().CreateAsync(new CreateUpdateSubscriptionPlanDto
            {
                Code = "SUPPORT",
                Description = "Support plan",
                BillingPeriodMonths = 1,
                Price = 100m,
                LineType = SubscriptionLineType.GLAccount,
                LineNo = "4300",
            });

            var subscriptions = GetRequiredService<ICustomerSubscriptionAppService>();
            var subscription = await subscriptions.CreateAsync(new CreateUpdateCustomerSubscriptionDto { CustomerNo = "C00010", PlanCode = "SUPPORT", StartDate = new DateTime(2026, 1, 1) });
            await subscriptions.ActivateAsync(subscription.Id);

            var result = await subscriptions.InvoiceAllDueAsync(new InvoiceSubscriptionsInput { AsOfDate = new DateTime(2026, 3, 15), Post = true });
            result.InvoiceCount.ShouldBe(3);

            var invoiced = await subscriptions.GetAsync(subscription.Id);
            invoiced.NextBillingDate.ShouldBe(new DateTime(2026, 4, 1));
            var entries = await GetRequiredService<IRepository<CustomerLedgerEntry, Guid>>().GetListAsync(e => e.CustomerNo == "C00010" && e.Description != null && e.PostingDate >= new DateTime(2026, 1, 1) && e.PostingDate <= new DateTime(2026, 3, 1));
            entries.Count(e => e.Amount > 0m).ShouldBeGreaterThanOrEqualTo(3);

            // Nothing new is due until April.
            (await subscriptions.InvoiceAllDueAsync(new InvoiceSubscriptionsInput { AsOfDate = new DateTime(2026, 3, 31) })).InvoiceCount.ShouldBe(0);
        });
    }

    [Fact]
    public async Task A_Till_Sale_Posts_The_Invoice_And_The_Cash_And_The_Session_Counts_Out()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await GetRequiredService<IPosTerminalAppService>().CreateAsync(new CreatePosTerminalInput().Build());
            var sessions = GetRequiredService<IPosSessionAppService>();
            var session = await sessions.OpenAsync(new OpenPosSessionInput { TerminalCode = "TILL1", OpeningFloat = 100m });
            (await Should.ThrowAsync<BusinessException>(() => sessions.OpenAsync(new OpenPosSessionInput { TerminalCode = "TILL1" })))
                .Code.ShouldBe(ErpErrorCodes.PointOfSale.SessionAlreadyOpen);

            var orders = GetRequiredService<IPosOrderAppService>();
            var order = await orders.StartAsync(new StartPosOrderInput { SessionId = session.Id });
            order = await orders.RingUpAsync(order.Id, new RingUpInput { ItemNo = "1000", Quantity = 2, UnitPrice = 50m });
            order.Subtotal.ShouldBe(100m);

            var paid = await orders.PayAsync(order.Id, new PayPosOrderInput { Method = PosPaymentMethod.Cash, AmountTendered = 500m });
            paid.Status.ShouldBe(PosOrderStatus.Paid);
            paid.Change.ShouldBe(500m - paid.TotalPaid);
            var invoiceEntry = (await GetRequiredService<IRepository<CustomerLedgerEntry, Guid>>().GetListAsync(e => e.DocumentNo == paid.PostedInvoiceNo)).ShouldHaveSingleItem();
            invoiceEntry.Open.ShouldBeFalse();

            var closed = await sessions.CloseAsync(session.Id, new ClosePosSessionInput { CountedCash = 100m + paid.TotalPaid });
            closed.Status.ShouldBe(PosSessionStatus.Closed);
            closed.Difference.ShouldBe(0m);
        });
    }

    [Fact]
    public async Task Preventive_Maintenance_Comes_Round_And_Completing_It_Moves_The_Next_Date()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var equipment = await GetRequiredService<IMaintenanceEquipmentAppService>().CreateAsync(new CreateUpdateMaintenanceEquipmentDto
            {
                Description = "Forklift",
                PreventiveIntervalDays = 30,
                NextPreventiveDate = PostingDate,
            });

            var requests = GetRequiredService<IMaintenanceRequestAppService>();
            (await requests.GeneratePreventiveAsync(new GeneratePreventiveInput { AsOfDate = PostingDate })).RequestsCreated.ShouldBe(1);
            (await requests.GeneratePreventiveAsync(new GeneratePreventiveInput { AsOfDate = PostingDate })).RequestsCreated.ShouldBe(0);

            var request = (await requests.GetListAsync(new GetMaintenanceListInput { EquipmentNo = equipment.No, MaxResultCount = 10 })).Items.ShouldHaveSingleItem();
            request.Type.ShouldBe(MaintenanceType.Preventive);
            await requests.StartAsync(request.Id);
            var done = await requests.CompleteAsync(request.Id);
            done.Stage.ShouldBe(MaintenanceStage.Repaired);

            var maintained = await GetRequiredService<IMaintenanceEquipmentAppService>().GetAsync(equipment.Id);
            maintained.NextPreventiveDate.ShouldBe(maintained.LastMaintenanceDate!.Value.AddDays(30));
        });
    }

    [Fact]
    public async Task A_Receipt_Raises_The_Checks_Its_Control_Points_Ask_For()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await GetRequiredService<IQualityControlPointAppService>().CreateAsync(new CreateUpdateQualityControlPointDto
            {
                Code = "WEIGHT",
                Description = "Frame weight",
                Operation = QualityOperation.Receipt,
                ItemNo = "1000",
                TestType = QualityTestType.Measure,
                MinValue = 1m,
                MaxValue = 2m,
            });

            var vendor = await GetRequiredService<IVendorAppService>().GetByNoAsync("V00010");
            var purchases = GetRequiredService<IPurchaseDocumentAppService>();
            var order = await purchases.CreateAsync(new CreateUpdatePurchaseHeaderDto
            {
                DocumentType = PurchaseDocumentType.Order,
                No = "PO-QC-1",
                VendorId = vendor.Id,
                PostingDate = PostingDate,
                VendorInvoiceNo = "VI-QC-1",
                Lines = [new PurchaseLineInputDto { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle", Quantity = 3, DirectUnitCost = 100m, LocationCode = "MAIN" }],
            });
            var received = await purchases.RunOrderPostingAsync(order.Id, new PurchaseOrderPostingInput { Receive = true, Invoice = false });

            var checks = GetRequiredService<IQualityCheckAppService>();
            var check = (await checks.GetListAsync(new GetQualityCheckListInput { SourceDocumentNo = received.ReceiptNo, MaxResultCount = 10 })).Items.ShouldHaveSingleItem();
            check.Result.ShouldBe(QualityCheckResult.Pending);
            check.Quantity.ShouldBe(3m);

            (await Should.ThrowAsync<BusinessException>(() => checks.RecordAsync(check.Id, new RecordQualityCheckInput()))).Code.ShouldBe(ErpErrorCodes.Quality.MeasureRequired);
            (await checks.RecordAsync(check.Id, new RecordQualityCheckInput { MeasuredValue = 2.4m })).Result.ShouldBe(QualityCheckResult.Fail);
        });
    }

    [Fact]
    public async Task A_Vehicle_Costs_What_Its_Logs_Say_Per_Kilometre()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var vehicle = await GetRequiredService<IVehicleAppService>().CreateAsync(new CreateUpdateVehicleDto { LicensePlate = "KDA 123A", Make = "Toyota", Model = "Hilux" });
            var logs = GetRequiredService<IVehicleLogEntryAppService>();
            await logs.CreateAsync(new CreateVehicleLogEntryDto { VehicleNo = vehicle.No, Date = PostingDate, Type = VehicleLogType.Odometer, Odometer = 1000m });
            await logs.CreateAsync(new CreateVehicleLogEntryDto { VehicleNo = vehicle.No, Date = PostingDate.AddDays(5), Type = VehicleLogType.Fuel, Odometer = 1100m, Liters = 40m, Cost = 50m });
            await logs.CreateAsync(new CreateVehicleLogEntryDto { VehicleNo = vehicle.No, Date = PostingDate.AddDays(9), Type = VehicleLogType.Service, Odometer = 1200m, Cost = 70m });

            (await Should.ThrowAsync<BusinessException>(() => logs.CreateAsync(new CreateVehicleLogEntryDto { VehicleNo = vehicle.No, Type = VehicleLogType.Odometer, Odometer = 900m })))
                .Code.ShouldBe(ErpErrorCodes.Fleet.OdometerBackwards);

            var cost = (await GetRequiredService<IVehicleAppService>().GetCostsAsync(new GetVehicleCostsInput { FromDate = PostingDate, ToDate = PostingDate.AddMonths(1) }))
                .Single(c => c.VehicleNo == vehicle.No);
            cost.Distance.ShouldBe(200m);
            cost.TotalCost.ShouldBe(120m);
            cost.CostPerKm.ShouldBe(0.6m);
        });
    }

    [Fact]
    public async Task A_Segment_Becomes_A_Campaign_Audience_That_Is_Mailed()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var contacts = GetRequiredService<IRepository<Contact, Guid>>();
            foreach (var (no, email) in new[] { ("CT-MKT-1", "one@example.com"), ("CT-MKT-2", "two@example.com"), ("CT-MKT-3", (string)null) })
            {
                var contact = new Contact(Guid.NewGuid(), no, ContactType.Person, no);
                contact.SetCommunication(null, email, null, null);
                contact.SetAddress(null, "Mombasa", null);
                await contacts.InsertAsync(contact, autoSave: true);
            }

            await GetRequiredService<IRepository<Campaign, Guid>>().InsertAsync(new Campaign(Guid.NewGuid(), "CAMP-MKT", "Coast launch"), autoSave: true);

            var segments = GetRequiredService<ISegmentAppService>();
            var segment = await segments.CreateAsync(new CreateUpdateSegmentDto { Description = "Mombasa contacts" });
            (await segments.PickContactsAsync(segment.Id, new PickSegmentContactsInput { City = "Mombasa", OnlyWithEmail = false })).Count.ShouldBe(3);
            (await segments.PickContactsAsync(segment.Id, new PickSegmentContactsInput { City = "Mombasa", OnlyWithEmail = false })).Count.ShouldBe(0);
            (await segments.ApplyToCampaignAsync(segment.Id, new ApplySegmentInput { CampaignNo = "CAMP-MKT" })).Count.ShouldBe(3);

            var mailing = await GetRequiredService<ICampaignMailingAppService>().SendAsync(new SendMailingInput { CampaignNo = "CAMP-MKT", Subject = "We are open", BodyHtml = "<p>Hello {{Name}}</p>" });
            mailing.Sent.ShouldBe(2);
            mailing.WithoutEmail.ShouldBe(1);
            (await GetRequiredService<ICampaignMailingAppService>().GetListAsync(new GetCampaignMailingListInput { CampaignNo = "CAMP-MKT", MaxResultCount = 10 })).TotalCount.ShouldBe(3);
        });
    }

    /// <summary>A till at MAIN selling to the demo customer, paid into the operating account.</summary>
    private sealed class CreatePosTerminalInput
    {
        public CreateUpdatePosTerminalDto Build() => new()
        {
            Code = "TILL1",
            Description = "Front till",
            LocationCode = "MAIN",
            WalkInCustomerNo = "C00010",
            CashBankAccountNo = "WWB-OPERATING",
        };
    }
}
