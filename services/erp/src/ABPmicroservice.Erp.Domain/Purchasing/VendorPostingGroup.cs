using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Vendor Posting Group. Mirrors Business Central table 93 "Vendor Posting Group".
/// Maps Vendor Posting Group Code -> Payables G/L Account.
/// </summary>
public class VendorPostingGroup : CompanyEntity
{
    public string Code { get; private set; }
    public string Description { get; private set; }
    public string PayablesAccountNo { get; private set; }

    protected VendorPostingGroup() { }

    public VendorPostingGroup(Guid id, string code, string payablesAccountNo, string description = null)
        : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxPostingGroupLength);
        PayablesAccountNo = Check.NotNullOrWhiteSpace(payablesAccountNo, nameof(payablesAccountNo), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }
}
