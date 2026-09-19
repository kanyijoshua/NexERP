using ABPmicroservice.Erp.Chatter;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Finance;
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
        CreateMap<GenJournalBatch, GenJournalBatchDto>();
        CreateMap<GenJournalLine, GenJournalLineDto>();

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
        CreateMap<FinancialReportResultDto, FinancialReportDto>();
        CreateMap<FinancialReportRowDto, FinancialReportLineDto>();
        CreateMap<CustomReportLayout, ReportLayoutDto>();

        // Chatter and Kanban
        CreateMap<DocumentNote, DocumentNoteDto>();
        CreateMap<ActivityStreamEntry, ActivityStreamEntryDto>();
        CreateMap<DocumentActivityTask, DocumentActivityTaskDto>();
        CreateMap<KanbanStage, KanbanStageDto>();
    }
}
