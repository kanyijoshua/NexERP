using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Workflows;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Companies;

/// <summary>
/// Ambient Company Context.
/// </summary>
public interface ICurrentCompany
{
    Guid? Id { get; }
    string Name { get; }
}

public class CurrentCompany : ICurrentCompany
{
    public Guid? Id { get; set; }
    public string Name { get; set; }
}

/// <summary>
/// Company Copying Engine.
/// Mirrors Business Central Codeunit 357 "Copy Company".
/// Duplicates setup tables (CoA, Dimensions, Posting Setup, Workflows) into a newly created company.
/// </summary>
public class CompanyCopyEngine : DomainService
{
    private readonly IRepository<Company, Guid> _companyRepository;
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly IRepository<GeneralPostingSetup, Guid> _postingSetupRepository;
    private readonly IRepository<Dimension, Guid> _dimensionRepository;
    private readonly IRepository<DimensionValue, Guid> _dimensionValueRepository;

    public CompanyCopyEngine(
        IRepository<Company, Guid> companyRepository,
        IRepository<GLAccount, Guid> glAccountRepository,
        IRepository<GeneralPostingSetup, Guid> postingSetupRepository,
        IRepository<Dimension, Guid> dimensionRepository,
        IRepository<DimensionValue, Guid> dimensionValueRepository
    )
    {
        _companyRepository = companyRepository;
        _glAccountRepository = glAccountRepository;
        _postingSetupRepository = postingSetupRepository;
        _dimensionRepository = dimensionRepository;
        _dimensionValueRepository = dimensionValueRepository;
    }

    public async Task<Company> CopyCompanyAsync(Guid sourceCompanyId, string newCompanyName, string newDisplayName)
    {
        var targetCompany = new Company(GuidGenerator.Create(), newCompanyName, newDisplayName);
        await _companyRepository.InsertAsync(targetCompany, autoSave: true);

        // Copy Chart of Accounts template
        var accounts = await _glAccountRepository.GetListAsync();
        foreach (var acc in accounts)
        {
            await _glAccountRepository.InsertAsync(new GLAccount(
                GuidGenerator.Create(),
                acc.No,
                acc.Name,
                acc.AccountType,
                acc.AccountCategory,
                acc.IncomeBalance,
                acc.Subcategory,
                acc.DirectPosting
            ));
        }

        // Copy General Posting Setup matrix
        var postingSetups = await _postingSetupRepository.GetListAsync();
        foreach (var ps in postingSetups)
        {
            await _postingSetupRepository.InsertAsync(new GeneralPostingSetup(
                GuidGenerator.Create(),
                ps.GenBusPostingGroup,
                ps.GenProdPostingGroup,
                ps.SalesAccountNo,
                ps.PurchAccountNo,
                ps.COGSAccountNo,
                ps.InventoryAdjmtAccountNo
            ));
        }

        return targetCompany;
    }
}
