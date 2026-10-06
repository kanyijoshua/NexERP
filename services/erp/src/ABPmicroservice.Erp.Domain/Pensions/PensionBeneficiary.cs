using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// A beneficiary (nominee) of a member: who receives the member's benefit when the member dies, and
/// what share of it. The active shares of a member add up to at most 100%, and must be exactly 100%
/// before a death benefit is approved.
/// </summary>
public class PensionBeneficiary : CompanyEntity
{
    public string MemberNo { get; private set; }
    public int LineNo { get; private set; }
    public string Name { get; private set; }
    public BeneficiaryRelationship Relationship { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string NationalId { get; private set; }

    /// <summary>The beneficiary's share of the benefit, as a percentage.</summary>
    public decimal BenefitPct { get; private set; }

    public BeneficiaryStatus Status { get; private set; }

    /// <summary>Who receives the share on behalf of a beneficiary who is a minor.</summary>
    public string GuardianName { get; private set; }

    public string PhoneNo { get; private set; }
    public string Email { get; private set; }
    public string BankName { get; private set; }
    public string BankAccountNo { get; private set; }

    protected PensionBeneficiary() { }

    public PensionBeneficiary(Guid id, string memberNo, int lineNo, string name)
        : base(id)
    {
        MemberNo = Check.NotNullOrWhiteSpace(memberNo, nameof(memberNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        LineNo = lineNo;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength).Trim();
    }

    public void Set(
        string name,
        BeneficiaryRelationship relationship,
        DateTime? dateOfBirth,
        string nationalId,
        decimal benefitPct,
        BeneficiaryStatus status,
        string guardianName
    )
    {
        if (benefitPct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", benefitPct);
        }

        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength).Trim();
        Relationship = relationship;
        DateOfBirth = dateOfBirth?.Date;
        NationalId = Check.Length(nationalId?.Trim(), nameof(nationalId), ErpDomainConsts.MaxCodeLength * 2);
        BenefitPct = benefitPct;
        Status = status;
        GuardianName = Check.Length(guardianName?.Trim(), nameof(guardianName), ErpDomainConsts.MaxNameLength);
    }

    public void SetContact(string phoneNo, string email, string bankName, string bankAccountNo)
    {
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), ErpDomainConsts.MaxPhoneLength);
        Email = Check.Length(email, nameof(email), ErpDomainConsts.MaxEmailLength);
        BankName = Check.Length(bankName, nameof(bankName), ErpDomainConsts.MaxNameLength);
        BankAccountNo = Check.Length(bankAccountNo, nameof(bankAccountNo), ErpDomainConsts.MaxBankAccountNoLength);
    }

    /// <summary>Whether the beneficiary is under 18 on a date, so that the share is paid to a guardian.</summary>
    public bool IsMinorOn(DateTime date) => DateOfBirth.HasValue && DateOfBirth.Value.AddYears(18) > date.Date;
}

/// <summary>One beneficiary's part of a benefit: the share and the amount it comes to.</summary>
public record BeneficiaryShare(PensionBeneficiary Beneficiary, decimal Amount);

/// <summary>Keeps the beneficiaries' shares of each member within 100%, and shares a benefit out between them.</summary>
public class PensionBeneficiaryManager : DomainService
{
    private readonly IRepository<PensionBeneficiary, Guid> _beneficiaries;

    public PensionBeneficiaryManager(IRepository<PensionBeneficiary, Guid> beneficiaries)
    {
        _beneficiaries = beneficiaries;
    }

    /// <summary>
    /// Refuses a share that would take the member's active shares above 100%. <paramref name="beneficiary"/>
    /// is counted with its new share, whether or not it has been saved yet.
    /// </summary>
    public async Task EnsureSharesWithinLimitAsync(PensionBeneficiary beneficiary)
    {
        var others = await _beneficiaries.GetListAsync(b => b.MemberNo == beneficiary.MemberNo && b.Id != beneficiary.Id && b.Status == BeneficiaryStatus.Active);
        var total = others.Sum(b => b.BenefitPct) + (beneficiary.Status == BeneficiaryStatus.Active ? beneficiary.BenefitPct : 0m);

        if (total > 100m)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.BeneficiaryShareExceeded).WithData("memberNo", beneficiary.MemberNo).WithData("total", total);
        }
    }

    /// <summary>The member's active beneficiaries, whose shares must come to exactly 100%.</summary>
    public async Task<List<PensionBeneficiary>> GetCompleteAsync(string memberNo)
    {
        var active = (await _beneficiaries.GetListAsync(b => b.MemberNo == memberNo && b.Status == BeneficiaryStatus.Active)).OrderBy(b => b.LineNo).ToList();
        var total = active.Sum(b => b.BenefitPct);

        return total == 100m
            ? active
            : throw new BusinessException(ErpErrorCodes.Pensions.BeneficiarySharesIncomplete).WithData("memberNo", memberNo).WithData("total", total);
    }

    /// <summary>
    /// Shares an amount out between beneficiaries by their percentages. Rounding is absorbed by the
    /// largest share, so the parts always add up to the amount.
    /// </summary>
    public static List<BeneficiaryShare> Share(decimal amount, IReadOnlyList<PensionBeneficiary> beneficiaries)
    {
        var shares = beneficiaries.Select(b => new BeneficiaryShare(b, Math.Round(amount * b.BenefitPct / 100m, 2, MidpointRounding.AwayFromZero))).ToList();
        if (shares.Count == 0)
        {
            return shares;
        }

        var difference = amount - shares.Sum(s => s.Amount);
        if (difference != 0m)
        {
            var largest = shares.OrderByDescending(s => s.Beneficiary.BenefitPct).First();
            shares[shares.IndexOf(largest)] = largest with { Amount = largest.Amount + difference };
        }

        return shares;
    }
}
