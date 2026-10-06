using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>A bank pensioners and other schemes are paid through. Its description is the bank's name.</summary>
public class PensionBank : CodeTableEntity
{
    public string SwiftCode { get; private set; }

    protected PensionBank() { }

    public PensionBank(Guid id, string code, string name)
        : base(id, code, name) { }

    public void Set(string swiftCode)
    {
        SwiftCode = Check.Length(swiftCode?.Trim(), nameof(swiftCode), ErpDomainConsts.MaxSwiftCodeLength);
    }
}

/// <summary>A branch of a bank, keyed by the bank and the branch code.</summary>
public class PensionBankBranch : CompanyEntity
{
    public string BankCode { get; private set; }
    public string BranchCode { get; private set; }
    public string Name { get; private set; }
    public string SwiftCode { get; private set; }

    protected PensionBankBranch() { }

    public PensionBankBranch(Guid id, string bankCode, string branchCode, string name)
        : base(id)
    {
        SetKey(bankCode, branchCode);
        Set(name, null);
    }

    public void SetKey(string bankCode, string branchCode)
    {
        BankCode = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(bankCode, nameof(bankCode), ErpDomainConsts.MaxCodeLength));
        BranchCode = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(branchCode, nameof(branchCode), ErpDomainConsts.MaxCodeLength));
    }

    public void Set(string name, string swiftCode)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength).Trim();
        SwiftCode = Check.Length(swiftCode?.Trim(), nameof(swiftCode), ErpDomainConsts.MaxSwiftCodeLength);
    }
}

/// <summary>How a pensioner is paid: through a bank, by mobile money, by cheque or in cash.</summary>
public class PensionerPayMode : CodeTableEntity
{
    public PensionerPaymentType PaymentType { get; private set; }

    protected PensionerPayMode() { }

    public PensionerPayMode(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(PensionerPaymentType paymentType) => PaymentType = paymentType;
}

/// <summary>Why a pension may be stopped.</summary>
public class PensionerSuspensionReason : CodeTableEntity
{
    /// <summary>
    /// The pension stopped for want of a life certificate: suspending overdue pensioners uses this
    /// reason, and receiving a certificate pays the pension again.
    /// </summary>
    public bool LifeCertificate { get; private set; }

    protected PensionerSuspensionReason() { }

    public PensionerSuspensionReason(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(bool lifeCertificate) => LifeCertificate = lifeCertificate;
}

/// <summary>Why pensions are revised, e.g. a yearly cost of living increase.</summary>
public class PensionRevisionReason : CodeTableEntity
{
    protected PensionRevisionReason() { }

    public PensionRevisionReason(Guid id, string code, string description)
        : base(id, code, description) { }
}

/// <summary>Another retirement benefits scheme that members transfer in from or out to. Its description is its name.</summary>
public class OtherPensionScheme : CodeTableEntity
{
    public string RegulatorReferenceNo { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }
    public string ContactName { get; private set; }
    public string PhoneNo { get; private set; }
    public string Email { get; private set; }
    public string BankCode { get; private set; }
    public string BankBranchCode { get; private set; }
    public string BankAccountNo { get; private set; }

    protected OtherPensionScheme() { }

    public OtherPensionScheme(Guid id, string code, string name)
        : base(id, code, name) { }

    public void Set(string regulatorReferenceNo, string address, string city, string contactName, string phoneNo, string email)
    {
        RegulatorReferenceNo = Check.Length(regulatorReferenceNo, nameof(regulatorReferenceNo), ErpDomainConsts.MaxExternalDocumentNoLength);
        Address = Check.Length(address, nameof(address), ErpDomainConsts.MaxAddressLength);
        City = Check.Length(city, nameof(city), ErpDomainConsts.MaxCityLength);
        ContactName = Check.Length(contactName, nameof(contactName), ErpDomainConsts.MaxContactLength);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
        Email = Check.Length(email, nameof(email), ErpDomainConsts.MaxEmailLength);
    }

    public void SetBank(string bankCode, string bankBranchCode, string bankAccountNo)
    {
        BankCode = NormalizeCode(Check.Length(bankCode, nameof(bankCode), ErpDomainConsts.MaxCodeLength));
        BankBranchCode = NormalizeCode(Check.Length(bankBranchCode, nameof(bankBranchCode), ErpDomainConsts.MaxCodeLength));
        BankAccountNo = Check.Length(bankAccountNo, nameof(bankAccountNo), ErpDomainConsts.MaxBankAccountNoLength);
    }
}

/// <summary>
/// An earning paid with pensions (e.g. a medical allowance) or a deduction taken off them (e.g. a
/// loan repayment or a union fee). Pensioners are given the items that apply to them.
/// </summary>
public class PensionerPayItem : CodeTableEntity
{
    public PensionerPayItemType ItemType { get; private set; }
    public PensionerPayItemCalculation Calculation { get; private set; }

    /// <summary>The flat amount, for a pensioner given the item without an amount of their own.</summary>
    public decimal Amount { get; private set; }

    /// <summary>The percentage of the month's pension, for the percentage calculation.</summary>
    public decimal Pct { get; private set; }

    /// <summary>An earning that is taxed with the pension; a deduction that is taken off the pension before it is taxed.</summary>
    public bool Taxable { get; private set; }

    /// <summary>
    /// The G/L account the item posts to: debited with an earning (blank uses the Pensions Paid
    /// account), credited with a deduction (required).
    /// </summary>
    public string AccountNo { get; private set; }

    /// <summary>A blocked item is not paid or taken on any payroll.</summary>
    public bool Blocked { get; private set; }

    protected PensionerPayItem() { }

    public PensionerPayItem(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(PensionerPayItemType itemType, PensionerPayItemCalculation calculation, decimal amount, decimal pct, bool taxable, string accountNo, bool blocked)
    {
        if (amount < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Amount");
        }

        if (pct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", pct);
        }

        ItemType = itemType;
        Calculation = calculation;
        Amount = amount;
        Pct = pct;
        Taxable = taxable;
        AccountNo = accountNo.IsNullOrWhiteSpace() ? null : Check.Length(accountNo.Trim(), nameof(accountNo), ErpDomainConsts.MaxNoLength);
        Blocked = blocked;
    }

    /// <summary>The item's amount for a month: the pensioner's own amount if they have one, else the item's.</summary>
    public decimal AmountFor(decimal monthlyPension, decimal pensionerAmount)
    {
        var amount = Calculation == PensionerPayItemCalculation.PercentOfPension
            ? monthlyPension * (pensionerAmount > 0m ? pensionerAmount : Pct) / 100m
            : pensionerAmount > 0m ? pensionerAmount : Amount;

        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}

/// <summary>
/// A pay item a pensioner is given, from a month and possibly until one. Every payroll for a month
/// within the dates pays or takes it.
/// </summary>
public class PensionerPayItemAssignment : CompanyEntity
{
    public string PensionerNo { get; private set; }
    public string PayItemCode { get; private set; }

    /// <summary>
    /// The pensioner's own amount (a percentage for a percentage item); zero takes the item's.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>The first month the item is paid or taken, and the last; no end date keeps it on.</summary>
    public DateTime StartDate { get; private set; }

    public DateTime? EndDate { get; private set; }

    public string Comment { get; private set; }

    protected PensionerPayItemAssignment() { }

    public PensionerPayItemAssignment(Guid id, string pensionerNo, string payItemCode, DateTime startDate)
        : base(id)
    {
        Set(pensionerNo, payItemCode, 0m, startDate, null, null);
    }

    public void Set(string pensionerNo, string payItemCode, decimal amount, DateTime startDate, DateTime? endDate, string comment)
    {
        if (amount < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Amount");
        }

        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
        {
            throw new BusinessException(ErpErrorCodes.Reports.PeriodReversed);
        }

        PensionerNo = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(pensionerNo, nameof(pensionerNo), ErpDomainConsts.MaxNoLength));
        PayItemCode = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(payItemCode, nameof(payItemCode), ErpDomainConsts.MaxCodeLength));
        Amount = amount;
        StartDate = new DateTime(startDate.Year, startDate.Month, 1);
        EndDate = endDate?.Date;
        Comment = Check.Length(comment, nameof(comment), ErpDomainConsts.MaxDescriptionLength);
    }

    /// <summary>Whether the item applies to the month starting on <paramref name="period"/>.</summary>
    public bool AppliesTo(DateTime period) => StartDate <= period && (!EndDate.HasValue || EndDate.Value >= period);
}

/// <summary>
/// An earning or deduction on a pensioner's payroll line, as worked out when the line was: the
/// item's description, tax treatment and account are kept, so that later changes to the item do
/// not change a payroll already prepared.
/// </summary>
public class PensionPayrollLineItem : CompanyEntity
{
    public string DocumentNo { get; private set; }
    public int LineNo { get; private set; }
    public string PensionerNo { get; private set; }
    public string PayItemCode { get; private set; }
    public string Description { get; private set; }
    public PensionerPayItemType ItemType { get; private set; }
    public bool Taxable { get; private set; }
    public string AccountNo { get; private set; }
    public decimal Amount { get; private set; }

    protected PensionPayrollLineItem() { }

    public PensionPayrollLineItem(Guid id, PensionPayrollLine line, PensionerPayItem item, decimal amount)
        : base(id)
    {
        DocumentNo = line.DocumentNo;
        LineNo = line.LineNo;
        PensionerNo = line.PensionerNo;
        PayItemCode = item.Code;
        Description = item.Description;
        ItemType = item.ItemType;
        Taxable = item.Taxable;
        AccountNo = item.AccountNo;
        Amount = amount;
    }

    public bool IsEarning => ItemType == PensionerPayItemType.Earning;
}

/// <summary>A document an exit for a reason needs before it can be approved, e.g. a death certificate.</summary>
public class ExitReasonDocument : CompanyEntity
{
    public string ExitReasonCode { get; private set; }
    public int LineNo { get; private set; }
    public string DocumentName { get; private set; }

    /// <summary>The exit cannot be approved until a mandatory document has been received.</summary>
    public bool Mandatory { get; private set; }

    protected ExitReasonDocument() { }

    public ExitReasonDocument(Guid id, string exitReasonCode, int lineNo, string documentName)
        : base(id)
    {
        ExitReasonCode = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(exitReasonCode, nameof(exitReasonCode), ErpDomainConsts.MaxCodeLength));
        LineNo = lineNo;
        Set(documentName, true);
    }

    public void Set(string documentName, bool mandatory)
    {
        DocumentName = Check.NotNullOrWhiteSpace(documentName, nameof(documentName), ErpDomainConsts.MaxNameLength).Trim();
        Mandatory = mandatory;
    }
}

/// <summary>A document an exit needs, and whether it has been received.</summary>
public class MemberExitDocument : CompanyEntity
{
    public string ExitNo { get; private set; }
    public int LineNo { get; private set; }
    public string DocumentName { get; private set; }
    public bool Mandatory { get; private set; }
    public bool Received { get; private set; }
    public DateTime? ReceivedDate { get; private set; }
    public string Remarks { get; private set; }

    protected MemberExitDocument() { }

    public MemberExitDocument(Guid id, string exitNo, int lineNo, string documentName, bool mandatory)
        : base(id)
    {
        ExitNo = Check.NotNullOrWhiteSpace(exitNo, nameof(exitNo), ErpDomainConsts.MaxDocumentNoLength);
        LineNo = lineNo;
        Set(documentName, mandatory, false, null, null);
    }

    /// <summary>A document marked received without a date is received today.</summary>
    public void Set(string documentName, bool mandatory, bool received, DateTime? receivedDate, string remarks)
    {
        DocumentName = Check.NotNullOrWhiteSpace(documentName, nameof(documentName), ErpDomainConsts.MaxNameLength).Trim();
        Mandatory = mandatory;
        Received = received;
        ReceivedDate = received ? (receivedDate ?? DateTime.Today).Date : null;
        Remarks = Check.Length(remarks, nameof(remarks), ErpDomainConsts.MaxDescriptionLength);
    }
}

/// <summary>
/// A defined benefit scheme's factor for an age: an early or late retirement factor that multiplies
/// the pension, or the commutation factor a lump sum is worked out by. Factors differ for women
/// and men, who live different lengths of time on average.
/// </summary>
public class PensionAgeFactor : CompanyEntity
{
    public string SchemeCode { get; private set; }
    public PensionFactorType FactorType { get; private set; }

    /// <summary>The age in whole years the factor is for; it holds until the next age in the table.</summary>
    public int Age { get; private set; }

    public decimal MaleFactor { get; private set; }
    public decimal FemaleFactor { get; private set; }

    protected PensionAgeFactor() { }

    public PensionAgeFactor(Guid id, string schemeCode, PensionFactorType factorType, int age)
        : base(id)
    {
        SetKey(schemeCode, factorType, age);
    }

    public void SetKey(string schemeCode, PensionFactorType factorType, int age)
    {
        if (age is < 0 or > 120)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Age");
        }

        SchemeCode = PensionSponsor.SchemeOf(schemeCode);
        FactorType = factorType;
        Age = age;
    }

    public void Set(decimal maleFactor, decimal femaleFactor)
    {
        if (maleFactor < 0 || femaleFactor < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Factor");
        }

        MaleFactor = maleFactor;
        FemaleFactor = femaleFactor;
    }

    /// <summary>The factor for a member of the gender given; a member whose gender is not known takes the male factor.</summary>
    public decimal For(MemberGender gender) => gender == MemberGender.Female ? FemaleFactor : MaleFactor;
}

/// <summary>A pay item worked out for a pensioner's month.</summary>
public record PensionerPayItemAmount(PensionerPayItem Item, decimal Amount);

/// <summary>Works out the earnings and deductions of a pensioner for a month.</summary>
public class PensionerPayItemCalculator : DomainService
{
    private readonly IRepository<PensionerPayItem, Guid> _items;
    private readonly IRepository<PensionerPayItemAssignment, Guid> _assignments;

    public PensionerPayItemCalculator(IRepository<PensionerPayItem, Guid> items, IRepository<PensionerPayItemAssignment, Guid> assignments)
    {
        _items = items;
        _assignments = assignments;
    }

    /// <summary>The items the pensioner has for the month starting on <paramref name="period"/>, blocked items left out.</summary>
    public async Task<List<PensionerPayItemAmount>> GetForAsync(string pensionerNo, DateTime period, decimal monthlyPension)
    {
        var assigned = (await _assignments.GetListAsync(a => a.PensionerNo == pensionerNo)).Where(a => a.AppliesTo(period)).ToList();
        if (assigned.Count == 0)
        {
            return [];
        }

        var codes = assigned.Select(a => a.PayItemCode).Distinct().ToList();
        var items = (await _items.GetListAsync(i => codes.Contains(i.Code))).ToDictionary(i => i.Code, StringComparer.Ordinal);

        return assigned
            .Where(a => items.TryGetValue(a.PayItemCode, out var item) && !item.Blocked)
            .Select(a => new PensionerPayItemAmount(items[a.PayItemCode], items[a.PayItemCode].AmountFor(monthlyPension, a.Amount)))
            .Where(x => x.Amount > 0m)
            .OrderBy(x => x.Item.ItemType)
            .ThenBy(x => x.Item.Code, StringComparer.Ordinal)
            .ToList();
    }
}

/// <summary>Keeps the documents an exit needs in step with its reason, and holds approval until they are in.</summary>
public class ExitDocumentManager : DomainService
{
    private readonly IRepository<ExitReasonDocument, Guid> _reasonDocuments;
    private readonly IRepository<MemberExitDocument, Guid> _exitDocuments;

    public ExitDocumentManager(IRepository<ExitReasonDocument, Guid> reasonDocuments, IRepository<MemberExitDocument, Guid> exitDocuments)
    {
        _reasonDocuments = reasonDocuments;
        _exitDocuments = exitDocuments;
    }

    /// <summary>
    /// Adds the documents the exit's reason needs that the exit does not list yet. Documents not yet
    /// received that the reason does not need are dropped, so that changing the reason changes the list.
    /// Returns how many were added.
    /// </summary>
    public async Task<int> CopyFromReasonAsync(MemberExit exit)
    {
        var needed = (await _reasonDocuments.GetListAsync(d => d.ExitReasonCode == exit.ReasonCode)).OrderBy(d => d.LineNo).ToList();
        var listed = await _exitDocuments.GetListAsync(d => d.ExitNo == exit.No);

        var names = needed.Select(d => d.DocumentName.ToUpperInvariant()).ToHashSet(StringComparer.Ordinal);
        foreach (var stale in listed.Where(d => !d.Received && !names.Contains(d.DocumentName.ToUpperInvariant())).ToList())
        {
            await _exitDocuments.DeleteAsync(stale);
            listed.Remove(stale);
        }

        var have = listed.Select(d => d.DocumentName.ToUpperInvariant()).ToHashSet(StringComparer.Ordinal);
        var lineNo = listed.Count == 0 ? 0 : listed.Max(d => d.LineNo);
        var added = 0;

        foreach (var document in needed.Where(d => !have.Contains(d.DocumentName.ToUpperInvariant())))
        {
            lineNo += 10000;
            await _exitDocuments.InsertAsync(new MemberExitDocument(GuidGenerator.Create(), exit.No, lineNo, document.DocumentName, document.Mandatory));
            added++;
        }

        return added;
    }

    /// <summary>Refuses an actual exit while a mandatory document has not been received.</summary>
    public async Task EnsureCompleteAsync(MemberExit exit)
    {
        if (exit.WithdrawalType != MemberWithdrawalType.Actual)
        {
            return;
        }

        var missing = (await _exitDocuments.GetListAsync(d => d.ExitNo == exit.No && d.Mandatory && !d.Received))
            .OrderBy(d => d.LineNo)
            .Select(d => d.DocumentName)
            .ToList();

        if (missing.Count > 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.ExitDocumentsOutstanding)
                .WithData("documentNo", exit.No)
                .WithData("documents", string.Join(", ", missing));
        }
    }

    public async Task DeleteForAsync(string exitNo)
    {
        await _exitDocuments.DeleteAsync(d => d.ExitNo == exitNo);
    }
}

/// <summary>
/// Finds a scheme's age factor, and works out the salary a defined benefit pension is based on from
/// the member's salary history.
/// </summary>
public class DefinedBenefitTermsManager : DomainService
{
    private readonly IRepository<PensionAgeFactor, Guid> _factors;
    private readonly IRepository<MemberSalaryEntry, Guid> _salaries;

    public DefinedBenefitTermsManager(IRepository<PensionAgeFactor, Guid> factors, IRepository<MemberSalaryEntry, Guid> salaries)
    {
        _factors = factors;
        _salaries = salaries;
    }

    /// <summary>The factor for the age: the one for the highest age in the table not above it. None when the table has no such age.</summary>
    public async Task<PensionAgeFactor> FindFactorAsync(string schemeCode, PensionFactorType factorType, decimal age)
    {
        var whole = (int)Math.Floor(age);
        var rows = await _factors.GetListAsync(f => f.SchemeCode == schemeCode && f.FactorType == factorType && f.Age <= whole);
        return rows.OrderByDescending(f => f.Age).FirstOrDefault();
    }

    /// <summary>
    /// The yearly salary the pension is based on, by the scheme's basis, from the salaries recorded
    /// in the months before <paramref name="retirementDate"/>. None when the basis is the current
    /// salary or the member has no salary history.
    /// </summary>
    public async Task<decimal?> GetPensionableSalaryAsync(PensionScheme scheme, string memberNo, DateTime retirementDate)
    {
        if (scheme.PensionableSalaryBasis == PensionableSalaryBasis.CurrentSalary)
        {
            return null;
        }

        var end = new DateTime(retirementDate.Year, retirementDate.Month, 1);
        var years = Math.Max(1, scheme.SalaryAveragingYears);
        var start = end.AddYears(-years);

        var months = (await _salaries.GetListAsync(s => s.MemberNo == memberNo && s.Period >= start && s.Period < end))
            .Where(s => s.Salary > 0m)
            .ToDictionary(s => s.Period, s => s.Salary);

        if (months.Count == 0)
        {
            return null;
        }

        return scheme.PensionableSalaryBasis == PensionableSalaryBasis.AverageOfLastYears
            ? Round(months.Values.Sum() / months.Count * 12m)
            : Round(HighestTwelveMonths(months, start, end));
    }

    /// <summary>The highest total of any twelve months in a row; a month with no salary counts as nothing.</summary>
    public static decimal HighestTwelveMonths(IReadOnlyDictionary<DateTime, decimal> months, DateTime start, DateTime end)
    {
        var highest = 0m;
        for (var from = start; from < end; from = from.AddMonths(1))
        {
            var total = 0m;
            for (var month = from; month < from.AddMonths(12); month = month.AddMonths(1))
            {
                total += months.GetValueOrDefault(month);
            }

            highest = Math.Max(highest, total);
        }

        return highest;
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

/// <summary>Checks bank and branch codes, and finds the names a record shows for them.</summary>
public class PensionBankResolver : DomainService
{
    private readonly IRepository<PensionBank, Guid> _banks;
    private readonly IRepository<PensionBankBranch, Guid> _branches;

    public PensionBankResolver(IRepository<PensionBank, Guid> banks, IRepository<PensionBankBranch, Guid> branches)
    {
        _banks = banks;
        _branches = branches;
    }

    /// <summary>
    /// The names of the bank and branch, checking both exist and that the branch is one of the
    /// bank's. A blank bank gives no names; a branch needs a bank.
    /// </summary>
    public async Task<(string BankName, string BranchName)> ResolveAsync(string bankCode, string branchCode)
    {
        var bank = CodeTableEntity.NormalizeCode(bankCode);
        var branch = CodeTableEntity.NormalizeCode(branchCode);
        if (bank == null)
        {
            if (branch != null)
            {
                throw new BusinessException(ErpErrorCodes.Pensions.BankBranchNotFound).WithData("branchCode", branch).WithData("bankCode", string.Empty);
            }

            return (null, null);
        }

        var found = await _banks.FirstOrDefaultAsync(b => b.Code == bank)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Bank").WithData("code", bank);

        if (branch == null)
        {
            return (found.Description ?? found.Code, null);
        }

        var foundBranch = await _branches.FirstOrDefaultAsync(b => b.BankCode == bank && b.BranchCode == branch)
            ?? throw new BusinessException(ErpErrorCodes.Pensions.BankBranchNotFound).WithData("branchCode", branch).WithData("bankCode", bank);

        return (found.Description ?? found.Code, foundBranch.Name);
    }

    public Task EnsureBranchAsync(string bankCode, string branchCode) => ResolveAsync(bankCode, branchCode);
}
