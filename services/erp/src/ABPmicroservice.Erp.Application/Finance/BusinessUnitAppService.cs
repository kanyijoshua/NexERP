using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

/// <summary>Business units and consolidation; this company is the consolidation company.</summary>
public class BusinessUnitAppService : CodeTableAppServiceBase<BusinessUnit, BusinessUnitDto, CreateUpdateBusinessUnitDto>, IBusinessUnitAppService
{
    private ConsolidationManager Manager => LazyServiceProvider.LazyGetRequiredService<ConsolidationManager>();

    public BusinessUnitAppService(IRepository<BusinessUnit, Guid> repository)
        : base(repository, ErpPermissions.Consolidation.Default) { }

    protected override BusinessUnit NewEntity(Guid id, CreateUpdateBusinessUnitDto input) => new(id, input.Code, input.Description, input.SubsidiaryCompanyId);

    protected override async Task ApplyAsync(BusinessUnit entity, CreateUpdateBusinessUnitDto input)
    {
        if (input.SubsidiaryCompanyId == Guid.Empty || input.SubsidiaryCompanyId == LazyServiceProvider.LazyGetRequiredService<ICurrentCompany>().Id
            || !await LazyServiceProvider.LazyGetRequiredService<IRepository<Company, Guid>>().AnyAsync(c => c.Id == input.SubsidiaryCompanyId))
        {
            throw new BusinessException(ErpErrorCodes.Consolidation.BusinessUnitCompanyMissing).WithData("code", CodeTableEntity.NormalizeCode(input.Code) ?? "");
        }

        await CodeTableChecker.EnsureExistsAsync<Currency>(input.CurrencyCode);
        await LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>().EnsureGLAccountsExistAsync(input.ResidualAccountNo);
        entity.Set(input.SubsidiaryCompanyId, input.Consolidate, input.ConsolidationPct, input.CurrencyCode, input.ResidualAccountNo);
    }

    public async Task<List<ConsolidationLineDto>> GetPreviewAsync(Guid id, ConsolidationPeriodInput input) =>
        (await Manager.PreviewAsync(id, input.StartingDate, input.EndingDate)).Select(Map).ToList();

    [Authorize(ErpPermissions.Consolidation.Run)]
    public async Task<List<ConsolidationLineDto>> RunConsolidationAsync(RunConsolidationInput input) =>
        (await Manager.RunAsync(input.StartingDate, input.EndingDate, input.DocumentNo.Trim().ToUpperInvariant(), input.BusinessUnitId)).Select(Map).ToList();

    private static ConsolidationLineDto Map(ConsolidationLine line) => new()
    {
        BusinessUnitCode = line.BusinessUnitCode,
        SubsidiaryAccountNo = line.SubsidiaryAccountNo,
        ConsolidationAccountNo = line.ConsolidationAccountNo,
        TranslationMethod = line.TranslationMethod,
        SubsidiaryAmount = line.SubsidiaryAmount,
        Amount = line.Amount,
    };
}
