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

    /// <summary>Where the payment discounts granted to customers go (a debit).</summary>
    public string PaymentDiscDebitAccNo { get; private set; }

    /// <summary>Where a granted discount goes when it is a credit, such as on a credit memo.</summary>
    public string PaymentDiscCreditAccNo { get; private set; }

    /// <summary>The income account for reminder and finance charge fees.</summary>
    public string AdditionalFeeAccountNo { get; private set; }

    /// <summary>The income account for interest charged on overdue amounts.</summary>
    public string InterestAccountNo { get; private set; }

    public void SetCollectionAccounts(string paymentDiscDebitAccNo, string paymentDiscCreditAccNo, string additionalFeeAccountNo, string interestAccountNo)
    {
        PaymentDiscDebitAccNo = Account(paymentDiscDebitAccNo, nameof(paymentDiscDebitAccNo));
        PaymentDiscCreditAccNo = Account(paymentDiscCreditAccNo, nameof(paymentDiscCreditAccNo));
        AdditionalFeeAccountNo = Account(additionalFeeAccountNo, nameof(additionalFeeAccountNo));
        InterestAccountNo = Account(interestAccountNo, nameof(interestAccountNo));
    }

    private static string Account(string no, string name) =>
        no.IsNullOrWhiteSpace() ? null : Check.Length(no.Trim(), name, ErpDomainConsts.MaxNoLength);

    public void SetReceivablesAccount(string receivablesAccountNo)
    {
        ReceivablesAccountNo = Check.NotNullOrWhiteSpace(receivablesAccountNo, nameof(receivablesAccountNo), ErpDomainConsts.MaxNoLength);
    }
}
