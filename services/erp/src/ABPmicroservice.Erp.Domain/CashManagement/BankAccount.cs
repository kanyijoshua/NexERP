using System;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;

namespace ABPmicroservice.Erp.CashManagement;

/// <summary>
/// Bank Account Posting Group: the G/L account the balance of
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
/// Bank Account: an account the company holds at a bank. Its
/// ledger is the Bank Account Ledger Entry; its G/L balance lives on the posting group's account.
/// </summary>
public class BankAccount : CompanyAggregateRoot, IHasNo
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

    /// <summary>Name 2.</summary>
    public string Name2 { get; private set; }

    /// <summary>Address 2.</summary>
    public string Address2 { get; private set; }

    /// <summary>Post Code.</summary>
    public string PostCode { get; private set; }

    /// <summary>County.</summary>
    public string County { get; private set; }

    /// <summary>Country/Region Code.</summary>
    public string CountryRegionCode { get; private set; }

    /// <summary>E-Mail.</summary>
    public string Email { get; private set; }

    /// <summary>Fax No..</summary>
    public string FaxNo { get; private set; }

    /// <summary>Home Page.</summary>
    public string HomePage { get; private set; }

    /// <summary>Global Dimension 1 Code.</summary>
    public string GlobalDimension1Code { get; private set; }

    /// <summary>Global Dimension 2 Code.</summary>
    public string GlobalDimension2Code { get; private set; }

    /// <summary>Our Contact Code.</summary>
    public string OurContactCode { get; private set; }

    /// <summary>Min. Balance.</summary>
    public decimal MinBalance { get; private set; }

    /// <summary>Last Statement No..</summary>
    public string LastStatementNo { get; private set; }

    /// <summary>Balance Last Statement.</summary>
    public decimal BalanceLastStatement { get; private set; }

    /// <summary>Last Payment Statement No..</summary>
    public string LastPaymentStatementNo { get; private set; }

    /// <summary>Last Check No..</summary>
    public string LastCheckNo { get; private set; }

    /// <summary>Transit No..</summary>
    public string TransitNo { get; private set; }

    /// <summary>Bank Clearing Code.</summary>
    public string BankClearingCode { get; private set; }

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

    /// <summary>The card fields beyond those the posting routines read.</summary>
    public void SetAdditionalFields(
        string name2,
        string address2,
        string postCode,
        string county,
        string countryRegionCode,
        string email,
        string faxNo,
        string homePage,
        string globalDimension1Code,
        string globalDimension2Code,
        string ourContactCode,
        decimal minBalance,
        string lastStatementNo,
        decimal balanceLastStatement,
        string lastPaymentStatementNo,
        string lastCheckNo,
        string transitNo,
        string bankClearingCode
    )
    {
        Name2 = Check.Length(name2, nameof(name2), 50);
        Address2 = Check.Length(address2, nameof(address2), 50);
        PostCode = CodeTableEntity.NormalizeCode(Check.Length(postCode, nameof(postCode), 20));
        County = Check.Length(county, nameof(county), 30);
        CountryRegionCode = CodeTableEntity.NormalizeCode(Check.Length(countryRegionCode, nameof(countryRegionCode), 10));
        Email = Check.Length(email, nameof(email), 80);
        FaxNo = Check.Length(faxNo, nameof(faxNo), 30);
        HomePage = Check.Length(homePage, nameof(homePage), 80);
        GlobalDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension1Code, nameof(globalDimension1Code), 20));
        GlobalDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension2Code, nameof(globalDimension2Code), 20));
        OurContactCode = CodeTableEntity.NormalizeCode(Check.Length(ourContactCode, nameof(ourContactCode), 20));
        MinBalance = minBalance;
        LastStatementNo = CodeTableEntity.NormalizeCode(Check.Length(lastStatementNo, nameof(lastStatementNo), 20));
        BalanceLastStatement = balanceLastStatement;
        LastPaymentStatementNo = CodeTableEntity.NormalizeCode(Check.Length(lastPaymentStatementNo, nameof(lastPaymentStatementNo), 20));
        LastCheckNo = CodeTableEntity.NormalizeCode(Check.Length(lastCheckNo, nameof(lastCheckNo), 20));
        TransitNo = Check.Length(transitNo, nameof(transitNo), 20);
        BankClearingCode = Check.Length(bankClearingCode, nameof(bankClearingCode), 50);
    }
}

/// <summary>
/// Bank Account Ledger Entry: one row per amount posted to a
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
/// Payment Method: how a customer pays or a vendor is paid,
/// and the account a payment of that kind is balanced against (cash on hand, a bank account).
/// </summary>
public class PaymentMethod : CodeTableEntity
{
    public GenJournalAccountType? BalAccountType { get; private set; }
    public string BalAccountNo { get; private set; }

    /// <summary>Direct Debit.</summary>
    public bool DirectDebit { get; private set; }

    /// <summary>Direct Debit Pmt. Terms Code.</summary>
    public string DirectDebitPmtTermsCode { get; private set; }

    /// <summary>Pmt. Export Line Definition.</summary>
    public string PmtExportLineDefinition { get; private set; }

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

    /// <summary>The card fields beyond those the posting routines read.</summary>
    public void SetAdditionalFields(bool directDebit, string directDebitPmtTermsCode, string pmtExportLineDefinition)
    {
        DirectDebit = directDebit;
        DirectDebitPmtTermsCode = CodeTableEntity.NormalizeCode(Check.Length(directDebitPmtTermsCode, nameof(directDebitPmtTermsCode), 10));
        PmtExportLineDefinition = CodeTableEntity.NormalizeCode(Check.Length(pmtExportLineDefinition, nameof(pmtExportLineDefinition), 20));
    }
}
