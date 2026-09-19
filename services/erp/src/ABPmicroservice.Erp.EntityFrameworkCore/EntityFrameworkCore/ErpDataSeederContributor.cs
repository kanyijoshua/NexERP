using System;
using System.Linq;
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
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

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

    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentCompany _currentCompany;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

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
        IRepository<DimensionValue, Guid> dimensionValueRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        ICurrentCompany currentCompany,
        IUnitOfWorkManager unitOfWorkManager
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
        if (await _customReportLayoutRepository.GetCountAsync() == 0)
        {
            await _customReportLayoutRepository.InsertAsync(new CustomReportLayout(NewId(), "Trial Balance", "Standard Grid (RDLC)", "RDLC", "Standard Business Central tabular grid layout", isDefault: true));
            await _customReportLayoutRepository.InsertAsync(new CustomReportLayout(NewId(), "Trial Balance", "Executive Summary (Word/Print)", "Word", "Executive summary print layout with totals and charts"));
            await _customReportLayoutRepository.InsertAsync(new CustomReportLayout(NewId(), "Trial Balance", "Data Analyst (Excel Worksheet)", "Excel", "Full raw data excel layout formatted for financial modeling"));
        }

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

    private Guid NewId() => _guidGenerator.Create();
}
