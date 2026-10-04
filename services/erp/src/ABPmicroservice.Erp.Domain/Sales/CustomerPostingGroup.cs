using System;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Customer Posting Group.
/// Maps Customer Posting Group Code -> Receivables G/L Account.
/// </summary>
public class CustomerPostingGroup : PostingGroupBase
{
    public string ReceivablesAccountNo { get; private set; }

    protected CustomerPostingGroup() { }

    public CustomerPostingGroup(Guid id, string code, string receivablesAccountNo, string description = null)
        : base(id, code, description)
    {
        SetReceivablesAccount(receivablesAccountNo);
    }

    public void SetReceivablesAccount(string receivablesAccountNo)
    {
        ReceivablesAccountNo = Check.NotNullOrWhiteSpace(receivablesAccountNo, nameof(receivablesAccountNo), ErpDomainConsts.MaxNoLength);
    }
}
