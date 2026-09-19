using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Vendor Ledger Entry. Mirrors Business Central table 25 "Vendor Ledger Entry".
/// Immutable subledger entry tracking accounts payable balances per vendor.
/// </summary>
public class VendorLedgerEntry : LedgerEntryBase
{
    public Guid VendorId { get; private set; }
    public string VendorNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string DocumentType { get; private set; } // Invoice, Payment, Credit Memo
    public string DocumentNo { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public decimal RemainingAmount { get; internal set; }
    public DateTime DueDate { get; private set; }
    public bool Open { get; internal set; }
    public Guid DimensionSetId { get; private set; }

    protected VendorLedgerEntry() { }

    public VendorLedgerEntry(
        Guid id,
        Guid vendorId,
        string vendorNo,
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
        VendorId = vendorId;
        VendorNo = Check.NotNullOrWhiteSpace(vendorNo, nameof(vendorNo), ErpDomainConsts.MaxNoLength);
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
