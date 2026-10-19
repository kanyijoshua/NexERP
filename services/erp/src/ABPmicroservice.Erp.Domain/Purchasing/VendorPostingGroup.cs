using System;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Vendor Posting Group.
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

    /// <summary>Where a discount received from a vendor goes when it is a debit, such as on a credit memo.</summary>
    public string PaymentDiscDebitAccNo { get; private set; }

    /// <summary>Where the payment discounts received from vendors go (a credit).</summary>
    public string PaymentDiscCreditAccNo { get; private set; }

    public void SetPaymentDiscountAccounts(string paymentDiscDebitAccNo, string paymentDiscCreditAccNo)
    {
        PaymentDiscDebitAccNo = paymentDiscDebitAccNo.IsNullOrWhiteSpace() ? null : Check.Length(paymentDiscDebitAccNo.Trim(), nameof(paymentDiscDebitAccNo), ErpDomainConsts.MaxNoLength);
        PaymentDiscCreditAccNo = paymentDiscCreditAccNo.IsNullOrWhiteSpace() ? null : Check.Length(paymentDiscCreditAccNo.Trim(), nameof(paymentDiscCreditAccNo), ErpDomainConsts.MaxNoLength);
    }

    public void SetPayablesAccount(string payablesAccountNo)
    {
        PayablesAccountNo = Check.NotNullOrWhiteSpace(payablesAccountNo, nameof(payablesAccountNo), ErpDomainConsts.MaxNoLength);
    }
}
