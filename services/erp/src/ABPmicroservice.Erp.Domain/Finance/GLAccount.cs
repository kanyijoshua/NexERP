using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// G/L Account (Chart of Accounts). Mirrors Business Central table 15 "G/L Account".
/// </summary>
public class GLAccount : CompanyAggregateRoot
{
    /// <summary>Business key. Mirrors BC field "No.".</summary>
    public string No { get; private set; }

    public string Name { get; private set; }

    public GLAccountType AccountType { get; private set; }

    public GLAccountCategory AccountCategory { get; private set; }

    public string Subcategory { get; private set; }

    public IncomeBalanceType IncomeBalance { get; private set; }

    /// <summary>Whether direct posting is allowed to this account.</summary>
    public bool DirectPosting { get; private set; }

    /// <summary>The VAT rate class of sales and purchase lines posted to this account.</summary>
    public string VatProdPostingGroup { get; private set; }

    /// <summary>Whether journal lines on this account are purchases or sales, so which VAT they carry.</summary>
    public GeneralPostingType GenPostingType { get; private set; }

    /// <summary>The VAT business group journal lines on this account default to.</summary>
    public string VatBusPostingGroup { get; private set; }

    public bool Blocked { get; private set; }

    /// <summary>Search Name. Mirrors BC field 3 "Search Name".</summary>
    public string SearchName { get; private set; }

    /// <summary>Debit or credit constraint. Mirrors BC field 10 "Debit/Credit".</summary>
    public GLAccountDebitCredit DebitCredit { get; private set; }

    /// <summary>Reconciliation account indicator. Mirrors BC field 16 "Reconciliation Account".</summary>
    public bool ReconciliationAccount { get; private set; }

    /// <summary>Account range or formula for Total accounts. Mirrors BC field 34 "Totaling".</summary>
    public string Totaling { get; private set; }

    /// <summary>Default Gen. Business Posting Group. Mirrors BC field 44 "Gen. Bus. Posting Group".</summary>
    public string GenBusPostingGroup { get; private set; }

    /// <summary>Default Gen. Product Posting Group. Mirrors BC field 45 "Gen. Prod. Posting Group".</summary>
    public string GenProdPostingGroup { get; private set; }

    /// <summary>Automatic extended text. Mirrors BC field 49 "Automatic Ext. Texts".</summary>
    public bool AutomaticExtTexts { get; private set; }

    /// <summary>Tax Area Code. Mirrors BC field 54 "Tax Area Code".</summary>
    public string TaxAreaCode { get; private set; }

    /// <summary>Tax Liable. Mirrors BC field 55 "Tax Liable".</summary>
    public bool TaxLiable { get; private set; }

    /// <summary>Tax Group Code. Mirrors BC field 56 "Tax Group Code".</summary>
    public string TaxGroupCode { get; private set; }

    /// <summary>Consolidation translation method. Mirrors BC field 39 "Consol. Translation Method".</summary>
    public ConsolidationTranslationMethod ConsolTranslationMethod { get; private set; }

    /// <summary>Consolidation debit account. Mirrors BC field 40 "Consol. Debit Acc.".</summary>
    public string ConsolDebitAcc { get; private set; }

    /// <summary>Consolidation credit account. Mirrors BC field 41 "Consol. Credit Acc.".</summary>
    public string ConsolCreditAcc { get; private set; }

    /// <summary>Cost Type No. Mirrors BC field 1100 "Cost Type No.".</summary>
    public string CostTypeNo { get; private set; }

    /// <summary>Default Deferral Template Code. Mirrors BC field 1700 "Default Deferral Template Code".</summary>
    public string DefaultDeferralTemplateCode { get; private set; }

    /// <summary>Omit default description in journal. Mirrors BC field 70 "Omit Default Descr. in Jnl.".</summary>
    public bool OmitDefaultDescrInJnl { get; private set; }

    /// <summary>Running net change (denormalized, updated by posting).</summary>
    public decimal NetChange { get; internal set; }

    /// <summary>Running balance (denormalized, updated by posting).</summary>
    public decimal Balance { get; internal set; }

    protected GLAccount() { }

    public GLAccount(
        Guid id,
        string no,
        string name,
        GLAccountType accountType,
        GLAccountCategory accountCategory,
        IncomeBalanceType incomeBalance,
        string subcategory = null,
        bool directPosting = true,
        string searchName = null,
        GLAccountDebitCredit debitCredit = GLAccountDebitCredit.Both,
        bool reconciliationAccount = false,
        string totaling = null,
        string genBusPostingGroup = null,
        string genProdPostingGroup = null,
        bool automaticExtTexts = false,
        string taxAreaCode = null,
        bool taxLiable = false,
        string taxGroupCode = null,
        ConsolidationTranslationMethod consolTranslationMethod = ConsolidationTranslationMethod.Average,
        string consolDebitAcc = null,
        string consolCreditAcc = null,
        string costTypeNo = null,
        string defaultDeferralTemplateCode = null,
        bool omitDefaultDescrInJnl = false
    )
        : base(id)
    {
        SetNo(no);
        SetName(name);
        AccountType = accountType;
        AccountCategory = accountCategory;
        IncomeBalance = incomeBalance;
        Subcategory = subcategory;
        DirectPosting = directPosting;
        Blocked = false;
        NetChange = 0m;
        Balance = 0m;

        SetSearchName(searchName ?? name);
        DebitCredit = debitCredit;
        ReconciliationAccount = reconciliationAccount;
        SetTotaling(totaling);
        SetGeneralPostingGroups(genBusPostingGroup, genProdPostingGroup);
        AutomaticExtTexts = automaticExtTexts;
        SetTax(taxAreaCode, taxLiable, taxGroupCode);
        SetConsolidation(consolTranslationMethod, consolDebitAcc, consolCreditAcc);
        SetCostTypeNo(costTypeNo);
        SetDefaultDeferralTemplateCode(defaultDeferralTemplateCode);
        OmitDefaultDescrInJnl = omitDefaultDescrInJnl;
    }

    public void SetNo(string no)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength);
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
    }

    public void SetSearchName(string searchName)
    {
        SearchName = searchName.IsNullOrWhiteSpace()
            ? Name
            : Check.Length(searchName.Trim(), nameof(searchName), ErpDomainConsts.MaxNameLength);
    }

    public void SetAccountType(GLAccountType accountType) => AccountType = accountType;

    public void SetAccountCategory(GLAccountCategory accountCategory) =>
        AccountCategory = accountCategory;

    public void SetIncomeBalance(IncomeBalanceType incomeBalance) => IncomeBalance = incomeBalance;

    public void SetSubcategory(string subcategory) =>
        Subcategory = Check.Length(
            subcategory,
            nameof(subcategory),
            ErpDomainConsts.MaxNameLength
        );

    public void SetDirectPosting(bool directPosting) => DirectPosting = directPosting;

    public void SetDebitCredit(GLAccountDebitCredit debitCredit) => DebitCredit = debitCredit;

    public void SetReconciliationAccount(bool reconciliationAccount) => ReconciliationAccount = reconciliationAccount;

    public void SetTotaling(string totaling) =>
        Totaling = Check.Length(totaling?.Trim(), nameof(totaling), ErpDomainConsts.MaxTotalingLength);

    public void SetGeneralPostingGroups(string genBusPostingGroup, string genProdPostingGroup)
    {
        GenBusPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(genBusPostingGroup, nameof(genBusPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
        GenProdPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(genProdPostingGroup, nameof(genProdPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
    }

    public void SetAutomaticExtTexts(bool automaticExtTexts) => AutomaticExtTexts = automaticExtTexts;

    public void SetTax(string taxAreaCode, bool taxLiable, string taxGroupCode)
    {
        TaxAreaCode = CodeTableEntity.NormalizeCode(Check.Length(taxAreaCode, nameof(taxAreaCode), ErpDomainConsts.MaxTaxAreaCodeLength));
        TaxLiable = taxLiable;
        TaxGroupCode = CodeTableEntity.NormalizeCode(Check.Length(taxGroupCode, nameof(taxGroupCode), ErpDomainConsts.MaxTaxGroupCodeLength));
    }

    public void SetConsolidation(ConsolidationTranslationMethod translationMethod, string consolDebitAcc, string consolCreditAcc)
    {
        ConsolTranslationMethod = translationMethod;
        ConsolDebitAcc = CodeTableEntity.NormalizeCode(Check.Length(consolDebitAcc, nameof(consolDebitAcc), ErpDomainConsts.MaxNoLength));
        ConsolCreditAcc = CodeTableEntity.NormalizeCode(Check.Length(consolCreditAcc, nameof(consolCreditAcc), ErpDomainConsts.MaxNoLength));
    }

    public void SetCostTypeNo(string costTypeNo) =>
        CostTypeNo = CodeTableEntity.NormalizeCode(Check.Length(costTypeNo, nameof(costTypeNo), ErpDomainConsts.MaxCostTypeLength));

    public void SetDefaultDeferralTemplateCode(string templateCode) =>
        DefaultDeferralTemplateCode = CodeTableEntity.NormalizeCode(Check.Length(templateCode, nameof(templateCode), ErpDomainConsts.MaxDeferralTemplateCodeLength));

    public void SetOmitDefaultDescrInJnl(bool omit) => OmitDefaultDescrInJnl = omit;

    public void Block() => Blocked = true;

    public void Unblock() => Blocked = false;

    internal void ApplyEntry(decimal amount)
    {
        NetChange += amount;
        Balance += amount;
    }

    public void SetVatProdPostingGroup(string vatProdPostingGroup) =>
        VatProdPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(vatProdPostingGroup, nameof(vatProdPostingGroup), ErpDomainConsts.MaxPostingGroupLength));

    /// <summary>The VAT defaults a journal line takes when this account is chosen (BC's posting tab).</summary>
    public void SetJournalVatDefaults(GeneralPostingType genPostingType, string vatBusPostingGroup)
    {
        GenPostingType = genPostingType;
        VatBusPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(vatBusPostingGroup, nameof(vatBusPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
    }
}
