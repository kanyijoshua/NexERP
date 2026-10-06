using ABPmicroservice.Erp.Attachments;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Chatter;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Integration;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.JobQueue;
using ABPmicroservice.Erp.Academics;
using ABPmicroservice.Erp.Payroll;
using ABPmicroservice.Erp.Kanban;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Pensions;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Workflows;
using AutoMapper;

namespace ABPmicroservice.Erp;

public class ErpApplicationAutoMapperProfile : Profile
{
    public ErpApplicationAutoMapperProfile()
    {
        // Companies
        CreateMap<Company, CompanyDto>();

        // Finance
        CreateMap<GLAccount, GLAccountDto>();
        CreateMap<GLEntry, GLEntryDto>();
        CreateMap<GenJournalTemplate, GenJournalTemplateDto>().ForMember(d => d.BatchCount, o => o.Ignore());
        // The line count, balance and recurring flag are filled from the batch's lines and template.
        CreateMap<GenJournalBatch, GenJournalBatchDto>()
            .ForMember(d => d.LineCount, o => o.Ignore())
            .ForMember(d => d.Balance, o => o.Ignore())
            .ForMember(d => d.Recurring, o => o.Ignore());
        CreateMap<GenJournalLine, GenJournalLineDto>();
        CreateMap<GLRegister, GLRegisterDto>();
        CreateMap<StandardGeneralJournal, StandardJournalDto>().ForMember(d => d.LineCount, o => o.Ignore());
        CreateMap<ReversalResult, ReversalResultDto>();
        CreateMap<GenJnlPostBatchResult, GenJournalPostingResultDto>();

        // Inventory
        CreateMap<Item, ItemDto>();
        CreateMap<ItemCategory, ItemCategoryDto>();
        CreateMap<UnitOfMeasure, UnitOfMeasureDto>();
        CreateMap<ItemLedgerEntry, ItemLedgerEntryDto>();

        // Sales
        CreateMap<Customer, CustomerDto>();
        CreateMap<SalesHeader, SalesHeaderDto>();
        CreateMap<SalesLine, SalesLineDto>();

        // Purchasing
        CreateMap<Vendor, VendorDto>();
        CreateMap<PurchaseHeader, PurchaseHeaderDto>();
        CreateMap<PurchaseLine, PurchaseLineDto>();

        // Number series: the summary fields are filled by NoSeriesAppService from the line in force.
        CreateMap<NoSeries, NoSeriesDto>()
            .ForMember(d => d.StartingNo, o => o.Ignore())
            .ForMember(d => d.EndingNo, o => o.Ignore())
            .ForMember(d => d.LastNoUsed, o => o.Ignore())
            .ForMember(d => d.NextNo, o => o.Ignore())
            .ForMember(d => d.Warning, o => o.Ignore());
        CreateMap<NoSeriesLine, NoSeriesLineDto>();
        CreateMap<SalesReceivablesSetup, SalesReceivablesSetupDto>();
        CreateMap<PurchasesPayablesSetup, PurchasesPayablesSetupDto>()
            .ForMember(d => d.CopyCommentsRetOrderToCrMemo, o => o.MapFrom(s => s.CopyCommentsRetOrdToCrMemo))
            .ForMember(d => d.CopyCommentsRetOrderToRetShpt, o => o.MapFrom(s => s.CopyCommentsRetOrdToRetShpt));

        // Posting groups and setups
        CreateMap<GenBusinessPostingGroup, PostingGroupDto>();
        CreateMap<GenProductPostingGroup, PostingGroupDto>();
        CreateMap<InventoryPostingGroup, PostingGroupDto>();
        CreateMap<CustomerPostingGroup, CustomerPostingGroupDto>();
        CreateMap<VendorPostingGroup, VendorPostingGroupDto>();
        CreateMap<GeneralPostingSetup, GeneralPostingSetupDto>();
        CreateMap<InventoryPostingSetup, InventoryPostingSetupDto>();
        CreateMap<GeneralLedgerSetup, GeneralLedgerSetupDto>();
        CreateMap<VatBusinessPostingGroup, PostingGroupDto>();
        CreateMap<VatProductPostingGroup, PostingGroupDto>();
        CreateMap<VatPostingSetup, VatPostingSetupDto>();
        CreateMap<VatEntry, VatEntryDto>();
        CreateMap<VatReturnLine, VatReturnLineDto>();
        CreateMap<VatReturnResult, VatReturnDto>();
        CreateMap<VatSettlementLine, VatSettlementLineDto>();
        CreateMap<VatSettlementResult, VatSettlementDto>();
        CreateMap<ExchRateAdjustmentLine, ExchRateAdjustmentLineDto>();
        CreateMap<ExchRateAdjustmentResult, ExchRateAdjustmentDto>();
        CreateMap<ExchRateAdjmtRegister, ExchRateAdjmtRegisterDto>();
        CreateMap<CustomerLedgerEntry, PartyLedgerEntryDto>().ForMember(d => d.PartyNo, o => o.MapFrom(s => s.CustomerNo));
        CreateMap<VendorLedgerEntry, PartyLedgerEntryDto>().ForMember(d => d.PartyNo, o => o.MapFrom(s => s.VendorNo));

        // Finance setup
        CreateMap<PaymentTerms, PaymentTermsDto>();
        CreateMap<Currency, CurrencyDto>();
        CreateMap<CurrencyExchangeRate, CurrencyExchangeRateDto>();
        CreateMap<AccountingPeriod, AccountingPeriodDto>();

        // Cash management
        CreateMap<BankAccountPostingGroup, BankAccountPostingGroupDto>();
        CreateMap<BankAccount, BankAccountDto>();
        CreateMap<BankAccountLedgerEntry, BankAccountLedgerEntryDto>();
        CreateMap<PaymentMethod, PaymentMethodDto>();

        // Inventory and sales setup
        CreateMap<Location, LocationDto>();
        CreateMap<InventorySetup, InventorySetupDto>();
        CreateMap<SalespersonPurchaser, SalespersonPurchaserDto>();

        // Human resources
        CreateMap<HumanResourcesSetup, HumanResourcesSetupDto>();
        CreateMap<HumanResourceUnitOfMeasure, HumanResourceUnitOfMeasureDto>();
        CreateMap<EmployeePostingGroup, EmployeePostingGroupDto>();
        CreateMap<CauseOfAbsence, CauseOfAbsenceDto>();
        CreateMap<Qualification, CodeTableDto>();
        CreateMap<Union, CodeTableDto>();
        CreateMap<EmploymentContract, CodeTableDto>();
        CreateMap<GroundsForTermination, CodeTableDto>();
        CreateMap<Employee, EmployeeDto>();
        CreateMap<EmployeeLedgerEntry, EmployeeLedgerEntryDto>();
        CreateMap<EmployeeAbsence, EmployeeAbsenceDto>();

        // Dimensions
        CreateMap<Dimension, DimensionDto>();
        CreateMap<DimensionValue, DimensionValueDto>();

        // Workflows
        CreateMap<Workflow, WorkflowDto>();
        CreateMap<WorkflowStep, WorkflowStepDto>();
        CreateMap<ApprovalEntry, ApprovalEntryDto>().ForMember(d => d.CanAct, o => o.Ignore());
        CreateMap<ApprovalUserSetup, ApprovalUserSetupDto>()
            .ForMember(d => d.ApproverUserName, o => o.Ignore())
            .ForMember(d => d.SubstituteUserName, o => o.Ignore());

        // Reporting
        CreateMap<ReportResult, ReportResultDto>();
        CreateMap<ReportColumnDefinition, ReportColumnDto>();
        CreateMap<ReportRow, ReportRowDto>();
        CreateMap<CustomReportLayout, ReportLayoutDto>()
            .ForMember(d => d.ReportCaption, o => o.Ignore());
        CreateMap<CustomReportLayout, ReportLayoutDetailDto>()
            .ForMember(d => d.ReportCaption, o => o.Ignore());
        CreateMap<AccountSchedule, AccountScheduleDto>().ForMember(d => d.LineCount, o => o.Ignore());
        CreateMap<AccountScheduleLine, AccountScheduleLineDto>();
        CreateMap<ColumnLayout, ColumnLayoutDto>().ForMember(d => d.LineCount, o => o.Ignore());
        CreateMap<ColumnLayoutLine, ColumnLayoutLineDto>();

        // Exporting and integration
        CreateMap<ExportTemplate, ExportTemplateDto>().ForMember(d => d.Fields, o => o.MapFrom(s => s.GetFields()));
        // The URL is built by the service, which knows the route; the secret is shown only once.
        CreateMap<PublishedWebService, PublishedWebServiceDto>()
            .ForMember(d => d.Url, o => o.Ignore())
            .ForMember(d => d.RequestBody, o => o.Ignore())
            .ForMember(d => d.FieldsUrl, o => o.Ignore());
        CreateMap<WebhookSubscription, WebhookSubscriptionDto>().ForMember(d => d.Secret, o => o.Ignore());
        CreateMap<WebhookDelivery, WebhookDeliveryDto>();

        // Chatter and Kanban
        CreateMap<DocumentNote, DocumentNoteDto>();
        CreateMap<DocumentAttachment, DocumentAttachmentDto>();

        // Pensions
        CreateMap<PensionSetup, PensionSetupDto>();
        CreateMap<PensionScheme, PensionSchemeDto>();
        CreateMap<PensionSponsor, PensionSponsorDto>();
        CreateMap<PensionMember, PensionMemberDto>();
        CreateMap<MemberLedgerEntry, MemberLedgerEntryDto>();
        CreateMap<PensionContributionHeader, PensionContributionHeaderDto>();
        CreateMap<PensionContributionLine, PensionContributionLineDto>();
        CreateMap<PensionInterestRate, PensionInterestRateDto>();
        CreateMap<ExitReason, ExitReasonDto>();
        CreateMap<LumpsumTaxTable, LumpsumTaxTableDto>();
        CreateMap<LumpsumTaxBand, LumpsumTaxBandDto>();
        CreateMap<MemberExit, MemberExitDto>();

        // Academics
        CreateMap<AcademicSetup, AcademicSetupDto>();
        CreateMap<AcademicYear, AcademicYearDto>();
        CreateMap<Semester, SemesterDto>();
        CreateMap<Intake, IntakeDto>();
        CreateMap<ExamCategory, ExamCategoryDto>();
        CreateMap<GradingBand, GradingBandDto>();
        CreateMap<ExamComponent, ExamComponentDto>();
        CreateMap<Programme, ProgrammeDto>();
        CreateMap<ProgrammeStage, ProgrammeStageDto>();
        CreateMap<CourseUnit, CourseUnitDto>();
        CreateMap<FeeItem, FeeItemDto>();
        CreateMap<FeeStructureLine, FeeStructureLineDto>();
        CreateMap<StudentApplication, StudentApplicationDto>();
        CreateMap<Student, StudentDto>();
        CreateMap<SemesterRegistration, SemesterRegistrationDto>();
        CreateMap<StudentUnit, StudentUnitDto>();
        CreateMap<StudentBillHeader, StudentBillHeaderDto>();
        CreateMap<StudentBillLine, StudentBillLineDto>();
        CreateMap<ExamResultHeader, ExamResultHeaderDto>();
        CreateMap<ExamResultLine, ExamResultLineDto>();
        CreateMap<StudentReceipt, StudentReceiptDto>();
        CreateMap<StudentRefund, StudentRefundDto>();
        CreateMap<StudentStatusChange, StudentStatusChangeDto>();

        // Pensioners and pension payroll
        CreateMap<Pensioner, PensionerDto>();
        CreateMap<PensionPayrollHeader, PensionPayrollHeaderDto>();
        CreateMap<PensionPayrollLine, PensionPayrollLineDto>();
        CreateMap<PensionBenefitCalculation, PensionBenefitCalculationDto>();
        CreateMap<PensionBeneficiary, PensionBeneficiaryDto>();
        CreateMap<PensionContributionRate, PensionContributionRateDto>();
        CreateMap<PensionVestingScale, PensionVestingScaleDto>();
        CreateMap<PensionTaxReliefLimit, PensionTaxReliefLimitDto>();
        CreateMap<MemberStatusEntry, MemberStatusEntryDto>();
        CreateMap<MemberSalaryEntry, MemberSalaryEntryDto>();
        CreateMap<PensionIncrement, PensionIncrementDto>();
        CreateMap<PensionerChangeEntry, PensionerChangeEntryDto>();
        CreateMap<PensionBank, PensionBankDto>();
        CreateMap<PensionBankBranch, PensionBankBranchDto>();
        CreateMap<PensionerPayMode, PensionerPayModeDto>();
        CreateMap<PensionerSuspensionReason, PensionerSuspensionReasonDto>();
        CreateMap<PensionRevisionReason, CodeTableDto>();
        CreateMap<OtherPensionScheme, OtherPensionSchemeDto>();
        CreateMap<PensionerPayItem, PensionerPayItemDto>();
        CreateMap<PensionerPayItemAssignment, PensionerPayItemAssignmentDto>()
            .ForMember(d => d.PayItemDescription, o => o.Ignore())
            .ForMember(d => d.ItemType, o => o.Ignore());
        CreateMap<PensionPayrollLineItem, PensionPayrollLineItemDto>();
        CreateMap<ExitReasonDocument, ExitReasonDocumentDto>();
        CreateMap<MemberExitDocument, MemberExitDocumentDto>();
        CreateMap<PensionAgeFactor, PensionAgeFactorDto>();

        // Payroll
        CreateMap<PayrollSetup, PayrollSetupDto>();
        CreateMap<PayrollEarning, PayrollEarningDto>();
        CreateMap<PayrollDeduction, PayrollDeductionDto>();
        CreateMap<PayrollTaxBand, PayrollTaxBandDto>();
        CreateMap<EmployeePayItem, EmployeePayItemDto>();
        CreateMap<PayrollRun, PayrollRunDto>();
        CreateMap<Payslip, PayslipDto>();
        CreateMap<PayslipLine, PayslipLineDto>();

        // Campus: timetable, attendance, hostels, infirmary, laundry, short courses
        CreateMap<LectureRoom, LectureRoomDto>();
        CreateMap<TimetableEntry, TimetableEntryDto>();
        CreateMap<AttendanceRegister, AttendanceRegisterDto>();
        CreateMap<AttendanceLine, AttendanceLineDto>();
        CreateMap<Hostel, HostelDto>();
        CreateMap<HostelRoom, HostelRoomDto>();
        CreateMap<HostelAllocation, HostelAllocationDto>();
        CreateMap<ClinicVisit, ClinicVisitDto>();
        CreateMap<ClinicPrescription, ClinicPrescriptionDto>();
        CreateMap<LaundryItem, LaundryItemDto>();
        CreateMap<LaundryOrder, LaundryOrderDto>();
        CreateMap<LaundryOrderLine, LaundryOrderLineDto>();
        CreateMap<ShortCourse, ShortCourseDto>();
        CreateMap<ShortCourseApplication, ShortCourseApplicationDto>();
        CreateMap<ShortCourseParticipant, ShortCourseParticipantDto>();

        // Payment vouchers
        CreateMap<CashManagementSetup, CashManagementSetupDto>();
        CreateMap<PaymentDeductionCode, PaymentDeductionCodeDto>();
        CreateMap<PaymentType, PaymentTypeDto>();
        CreateMap<PaymentVoucherHeader, PaymentVoucherHeaderDto>();
        CreateMap<PaymentVoucherLine, PaymentVoucherLineDto>();
        CreateMap<ActivityStreamEntry, ActivityStreamEntryDto>();
        CreateMap<DocumentActivityTask, DocumentActivityTaskDto>();
        CreateMap<KanbanStage, KanbanStageDto>();

        // Job Queue
        CreateMap<JobQueueCategory, JobQueueCategoryDto>();
        CreateMap<JobQueueEntry, JobQueueEntryDto>();
        CreateMap<JobQueueLogEntry, JobQueueLogEntryDto>();
    }
}
