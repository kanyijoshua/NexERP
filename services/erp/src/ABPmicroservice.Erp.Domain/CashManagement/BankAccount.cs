using System;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;

namespace ABPmicroservice.Erp.CashManagement;

/// <summary>
/// Bank Account Posting Group. Mirrors Business Central table 277: the G/L account the balance of
/// the bank accounts in the group is carried on.
/// </summary>
public class BankAccountPostingGroup : PostingGroupBase
{
    public string GLAccountNo { get; private set; }

    protected BankAccountPostingGroup() { }

    public BankAccountPostingGroup(Guid id, string code, string glAccountNo, string description = null)
        : base(id, code, description)
    {
        SetGLAccount(glAccountNo);
    }

    public void SetGLAccount(string glAccountNo)
    {
        GLAccountNo = Check.NotNullOrWhiteSpace(glAccountNo, nameof(glAccountNo), ErpDomainConsts.MaxNoLength).Trim();
    }
}

/// <summary>
/// Bank Account. Mirrors Business Central table 270: an account the company holds at a bank. Its
/// ledger is the Bank Account Ledger Entry; its G/L balance lives on the posting group's account.
/// </summary>
public class BankAccount : CompanyAggregateRoot
{
    public string No { get; private set; }
    public string Name { get; private set; }
    public string BankAccountNo { get; private set; }
    public string BankBranchNo { get; private set; }
    public string Iban { get; private set; }
    public string SwiftCode { get; private set; }
    public string CurrencyCode { get; private set; }
    public string BankAccPostingGroup { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }
    public string PhoneNo { get; private set; }
    public string Contact { get; private set; }

    /// <summary>Sum of the ledger entries in the account's currency, kept by posting.</summary>
    public decimal Balance { get; internal set; }

    /// <summary>The balance in LCY: what the G/L account carries for this bank account.</summary>
    public decimal BalanceLcy { get; internal set; }

    public bool Blocked { get; private set; }

    protected BankAccount() { }

    public BankAccount(Guid id, string no, string name)
        : base(id)
    {
        SetNo(no);
        SetName(name);
    }

    public void SetNo(string no) =>
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim();

    public void SetName(string name) =>
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);

    public void SetBankDetails(string bankAccountNo, string bankBranchNo, string iban, string swiftCode)
    {
        BankAccountNo = Check.Length(bankAccountNo, nameof(bankAccountNo), ErpDomainConsts.MaxBankAccountNoLength);
        BankBranchNo = Check.Length(bankBranchNo, nameof(bankBranchNo), ErpDomainConsts.MaxCodeLength);
        Iban = Check.Length(iban?.Replace(" ", "").ToUpperInvariant(), nameof(iban), ErpDomainConsts.MaxIbanLength);
        SwiftCode = Check.Length(swiftCode?.Trim().ToUpperInvariant(), nameof(swiftCode), ErpDomainConsts.MaxSwiftCodeLength);
    }

    public void SetAddress(string address, string city, string contact, string phoneNo)
    {
        Address = Check.Length(address, nameof(address), ErpDomainConsts.MaxAddressLength);
        City = Check.Length(city, nameof(city), ErpDomainConsts.MaxCityLength);
        Contact = Check.Length(contact, nameof(contact), ErpDomainConsts.MaxNameLength);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
    }

    public void SetPosting(string bankAccPostingGroup, string currencyCode)
    {
        BankAccPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(bankAccPostingGroup, nameof(bankAccPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
        CurrencyCode = CodeTableEntity.NormalizeCode(Check.Length(currencyCode, nameof(currencyCode), ErpDomainConsts.MaxCurrencyCodeLength));
    }

    public void Block() => Blocked = true;

    public void Unblock() => Blocked = false;

    internal void ApplyBalance(decimal amount, decimal amountLcy)
    {
        Balance += amount;
        BalanceLcy += amountLcy;
    }
}

/// <summary>
/// Bank Account Ledger Entry. Mirrors Business Central table 271: one row per amount posted to a
/// bank account, positive for money in.
/// </summary>
public class BankAccountLedgerEntry : LedgerEntryBase
{
    public Guid BankAccountId { get; private set; }
    public string BankAccountNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public GLEntryDocumentType DocumentType { get; private set; }
    public string DocumentNo { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }

    /// <summary>The amount in LCY. An exchange rate adjustment writes an entry with only this.</summary>
    public decimal AmountLcy { get; private set; }

    public string CurrencyCode { get; private set; }
    public decimal RemainingAmount { get; internal set; }
    public bool Open { get; internal set; }
    public Guid DimensionSetId { get; private set; }

    public long TransactionNo { get; internal set; }
    public long RegisterNo { get; internal set; }

    public bool Reversed { get; internal set; }
    public long ReversedByEntryNo { get; internal set; }
    public long ReversedEntryNo { get; internal set; }

    protected BankAccountLedgerEntry() { }

    public BankAccountLedgerEntry(
        Guid id,
        Guid bankAccountId,
        string bankAccountNo,
        DateTime postingDate,
        GLEntryDocumentType documentType,
        string documentNo,
        string description,
        decimal amount,
        Guid dimensionSetId = default,
        string currencyCode = null,
        decimal? amountLcy = null
    )
        : base(id)
    {
        BankAccountId = bankAccountId;
        BankAccountNo = Check.NotNullOrWhiteSpace(bankAccountNo, nameof(bankAccountNo), ErpDomainConsts.MaxNoLength);
        PostingDate = postingDate;
        DocumentType = documentType;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Amount = amount;
        AmountLcy = amountLcy ?? amount;
        CurrencyCode = currencyCode;
        RemainingAmount = amount;
        Open = true;
        DimensionSetId = dimensionSetId;
    }
}

/// <summary>
/// Payment Method. Mirrors Business Central table 289: how a customer pays or a vendor is paid,
/// and the account a payment of that kind is balanced against (cash on hand, a bank account).
/// </summary>
public class PaymentMethod : CodeTableEntity
{
    public GenJournalAccountType? BalAccountType { get; private set; }
    public string BalAccountNo { get; private set; }

    protected PaymentMethod() { }

    public PaymentMethod(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetBalancingAccount(GenJournalAccountType? balAccountType, string balAccountNo)
    {
        if (balAccountNo.IsNullOrWhiteSpace())
        {
            BalAccountType = null;
            BalAccountNo = null;
            return;
        }

        // Customers and vendors are the other side of a payment, never its balancing account.
        if (balAccountType is not (GenJournalAccountType.GLAccount or GenJournalAccountType.BankAccount))
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.InvalidBalAccountType);
        }

        BalAccountType = balAccountType;
        BalAccountNo = Check.Length(balAccountNo.Trim(), nameof(balAccountNo), ErpDomainConsts.MaxNoLength);
    }
}
