using System;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Finance;

public class GLAccountDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Name { get; set; }
    public GLAccountType AccountType { get; set; }
    public GLAccountCategory AccountCategory { get; set; }
    public string Subcategory { get; set; }
    public IncomeBalanceType IncomeBalance { get; set; }
    public bool DirectPosting { get; set; }
    public bool Blocked { get; set; }
    public decimal NetChange { get; set; }
    public decimal Balance { get; set; }
}

public class CreateUpdateGLAccountDto
{
    public string No { get; set; }
    public string Name { get; set; }
    public GLAccountType AccountType { get; set; }
    public GLAccountCategory AccountCategory { get; set; }
    public string Subcategory { get; set; }
    public IncomeBalanceType IncomeBalance { get; set; }
    public bool DirectPosting { get; set; }
}

public class GetGLAccountListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
    public GLAccountCategory? AccountCategory { get; set; }
    public GLAccountType? AccountType { get; set; }
}
