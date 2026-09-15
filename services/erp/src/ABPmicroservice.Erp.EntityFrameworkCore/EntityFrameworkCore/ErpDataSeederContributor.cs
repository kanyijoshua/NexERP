using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Kanban;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public class ErpDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Company, Guid> _companyRepository;
    private readonly IRepository<CompanyInformation, Guid> _companyInformationRepository;
    private readonly IRepository<KanbanStage, Guid> _kanbanStageRepository;
    private readonly IRepository<CustomReportLayout, Guid> _customReportLayoutRepository;

    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly IRepository<CustomerPostingGroup, Guid> _custPostingGroupRepository;
    private readonly IRepository<VendorPostingGroup, Guid> _vendorPostingGroupRepository;
    private readonly IRepository<GeneralPostingSetup, Guid> _generalPostingSetupRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly IRepository<Dimension, Guid> _dimensionRepository;
    private readonly IRepository<DimensionValue, Guid> _dimensionValueRepository;

    public ErpDataSeederContributor(
        IRepository<Company, Guid> companyRepository,
        IRepository<CompanyInformation, Guid> companyInformationRepository,
        IRepository<KanbanStage, Guid> kanbanStageRepository,
        IRepository<CustomReportLayout, Guid> customReportLayoutRepository,
        IRepository<GLAccount, Guid> glAccountRepository,
        IRepository<CustomerPostingGroup, Guid> custPostingGroupRepository,
        IRepository<VendorPostingGroup, Guid> vendorPostingGroupRepository,
        IRepository<GeneralPostingSetup, Guid> generalPostingSetupRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Vendor, Guid> vendorRepository,
        IRepository<Item, Guid> itemRepository,
        IRepository<Dimension, Guid> dimensionRepository,
        IRepository<DimensionValue, Guid> dimensionValueRepository
    )
    {
        _companyRepository = companyRepository;
        _companyInformationRepository = companyInformationRepository;
        _kanbanStageRepository = kanbanStageRepository;
        _customReportLayoutRepository = customReportLayoutRepository;

        _glAccountRepository = glAccountRepository;
        _custPostingGroupRepository = custPostingGroupRepository;
        _vendorPostingGroupRepository = vendorPostingGroupRepository;
        _generalPostingSetupRepository = generalPostingSetupRepository;
        _customerRepository = customerRepository;
        _vendorRepository = vendorRepository;
        _itemRepository = itemRepository;
        _dimensionRepository = dimensionRepository;
        _dimensionValueRepository = dimensionValueRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _customReportLayoutRepository.GetCountAsync() == 0)
        {
            await _customReportLayoutRepository.InsertAsync(new CustomReportLayout(Guid.NewGuid(), "Trial Balance", "Standard Grid (RDLC)", "RDLC", "Standard Business Central tabular grid layout", isDefault: true));
            await _customReportLayoutRepository.InsertAsync(new CustomReportLayout(Guid.NewGuid(), "Trial Balance", "Executive Summary (Word/Print)", "Word", "Executive summary print layout with totals and charts"));
            await _customReportLayoutRepository.InsertAsync(new CustomReportLayout(Guid.NewGuid(), "Trial Balance", "Data Analyst (Excel Worksheet)", "Excel", "Full raw data excel layout formatted for financial modeling"));
        }
        if (await _companyRepository.GetCountAsync() == 0)
        {
            var defaultCompany = await _companyRepository.InsertAsync(new Company(Guid.NewGuid(), "CRONUS International Ltd.", "CRONUS International Ltd.", context.TenantId));
            await _companyInformationRepository.InsertAsync(new CompanyInformation(Guid.NewGuid(), defaultCompany.Id, defaultCompany.Name, "1 Main Street", "London", "GB123456789", context.TenantId));

            await _companyRepository.InsertAsync(new Company(Guid.NewGuid(), "CRONUS US", "CRONUS US", context.TenantId, evaluationCompany: true));
        }

        if (await _kanbanStageRepository.GetCountAsync() == 0)
        {
            await _kanbanStageRepository.InsertAsync(new KanbanStage(Guid.NewGuid(), "Sales", 1, "Draft"));
            await _kanbanStageRepository.InsertAsync(new KanbanStage(Guid.NewGuid(), "Sales", 2, "Pending Approval"));
            await _kanbanStageRepository.InsertAsync(new KanbanStage(Guid.NewGuid(), "Sales", 3, "Released"));
            await _kanbanStageRepository.InsertAsync(new KanbanStage(Guid.NewGuid(), "Sales", 4, "Posted", isWonStage: true));
        }

        if (await _glAccountRepository.GetCountAsync() > 0)
        {
            return;
        }

        // 1. Chart of Accounts
        await _glAccountRepository.InsertAsync(new GLAccount(Guid.NewGuid(), "1010", "Cash", GLAccountType.Posting, GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet));
        await _glAccountRepository.InsertAsync(new GLAccount(Guid.NewGuid(), "1020", "Bank Account", GLAccountType.Posting, GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet));
        await _glAccountRepository.InsertAsync(new GLAccount(Guid.NewGuid(), "1200", "Accounts Receivable", GLAccountType.Posting, GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet, directPosting: false));
        await _glAccountRepository.InsertAsync(new GLAccount(Guid.NewGuid(), "1400", "Inventory", GLAccountType.Posting, GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet, directPosting: false));
        await _glAccountRepository.InsertAsync(new GLAccount(Guid.NewGuid(), "2100", "Accounts Payable", GLAccountType.Posting, GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet, directPosting: false));
        await _glAccountRepository.InsertAsync(new GLAccount(Guid.NewGuid(), "4000", "Sales Revenue", GLAccountType.Posting, GLAccountCategory.Income, IncomeBalanceType.IncomeStatement));
        await _glAccountRepository.InsertAsync(new GLAccount(Guid.NewGuid(), "5000", "Cost of Goods Sold", GLAccountType.Posting, GLAccountCategory.CostOfGoodsSold, IncomeBalanceType.IncomeStatement));

        // 2. Posting Groups
        await _custPostingGroupRepository.InsertAsync(new CustomerPostingGroup(Guid.NewGuid(), "DOMESTIC", "1200", "Domestic Customers"));
        await _vendorPostingGroupRepository.InsertAsync(new VendorPostingGroup(Guid.NewGuid(), "DOMESTIC", "2100", "Domestic Vendors"));
        await _generalPostingSetupRepository.InsertAsync(new GeneralPostingSetup(Guid.NewGuid(), "DOMESTIC", "RETAIL", "4000", "5000", "5000", "5000"));

        // 3. Dimensions
        var deptDim = await _dimensionRepository.InsertAsync(new Dimension(Guid.NewGuid(), "DEPARTMENT", "Department"));
        await _dimensionValueRepository.InsertAsync(new DimensionValue(Guid.NewGuid(), deptDim.Id, "DEPARTMENT", "SALES", "Sales"));
        await _dimensionValueRepository.InsertAsync(new DimensionValue(Guid.NewGuid(), deptDim.Id, "DEPARTMENT", "ADMIN", "Administration"));

        // 4. Sample Master Data
        var cust = new Customer(Guid.NewGuid(), "C00010", "Adatum Corporation");
        cust.SetPostingGroups("DOMESTIC", "DOMESTIC");
        await _customerRepository.InsertAsync(cust);

        var vend = new Vendor(Guid.NewGuid(), "V00010", "Fabrikam Inc.");
        vend.SetPostingGroups("DOMESTIC", "DOMESTIC");
        await _vendorRepository.InsertAsync(vend);

        var item = new Item(Guid.NewGuid(), "1000", "Bicycle Assembly", ItemType.Inventory, "PCS", unitPrice: 300m, unitCost: 150m);
        item.SetPostingGroups("RETAIL", "RETAIL");
        await _itemRepository.InsertAsync(item);
    }
}
