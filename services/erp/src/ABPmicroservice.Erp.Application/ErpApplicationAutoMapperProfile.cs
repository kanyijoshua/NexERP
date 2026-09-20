using ABPmicroservice.Erp.Chatter;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Integration;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Kanban;
using ABPmicroservice.Erp.Numbering;
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
        CreateMap<PurchasesPayablesSetup, PurchasesPayablesSetupDto>();

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
        CreateMap<CustomReportLayout, ReportLayoutDto>();
        CreateMap<AccountSchedule, AccountScheduleDto>().ForMember(d => d.LineCount, o => o.Ignore());
        CreateMap<AccountScheduleLine, AccountScheduleLineDto>();
        CreateMap<ColumnLayout, ColumnLayoutDto>().ForMember(d => d.LineCount, o => o.Ignore());
        CreateMap<ColumnLayoutLine, ColumnLayoutLineDto>();

        // Exporting and integration
        CreateMap<ExportTemplate, ExportTemplateDto>().ForMember(d => d.Fields, o => o.MapFrom(s => s.GetFields()));
        // The URL is built by the service, which knows the route; the secret is shown only once.
        CreateMap<PublishedWebService, PublishedWebServiceDto>().ForMember(d => d.Url, o => o.Ignore());
        CreateMap<WebhookSubscription, WebhookSubscriptionDto>().ForMember(d => d.Secret, o => o.Ignore());
        CreateMap<WebhookDelivery, WebhookDeliveryDto>();

        // Chatter and Kanban
        CreateMap<DocumentNote, DocumentNoteDto>();
        CreateMap<ActivityStreamEntry, ActivityStreamEntryDto>();
        CreateMap<DocumentActivityTask, DocumentActivityTaskDto>();
        CreateMap<KanbanStage, KanbanStageDto>();
    }
}
