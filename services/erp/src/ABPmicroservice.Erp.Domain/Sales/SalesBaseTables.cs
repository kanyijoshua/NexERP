using System;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;

namespace ABPmicroservice.Erp.Sales;

/// <summary>Customer Bank Account.</summary>
public class CustomerBankAccount : CompanyEntity
{
    public string CustomerNo { get; private set; }
    public string Code { get; private set; }

    public string Name { get; private set; }
    public string Name2 { get; private set; }
    public string Address { get; private set; }
    public string Address2 { get; private set; }
    public string City { get; private set; }
    public string PostCode { get; private set; }
    public string Contact { get; private set; }
    public string PhoneNo { get; private set; }
    public string BankBranchNo { get; private set; }
    public string BankAccountNo { get; private set; }
    public string TransitNo { get; private set; }
    public string CurrencyCode { get; private set; }
    public string CountryRegionCode { get; private set; }
    public string County { get; private set; }
    public string FaxNo { get; private set; }
    public string Email { get; private set; }
    public string Iban { get; private set; }
    public string SwiftCode { get; private set; }
    public string BankClearingCode { get; private set; }
    public string BankClearingStandard { get; private set; }

    protected CustomerBankAccount() { }

    public CustomerBankAccount(Guid id, string customerNo, string code)
        : base(id)
    {
        SetKey(customerNo, code);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string customerNo, string code)
    {
        CustomerNo = Check.NotNullOrWhiteSpace(customerNo, nameof(customerNo), 20).Trim().ToUpperInvariant();
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), 20).Trim().ToUpperInvariant();
    }

    public void Set(
        string name,
        string name2,
        string address,
        string address2,
        string city,
        string postCode,
        string contact,
        string phoneNo,
        string bankBranchNo,
        string bankAccountNo,
        string transitNo,
        string currencyCode,
        string countryRegionCode,
        string county,
        string faxNo,
        string email,
        string iban,
        string swiftCode,
        string bankClearingCode,
        string bankClearingStandard
    )
    {
        Name = Check.Length(name, nameof(name), 100);
        Name2 = Check.Length(name2, nameof(name2), 50);
        Address = Check.Length(address, nameof(address), 100);
        Address2 = Check.Length(address2, nameof(address2), 50);
        City = Check.Length(city, nameof(city), 30);
        PostCode = CodeTableEntity.NormalizeCode(Check.Length(postCode, nameof(postCode), 20));
        Contact = Check.Length(contact, nameof(contact), 100);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), 30);
        BankBranchNo = Check.Length(bankBranchNo, nameof(bankBranchNo), 20);
        BankAccountNo = Check.Length(bankAccountNo, nameof(bankAccountNo), 30);
        TransitNo = Check.Length(transitNo, nameof(transitNo), 20);
        CurrencyCode = CodeTableEntity.NormalizeCode(Check.Length(currencyCode, nameof(currencyCode), 10));
        CountryRegionCode = CodeTableEntity.NormalizeCode(Check.Length(countryRegionCode, nameof(countryRegionCode), 10));
        County = Check.Length(county, nameof(county), 30);
        FaxNo = Check.Length(faxNo, nameof(faxNo), 30);
        Email = Check.Length(email, nameof(email), 80);
        Iban = CodeTableEntity.NormalizeCode(Check.Length(iban, nameof(iban), 50));
        SwiftCode = CodeTableEntity.NormalizeCode(Check.Length(swiftCode, nameof(swiftCode), 20));
        BankClearingCode = Check.Length(bankClearingCode, nameof(bankClearingCode), 50);
        BankClearingStandard = Check.Length(bankClearingStandard, nameof(bankClearingStandard), 50);
    }
}

/// <summary>Detailed Cust. Ledg. Entry.</summary>
public class DetailedCustLedgEntry : LedgerEntryBase
{
    public long CustLedgerEntryNo { get; internal set; }
    public DetailedCVLedgerEntryType EntryType { get; internal set; }
    public DateTime? PostingDate { get; internal set; }
    public GLEntryDocumentType DocumentType { get; internal set; }
    public string DocumentNo { get; internal set; }
    public decimal Amount { get; internal set; }
    public decimal AmountLcy { get; internal set; }
    public string CustomerNo { get; internal set; }
    public string CurrencyCode { get; internal set; }
    public string UserId { get; internal set; }
    public string SourceCode { get; internal set; }
    public long TransactionNo { get; internal set; }
    public string JournalBatchName { get; internal set; }
    public string ReasonCode { get; internal set; }
    public decimal DebitAmount { get; internal set; }
    public decimal CreditAmount { get; internal set; }
    public decimal DebitAmountLcy { get; internal set; }
    public decimal CreditAmountLcy { get; internal set; }
    public DateTime? InitialEntryDueDate { get; internal set; }
    public string InitialEntryGlobalDim1 { get; internal set; }
    public string InitialEntryGlobalDim2 { get; internal set; }
    public string GenBusPostingGroup { get; internal set; }
    public string GenProdPostingGroup { get; internal set; }
    public string VatBusPostingGroup { get; internal set; }
    public string VatProdPostingGroup { get; internal set; }
    public GLEntryDocumentType InitialDocumentType { get; internal set; }
    public long AppliedCustLedgerEntryNo { get; internal set; }
    public bool Unapplied { get; internal set; }
    public long UnappliedByEntryNo { get; internal set; }
    public decimal RemainingPmtDiscPossible { get; internal set; }
    public decimal MaxPaymentTolerance { get; internal set; }
    public int ApplicationNo { get; internal set; }
    public bool LedgerEntryAmount { get; internal set; }
    public string PostingGroup { get; internal set; }
    public int ExchRateAdjmtRegNo { get; internal set; }

    protected DetailedCustLedgEntry() { }

    public DetailedCustLedgEntry(Guid id)
        : base(id) { }
}
