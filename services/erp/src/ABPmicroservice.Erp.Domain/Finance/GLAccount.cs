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

    public bool Blocked { get; private set; }

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
        bool directPosting = true
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
    }

    public void SetNo(string no)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength);
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
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

    public void Block() => Blocked = true;

    public void Unblock() => Blocked = false;

    internal void ApplyEntry(decimal amount)
    {
        NetChange += amount;
        Balance += amount;
    }
}
