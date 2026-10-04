using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Salesperson/Purchaser: the person responsible for a
/// customer or a vendor. The description is the person's name.
/// </summary>
public class SalespersonPurchaser : CodeTableEntity
{
    public string Email { get; private set; }
    public string PhoneNo { get; private set; }
    public string JobTitle { get; private set; }
    public decimal CommissionPercent { get; private set; }
    public bool Blocked { get; private set; }

    protected SalespersonPurchaser() { }

    public SalespersonPurchaser(Guid id, string code, string name)
        : base(id, code, name) { }

    public void SetContact(string email, string phoneNo, string jobTitle)
    {
        Email = Check.Length(email, nameof(email), ErpDomainConsts.MaxEmailLength);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
        JobTitle = Check.Length(jobTitle, nameof(jobTitle), ErpDomainConsts.MaxJobTitleLength);
    }

    public void SetCommission(decimal commissionPercent)
    {
        if (commissionPercent < 0 || commissionPercent > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", commissionPercent);
        }

        CommissionPercent = commissionPercent;
    }

    public void SetBlocked(bool blocked) => Blocked = blocked;
}
