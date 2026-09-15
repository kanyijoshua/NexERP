using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Customer Ledger Entry. Mirrors Business Central table 21 "Cust. Ledger Entry".
/// Immutable subledger entry tracking accounts receivable balances per customer.
/// </summary>
public class CustomerLedgerEntry : Entity<Guid>
{
    public long EntryNo { get; internal set; }
    public Guid CustomerId { get; private set; }
    public string CustomerNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string DocumentType { get; private set; } // Invoice, Payment, Credit Memo
    public string DocumentNo { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public decimal RemainingAmount { get; internal set; }
    public DateTime DueDate { get; private set; }
    public bool Open { get; internal set; }
    public Guid DimensionSetId { get; private set; }

    protected CustomerLedgerEntry() { }

    public CustomerLedgerEntry(
        Guid id,
        Guid customerId,
        string customerNo,
        DateTime postingDate,
        string documentType,
        string documentNo,
        string description,
        decimal amount,
        DateTime dueDate,
        Guid dimensionSetId = default
    )
        : base(id)
    {
        CustomerId = customerId;
        CustomerNo = Check.NotNullOrWhiteSpace(customerNo, nameof(customerNo), ErpDomainConsts.MaxNoLength);
        PostingDate = postingDate;
        DocumentType = documentType;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Amount = amount;
        RemainingAmount = amount;
        DueDate = dueDate;
        Open = true;
        DimensionSetId = dimensionSetId;
    }

    public void ApplyPayment(decimal applyAmount)
    {
        RemainingAmount -= applyAmount;
        if (RemainingAmount == 0m)
        {
            Open = false;
        }
    }
}
