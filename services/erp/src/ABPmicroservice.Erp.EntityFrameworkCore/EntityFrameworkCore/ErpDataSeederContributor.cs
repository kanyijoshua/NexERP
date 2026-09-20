using System;
using System.Linq;
using System.Threading.Tasks;
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
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public class ErpDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Company, Guid> _companyRepository;
    private readonly IRepository<CompanyInformation, Guid> _companyInformationRepository;
    private readonly IRepository<KanbanStage, Guid> _kanbanStageRepository;

    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly IRepository<CustomerPostingGroup, Guid> _custPostingGroupRepository;
    private readonly IRepository<VendorPostingGroup, Guid> _vendorPostingGroupRepository;
    private readonly IRepository<GeneralPostingSetup, Guid> _generalPostingSetupRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly IRepository<Dimension, Guid> _dimensionRepository;
    private readonly IRepository<DimensionValue, Guid> _dimensionValueRepository;

    private readonly IRepository<NoSeries, Guid> _noSeriesRepository;
    private readonly IRepository<SalesReceivablesSetup, Guid> _salesSetupRepository;
    private readonly IRepository<PurchasesPayablesSetup, Guid> _purchaseSetupRepository;
    private readonly IRepository<Workflow, Guid> _workflowRepository;

    private readonly IRepository<GenJournalTemplate, Guid> _journalTemplateRepository;
    private readonly IRepository<GenJournalBatch, Guid> _journalBatchRepository;
    private readonly IRepository<AccountSchedule, Guid> _accountScheduleRepository;
    private readonly IRepository<AccountScheduleLine, Guid> _accountScheduleLineRepository;
    private readonly IRepository<ColumnLayout, Guid> _columnLayoutRepository;
    private readonly IRepository<ColumnLayoutLine, Guid> _columnLayoutLineRepository;

    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentCompany _currentCompany;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public ErpDataSeederContributor(
        IRepository<Company, Guid> companyRepository,
        IRepository<CompanyInformation, Guid> companyInformationRepository,
        IRepository<KanbanStage, Guid> kanbanStageRepository,
        IRepository<GLAccount, Guid> glAccountRepository,
        IRepository<CustomerPostingGroup, Guid> custPostingGroupRepository,
        IRepository<VendorPostingGroup, Guid> vendorPostingGroupRepository,
        IRepository<GeneralPostingSetup, Guid> generalPostingSetupRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Vendor, Guid> vendorRepository,
        IRepository<Item, Guid> itemRepository,
        IRepository<Dimension, Guid> dimensionRepository,
        IRepository<DimensionValue, Guid> dimensionValueRepository,
        IRepository<NoSeries, Guid> noSeriesRepository,
        IRepository<SalesReceivablesSetup, Guid> salesSetupRepository,
        IRepository<PurchasesPayablesSetup, Guid> purchaseSetupRepository,
        IRepository<Workflow, Guid> workflowRepository,
        IRepository<GenJournalTemplate, Guid> journalTemplateRepository,
        IRepository<GenJournalBatch, Guid> journalBatchRepository,
        IRepository<AccountSchedule, Guid> accountScheduleRepository,
        IRepository<AccountScheduleLine, Guid> accountScheduleLineRepository,
        IRepository<ColumnLayout, Guid> columnLayoutRepository,
        IRepository<ColumnLayoutLine, Guid> columnLayoutLineRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        ICurrentCompany currentCompany,
        IUnitOfWorkManager unitOfWorkManager
    )
    {
        _companyRepository = companyRepository;
        _companyInformationRepository = companyInformationRepository;
        _kanbanStageRepository = kanbanStageRepository;

        _glAccountRepository = glAccountRepository;
        _custPostingGroupRepository = custPostingGroupRepository;
        _vendorPostingGroupRepository = vendorPostingGroupRepository;
        _generalPostingSetupRepository = generalPostingSetupRepository;
        _customerRepository = customerRepository;
        _vendorRepository = vendorRepository;
        _itemRepository = itemRepository;
        _dimensionRepository = dimensionRepository;
        _dimensionValueRepository = dimensionValueRepository;

        _noSeriesRepository = noSeriesRepository;
        _salesSetupRepository = salesSetupRepository;
        _purchaseSetupRepository = purchaseSetupRepository;
        _workflowRepository = workflowRepository;

        _journalTemplateRepository = journalTemplateRepository;
        _journalBatchRepository = journalBatchRepository;
        _accountScheduleRepository = accountScheduleRepository;
        _accountScheduleLineRepository = accountScheduleLineRepository;
        _columnLayoutRepository = columnLayoutRepository;
        _columnLayoutLineRepository = columnLayoutLineRepository;

        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _currentCompany = currentCompany;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            if (await _companyRepository.GetCountAsync() == 0)
            {
                var cronus = await _companyRepository.InsertAsync(
                    new Company(NewId(), "CRONUS International Ltd.", "CRONUS International Ltd.", context?.TenantId, isDefault: true),
                    autoSave: true
                );
                await _companyInformationRepository.InsertAsync(
                    new CompanyInformation(NewId(), cronus.Id, cronus.Name, "1 Main Street", "London", "GB123456789", context?.TenantId)
                );
                await _companyRepository.InsertAsync(
                    new Company(NewId(), "CRONUS US", "CRONUS US", context?.TenantId, evaluationCompany: true),
                    autoSave: true
                );
            }

            // Everything below is company data: it is stamped with, and filtered by, the ambient company.
            var companies = (await _companyRepository.GetListAsync()).OrderBy(c => c.CreationTime).ToList();
            var defaultCompany = companies.FirstOrDefault(c => c.IsDefault) ?? companies.First();

            foreach (var company in companies)
            {
                using (_currentCompany.Change(company.Id, company.Name))
                {
                    await SeedCompanySetupAsync();
                    await SeedNumberSeriesAsync();
                    await SeedApprovalWorkflowTemplatesAsync();
                    await SeedJournalTemplatesAsync();
                    await SeedFinancialReportsAsync();

                    if (company.Id == defaultCompany.Id)
                    {
                        await SeedSampleMasterDataAsync();
                    }

                    // Flush while this company is still ambient; CompanyId is stamped on save.
                    if (_unitOfWorkManager.Current != null)
                    {
                        await _unitOfWorkManager.Current.SaveChangesAsync();
                    }
                }
            }
        }
    }

    private async Task SeedCompanySetupAsync()
    {
        // No report layouts are seeded: with none, every report prints through the built-in
        // layout, which is also what a user downloads to start a custom one from.

        if (await _kanbanStageRepository.GetCountAsync() == 0)
        {
            await _kanbanStageRepository.InsertAsync(new KanbanStage(NewId(), "Sales", 1, "Draft"));
            await _kanbanStageRepository.InsertAsync(new KanbanStage(NewId(), "Sales", 2, "Pending Approval"));
            await _kanbanStageRepository.InsertAsync(new KanbanStage(NewId(), "Sales", 3, "Released"));
            await _kanbanStageRepository.InsertAsync(new KanbanStage(NewId(), "Sales", 4, "Posted", isWonStage: true));
        }

        if (await _glAccountRepository.GetCountAsync() > 0)
        {
            return;
        }

        // 1. Chart of Accounts
        await _glAccountRepository.InsertAsync(new GLAccount(NewId(), "1010", "Cash", GLAccountType.Posting, GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet));
        await _glAccountRepository.InsertAsync(new GLAccount(NewId(), "1020", "Bank Account", GLAccountType.Posting, GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet));
        await _glAccountRepository.InsertAsync(new GLAccount(NewId(), "1200", "Accounts Receivable", GLAccountType.Posting, GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet, directPosting: false));
        await _glAccountRepository.InsertAsync(new GLAccount(NewId(), "1400", "Inventory", GLAccountType.Posting, GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet, directPosting: false));
        await _glAccountRepository.InsertAsync(new GLAccount(NewId(), "2100", "Accounts Payable", GLAccountType.Posting, GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet, directPosting: false));
        await _glAccountRepository.InsertAsync(new GLAccount(NewId(), "4000", "Sales Revenue", GLAccountType.Posting, GLAccountCategory.Income, IncomeBalanceType.IncomeStatement));
        await _glAccountRepository.InsertAsync(new GLAccount(NewId(), "5000", "Cost of Goods Sold", GLAccountType.Posting, GLAccountCategory.CostOfGoodsSold, IncomeBalanceType.IncomeStatement));

        // 2. Posting Groups
        await _custPostingGroupRepository.InsertAsync(new CustomerPostingGroup(NewId(), "DOMESTIC", "1200", "Domestic Customers"));
        await _vendorPostingGroupRepository.InsertAsync(new VendorPostingGroup(NewId(), "DOMESTIC", "2100", "Domestic Vendors"));
        await _generalPostingSetupRepository.InsertAsync(new GeneralPostingSetup(NewId(), "DOMESTIC", "RETAIL", "4000", "5000", "5000", "5000"));

        // 3. Dimensions
        var deptDim = await _dimensionRepository.InsertAsync(new Dimension(NewId(), "DEPARTMENT", "Department"));
        await _dimensionValueRepository.InsertAsync(new DimensionValue(NewId(), deptDim.Id, "DEPARTMENT", "SALES", "Sales"));
        await _dimensionValueRepository.InsertAsync(new DimensionValue(NewId(), deptDim.Id, "DEPARTMENT", "ADMIN", "Administration"));
    }

    // The CRONUS set: documents may also be numbered by hand; posted documents may not,
    // and posted invoices must run in date order.
    private async Task SeedNumberSeriesAsync()
    {
        if (await _noSeriesRepository.GetCountAsync() > 0)
        {
            return;
        }

        await AddSeriesAsync("CUST", "Customers", "C00020", manualNos: true, incrementByNo: 10);
        await AddSeriesAsync("VEND", "Vendors", "V00020", manualNos: true, incrementByNo: 10);

        await AddSeriesAsync("S-QUO", "Sales Quotes", "SQ-00001", manualNos: true);
        await AddSeriesAsync("S-ORD", "Sales Orders", "SO-00001", manualNos: true);
        await AddSeriesAsync("S-INV", "Sales Invoices", "SI-00001", manualNos: true);
        await AddSeriesAsync("S-CR", "Sales Credit Memos", "SCM-00001", manualNos: true);
        await AddSeriesAsync("S-INV+", "Posted Sales Invoices", "PSI-00001", dateOrder: true);
        await AddSeriesAsync("S-CR+", "Posted Sales Credit Memos", "PSCM-00001", dateOrder: true);

        await AddSeriesAsync("P-QUO", "Purchase Quotes", "PQ-00001", manualNos: true);
        await AddSeriesAsync("P-ORD", "Purchase Orders", "PO-00001", manualNos: true);
        await AddSeriesAsync("P-INV", "Purchase Invoices", "PI-00001", manualNos: true);
        await AddSeriesAsync("P-CR", "Purchase Credit Memos", "PCM-00001", manualNos: true);
        await AddSeriesAsync("P-INV+", "Posted Purchase Invoices", "PPI-00001", dateOrder: true);
        await AddSeriesAsync("P-CR+", "Posted Purchase Credit Memos", "PPCM-00001", dateOrder: true);

        if (await _salesSetupRepository.GetCountAsync() == 0)
        {
            var salesSetup = new SalesReceivablesSetup(NewId());
            salesSetup.SetNumberSeries("CUST", "S-QUO", "S-ORD", "S-INV", "S-CR", "S-INV+", "S-CR+");
            await _salesSetupRepository.InsertAsync(salesSetup);
        }

        if (await _purchaseSetupRepository.GetCountAsync() == 0)
        {
            var purchaseSetup = new PurchasesPayablesSetup(NewId());
            purchaseSetup.SetNumberSeries("VEND", "P-QUO", "P-ORD", "P-INV", "P-CR", "P-INV+", "P-CR+");
            await _purchaseSetupRepository.InsertAsync(purchaseSetup);
        }
    }

    private async Task AddSeriesAsync(
        string code,
        string description,
        string startingNo,
        bool manualNos = false,
        bool dateOrder = false,
        int incrementByNo = 1
    )
    {
        var series = new NoSeries(NewId(), code, description, defaultNos: true, manualNos, dateOrder);
        series.AddLine(NewId(), null, startingNo, incrementByNo: incrementByNo);
        await _noSeriesRepository.InsertAsync(series);
    }

    // Templates only: they stay disabled until an administrator has filled in the Approval User Setup.
    private async Task SeedApprovalWorkflowTemplatesAsync()
    {
        if (await _workflowRepository.GetCountAsync() > 0)
        {
            return;
        }

        var sales = new Workflow(NewId(), "SIAPW", "Sales Document Approval Workflow", ApprovalDocumentKind.SalesDocument, dueDays: 3);
        sales.RebuildSteps(NewId);
        await _workflowRepository.InsertAsync(sales);

        var purchase = new Workflow(NewId(), "PIAPW", "Purchase Document Approval Workflow", ApprovalDocumentKind.PurchaseDocument, dueDays: 3);
        purchase.RebuildSteps(NewId);
        await _workflowRepository.InsertAsync(purchase);
    }

    private async Task SeedSampleMasterDataAsync()
    {
        if (await _customerRepository.GetCountAsync() > 0)
        {
            return;
        }

        var cust = new Customer(NewId(), "C00010", "Adatum Corporation");
        cust.SetPostingGroups("DOMESTIC", "DOMESTIC");
        await _customerRepository.InsertAsync(cust);

        var vend = new Vendor(NewId(), "V00010", "Fabrikam Inc.");
        vend.SetPostingGroups("DOMESTIC", "DOMESTIC");
        await _vendorRepository.InsertAsync(vend);

        var item = new Item(NewId(), "1000", "Bicycle Assembly", ItemType.Inventory, "PCS", unitPrice: 300m, unitCost: 150m);
        item.SetPostingGroups("RETAIL", "RETAIL");
        await _itemRepository.InsertAsync(item);
    }

    /// <summary>
    /// The CRONUS journals: a general one, a recurring one for accruals, and the two payment
    /// journals, each with the batch people actually type into.
    /// </summary>
    private async Task SeedJournalTemplatesAsync()
    {
        if (await _journalTemplateRepository.GetCountAsync() > 0)
        {
            return;
        }

        await AddTemplateAsync("GENERAL", "General Journal", GenJournalTemplateType.General, "GENJNL", "DEFAULT");
        await AddTemplateAsync(
            "RECURRING",
            "Recurring General Journal",
            GenJournalTemplateType.General,
            "GENJNL",
            "DEFAULT",
            recurring: true
        );
        await AddTemplateAsync(
            "CASHRCPT",
            "Cash Receipt Journal",
            GenJournalTemplateType.CashReceipts,
            "CASHRECJNL",
            "CASH",
            balAccountNo: "1020"
        );
        await AddTemplateAsync(
            "PAYMENT",
            "Payment Journal",
            GenJournalTemplateType.Payments,
            "PAYMENTJNL",
            "BANK",
            balAccountNo: "1020"
        );
    }

    private async Task AddTemplateAsync(
        string name,
        string description,
        GenJournalTemplateType type,
        string sourceCode,
        string batchName,
        bool recurring = false,
        string balAccountNo = null
    )
    {
        await _journalTemplateRepository.InsertAsync(
            new GenJournalTemplate(NewId(), name, description, type, recurring, sourceCode)
        );

        await _journalBatchRepository.InsertAsync(
            new GenJournalBatch(
                NewId(),
                name,
                batchName,
                description,
                balAccountType: balAccountNo == null ? null : GenJournalAccountType.GLAccount,
                balAccountNo: balAccountNo
            )
        );
    }

    /// <summary>
    /// A balance sheet and an income statement defined as account schedules, plus the column
    /// layouts BC ships: this period, and this period against the same one last year.
    /// </summary>
    private async Task SeedFinancialReportsAsync()
    {
        if (await _accountScheduleRepository.GetCountAsync() > 0)
        {
            return;
        }

        var period = new ColumnLayout(NewId(), "PERIODS", "Net change and balance for the period");
        await _columnLayoutRepository.InsertAsync(period, autoSave: true);
        await AddColumnAsync(period.Id, 10, "C10", "Net Change", ColumnLayoutType.NetChange);
        await AddColumnAsync(period.Id, 20, "C20", "Balance at Date", ColumnLayoutType.BalanceAtDate);

        var comparison = new ColumnLayout(NewId(), "LASTYEAR", "This period against the same period last year");
        await _columnLayoutRepository.InsertAsync(comparison, autoSave: true);
        await AddColumnAsync(comparison.Id, 10, "C10", "This Year", ColumnLayoutType.NetChange);
        await AddColumnAsync(comparison.Id, 20, "C20", "Last Year", ColumnLayoutType.NetChange, "-1Y");

        var balanceSheet = new AccountSchedule(NewId(), "BALANCE", "Balance Sheet", "PERIODS");
        await _accountScheduleRepository.InsertAsync(balanceSheet, autoSave: true);
        await AddScheduleLineAsync(balanceSheet.Id, 10, "R10", "Assets", AccountScheduleTotalingType.Description, "-", bold: true);
        await AddScheduleLineAsync(balanceSheet.Id, 20, "R20", "Cash and Bank", AccountScheduleTotalingType.PostingAccounts, "1010..1020", indentation: 1);
        await AddScheduleLineAsync(balanceSheet.Id, 30, "R30", "Accounts Receivable", AccountScheduleTotalingType.PostingAccounts, "1200", indentation: 1);
        await AddScheduleLineAsync(balanceSheet.Id, 40, "R40", "Inventory", AccountScheduleTotalingType.PostingAccounts, "1400", indentation: 1);
        await AddScheduleLineAsync(balanceSheet.Id, 50, "R50", "Total Assets", AccountScheduleTotalingType.Formula, "R20+R30+R40", bold: true);
        await AddScheduleLineAsync(balanceSheet.Id, 60, "R60", "Liabilities", AccountScheduleTotalingType.Description, "-", bold: true);
        // Liabilities carry credit balances, so the sign is flipped to read as a positive figure.
        await AddScheduleLineAsync(balanceSheet.Id, 70, "R70", "Accounts Payable", AccountScheduleTotalingType.PostingAccounts, "2100", showOppositeSign: true, indentation: 1);
        await AddScheduleLineAsync(balanceSheet.Id, 80, "R80", "Total Liabilities", AccountScheduleTotalingType.Formula, "R70", bold: true);

        var income = new AccountSchedule(NewId(), "INCOME", "Income Statement", "LASTYEAR");
        await _accountScheduleRepository.InsertAsync(income, autoSave: true);
        await AddScheduleLineAsync(income.Id, 10, "R10", "Revenue", AccountScheduleTotalingType.PostingAccounts, "4000..4999", showOppositeSign: true);
        await AddScheduleLineAsync(income.Id, 20, "R20", "Cost of Goods Sold", AccountScheduleTotalingType.PostingAccounts, "5000..5999");
        await AddScheduleLineAsync(income.Id, 30, "R30", "Gross Profit", AccountScheduleTotalingType.Formula, "R10-R20", bold: true);
    }

    private async Task AddColumnAsync(
        Guid layoutId,
        int lineNo,
        string columnNo,
        string header,
        ColumnLayoutType type,
        string comparisonFormula = null
    )
    {
        await _columnLayoutLineRepository.InsertAsync(
            new ColumnLayoutLine(NewId(), layoutId, lineNo, columnNo, header, type, comparisonFormula)
        );
    }

    private async Task AddScheduleLineAsync(
        Guid scheduleId,
        int lineNo,
        string rowNo,
        string description,
        AccountScheduleTotalingType totalingType,
        string totaling,
        bool showOppositeSign = false,
        bool bold = false,
        int indentation = 0
    )
    {
        await _accountScheduleLineRepository.InsertAsync(
            new AccountScheduleLine(
                NewId(),
                scheduleId,
                lineNo,
                rowNo,
                description,
                totalingType,
                totaling,
                showOppositeSign,
                bold,
                italic: false,
                indentation: indentation
            )
        );
    }

    private Guid NewId() => _guidGenerator.Create();
}
