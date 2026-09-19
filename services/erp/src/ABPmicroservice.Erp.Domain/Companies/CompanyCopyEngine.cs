using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Companies;

/// <summary>
/// Company Copying Engine.
/// Mirrors Business Central Codeunit 357 "Copy Company".
/// Duplicates setup tables (CoA, Posting Setup) into a newly created company.
/// </summary>
public class CompanyCopyEngine : DomainService
{
    private readonly IRepository<Company, Guid> _companyRepository;
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly IRepository<GeneralPostingSetup, Guid> _postingSetupRepository;
    private readonly ICurrentCompany _currentCompany;

    public CompanyCopyEngine(
        IRepository<Company, Guid> companyRepository,
        IRepository<GLAccount, Guid> glAccountRepository,
        IRepository<GeneralPostingSetup, Guid> postingSetupRepository,
        ICurrentCompany currentCompany
    )
    {
        _companyRepository = companyRepository;
        _glAccountRepository = glAccountRepository;
        _postingSetupRepository = postingSetupRepository;
        _currentCompany = currentCompany;
    }

    public async Task<Company> CopyCompanyAsync(Guid sourceCompanyId, string newCompanyName, string newDisplayName)
    {
        var sourceCompany = await _companyRepository.GetAsync(sourceCompanyId);
        if (await _companyRepository.AnyAsync(c => c.Name == newCompanyName))
        {
            throw new BusinessException(ErpErrorCodes.Companies.CompanyNameAlreadyExists).WithData("Name", newCompanyName);
        }

        var targetCompany = new Company(GuidGenerator.Create(), newCompanyName, newDisplayName, CurrentTenant.Id);
        await _companyRepository.InsertAsync(targetCompany, autoSave: true);

        // Read the templates inside the source company...
        List<GLAccount> accounts;
        List<GeneralPostingSetup> postingSetups;
        using (_currentCompany.Change(sourceCompany.Id, sourceCompany.Name))
        {
            accounts = await _glAccountRepository.GetListAsync();
            postingSetups = await _postingSetupRepository.GetListAsync();
        }

        // ...and write the copies inside the new one, so they are stamped with its id.
        // autoSave flushes while the target company is still ambient: CompanyId is stamped on save.
        using (_currentCompany.Change(targetCompany.Id, targetCompany.Name))
        {
            await _glAccountRepository.InsertManyAsync(
                accounts.ConvertAll(acc => new GLAccount(
                    GuidGenerator.Create(),
                    acc.No,
                    acc.Name,
                    acc.AccountType,
                    acc.AccountCategory,
                    acc.IncomeBalance,
                    acc.Subcategory,
                    acc.DirectPosting
                )),
                autoSave: true
            );

            await _postingSetupRepository.InsertManyAsync(
                postingSetups.ConvertAll(ps => new GeneralPostingSetup(
                    GuidGenerator.Create(),
                    ps.GenBusPostingGroup,
                    ps.GenProdPostingGroup,
                    ps.SalesAccountNo,
                    ps.PurchAccountNo,
                    ps.COGSAccountNo,
                    ps.InventoryAdjmtAccountNo
                )),
                autoSave: true
            );
        }

        return targetCompany;
    }
}
