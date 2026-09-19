using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Companies;

public class CompanyDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool EvaluationCompany { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateCompanyDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string DisplayName { get; set; }

    public bool EvaluationCompany { get; set; }
}

public class CopyCompanyInput
{
    public Guid SourceCompanyId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string NewCompanyName { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string NewDisplayName { get; set; }
}

public interface ICompanyAppService : IApplicationService
{
    /// <summary>The current tenant's companies. Not company-scoped: it feeds the company switcher.</summary>
    Task<ListResultDto<CompanyDto>> GetListAsync();

    Task<CompanyDto> CreateAsync(CreateCompanyDto input);

    /// <summary>Copies setup data into a new company. Routed as POST /api/erp/company/copy.</summary>
    Task<CompanyDto> CopyAsync(CopyCompanyInput input);

    /// <summary>Routed as POST /api/erp/company/{id}/set-as-default.</summary>
    Task SetAsDefaultAsync(Guid id);
}
