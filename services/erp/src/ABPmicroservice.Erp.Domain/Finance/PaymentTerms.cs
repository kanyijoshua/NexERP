using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Payment Terms: when an invoice falls due and when a payment
/// discount runs out, as date formulas on the document date ("30D", "CM+15D").
/// </summary>
public class PaymentTerms : CodeTableEntity
{
    protected override int MaxCodeLength => ErpDomainConsts.MaxPaymentTermsCodeLength;

    public string DueDateCalculation { get; private set; }
    public string DiscountDateCalculation { get; private set; }
    public decimal DiscountPercent { get; private set; }

    /// <summary>Calc. Pmt. Disc. on Cr. Memos.</summary>
    public bool CalcPmtDiscOnCrMemos { get; private set; }

    protected PaymentTerms() { }

    public PaymentTerms(Guid id, string code, string description, string dueDateCalculation, string discountDateCalculation = null, decimal discountPercent = 0m)
        : base(id, code, description)
    {
        SetCalculations(dueDateCalculation, discountDateCalculation, discountPercent);
    }

    public void SetCalculations(string dueDateCalculation, string discountDateCalculation, decimal discountPercent)
    {
        DueDateCalculation = Formula(dueDateCalculation);
        DiscountDateCalculation = Formula(discountDateCalculation);

        if (discountPercent < 0 || discountPercent > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", discountPercent);
        }

        DiscountPercent = discountPercent;
    }

    /// <summary>The due date of a document dated <paramref name="documentDate"/>; the date itself with no formula.</summary>
    public DateTime CalculateDueDate(DateTime documentDate)
    {
        return DueDateCalculation == null ? documentDate.Date : DateFormula.Parse(DueDateCalculation).Apply(documentDate.Date);
    }

    public DateTime? CalculateDiscountDate(DateTime documentDate)
    {
        return DiscountDateCalculation == null ? null : DateFormula.Parse(DiscountDateCalculation).Apply(documentDate.Date);
    }

    private static string Formula(string text)
    {
        if (text.IsNullOrWhiteSpace())
        {
            return null;
        }

        var trimmed = Check.Length(text.Trim().ToUpperInvariant(), nameof(text), ErpDomainConsts.MaxDateFormulaLength);
        if (!DateFormula.TryParse(trimmed, out _))
        {
            throw new BusinessException(ErpErrorCodes.Journals.InvalidDateFormula).WithData("formula", trimmed);
        }

        return trimmed;
    }

    /// <summary>The card fields beyond those the posting routines read.</summary>
    public void SetAdditionalFields(bool calcPmtDiscOnCrMemos)
    {
        CalcPmtDiscOnCrMemos = calcPmtDiscOnCrMemos;
    }
}

public class PaymentTermsManager : DomainService
{
    private readonly IRepository<PaymentTerms, Guid> _repository;

    public PaymentTermsManager(IRepository<PaymentTerms, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>The due date the payment terms give a document of this date; null without terms.</summary>
    public async Task<DateTime?> CalculateDueDateAsync(string paymentTermsCode, DateTime documentDate)
    {
        var code = CodeTableEntity.NormalizeCode(paymentTermsCode);
        var terms = code == null ? null : await _repository.FirstOrDefaultAsync(t => t.Code == code);
        return terms?.CalculateDueDate(documentDate);
    }
}
