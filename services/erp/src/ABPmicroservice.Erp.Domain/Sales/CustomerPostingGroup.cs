using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Customer Posting Group. Mirrors Business Central table 92 "Customer Posting Group".
/// Maps Customer Posting Group Code -> Receivables G/L Account.
/// </summary>
public class CustomerPostingGroup : FullAuditedEntity<Guid>
{
    public string Code { get; private set; }
    public string Description { get; private set; }
    public string ReceivablesAccountNo { get; private set; }

    protected CustomerPostingGroup() { }

    public CustomerPostingGroup(Guid id, string code, string receivablesAccountNo, string description = null)
        : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxPostingGroupLength);
        ReceivablesAccountNo = Check.NotNullOrWhiteSpace(receivablesAccountNo, nameof(receivablesAccountNo), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }
}
