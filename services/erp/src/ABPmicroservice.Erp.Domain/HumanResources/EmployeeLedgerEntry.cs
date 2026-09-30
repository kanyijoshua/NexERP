using System;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>
/// Employee Ledger Entry. Mirrors Business Central table 5222: the expenses a company owes its
/// employees and the payouts that settle them. An expense claim is a credit (negative), a payout
/// a debit, always in LCY.
/// </summary>
public class EmployeeLedgerEntry : LedgerEntryBase, IApplicableLedgerEntry
{
    public Guid EmployeeId { get; private set; }
    public string EmployeeNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public DateTime DocumentDate { get; private set; }
    public GLEntryDocumentType DocumentType { get; private set; }
    public string DocumentNo { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public decimal RemainingAmount { get; internal set; }
    public bool Open { get; internal set; }
    public Guid DimensionSetId { get; private set; }

    public long ClosedByEntryNo { get; internal set; }

    public long TransactionNo { get; internal set; }
    public long RegisterNo { get; internal set; }

    public bool Reversed { get; internal set; }
    public long ReversedByEntryNo { get; internal set; }
    public long ReversedEntryNo { get; internal set; }

    // Employees are paid in LCY only, as in Business Central.
    string IApplicableLedgerEntry.CurrencyCode => null;
    decimal IApplicableLedgerEntry.RemainingAmountLcy => RemainingAmount;

    protected EmployeeLedgerEntry() { }

    public EmployeeLedgerEntry(
        Guid id,
        Guid employeeId,
        string employeeNo,
        DateTime postingDate,
        DateTime documentDate,
        GLEntryDocumentType documentType,
        string documentNo,
        string description,
        decimal amount,
        Guid dimensionSetId = default
    )
        : base(id)
    {
        EmployeeId = employeeId;
        EmployeeNo = Check.NotNullOrWhiteSpace(employeeNo, nameof(employeeNo), ErpDomainConsts.MaxNoLength);
        PostingDate = postingDate;
        DocumentDate = documentDate == default ? postingDate : documentDate;
        DocumentType = documentType;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Amount = amount;
        RemainingAmount = amount;
        Open = amount != 0m;
        DimensionSetId = dimensionSetId;
    }

    void IApplicableLedgerEntry.ReduceRemaining(decimal amount, decimal amountLcy, long closedByEntryNo)
    {
        RemainingAmount -= amount;
        if (RemainingAmount == 0m)
        {
            Open = false;
            ClosedByEntryNo = closedByEntryNo;
        }
    }
}
