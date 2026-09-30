using System;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Vendor Posting Group. Mirrors Business Central table 93 "Vendor Posting Group".
/// Maps Vendor Posting Group Code -> Payables G/L Account.
/// </summary>
public class VendorPostingGroup : PostingGroupBase
{
    public string PayablesAccountNo { get; private set; }

    protected VendorPostingGroup() { }

    public VendorPostingGroup(Guid id, string code, string payablesAccountNo, string description = null)
        : base(id, code, description)
    {
        SetPayablesAccount(payablesAccountNo);
    }

    public void SetPayablesAccount(string payablesAccountNo)
    {
        PayablesAccountNo = Check.NotNullOrWhiteSpace(payablesAccountNo, nameof(payablesAccountNo), ErpDomainConsts.MaxNoLength);
    }
}
