using System.ComponentModel.DataAnnotations;
using System;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Finance;

public class GLAccountDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Name { get; set; }
    public GLAccountType AccountType { get; set; }
    public GLAccountCategory AccountCategory { get; set; }
    public string Subcategory { get; set; }
    public IncomeBalanceType IncomeBalance { get; set; }
    public bool DirectPosting { get; set; }
    public string VatProdPostingGroup { get; set; }
    public GeneralPostingType GenPostingType { get; set; }
    public string VatBusPostingGroup { get; set; }
    public bool Blocked { get; set; }
    public decimal NetChange { get; set; }
    public decimal Balance { get; set; }
    public string SearchName { get; set; }
    public GLAccountDebitCredit DebitCredit { get; set; }
    public bool ReconciliationAccount { get; set; }
    public string Totaling { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string GenProdPostingGroup { get; set; }
    public bool AutomaticExtTexts { get; set; }
    public string TaxAreaCode { get; set; }
    public bool TaxLiable { get; set; }
    public string TaxGroupCode { get; set; }
    public ConsolidationTranslationMethod ConsolTranslationMethod { get; set; }
    public string ConsolDebitAcc { get; set; }
    public string ConsolCreditAcc { get; set; }
    public string CostTypeNo { get; set; }
    public string DefaultDeferralTemplateCode { get; set; }
    public bool OmitDefaultDescrInJnl { get; set; }

    public string GlobalDimension1Code { get; set; }
    public string GlobalDimension2Code { get; set; }
    public int Indentation { get; set; }
    public int NoOfBlankLines { get; set; }
    public bool NewPage { get; set; }
}

public class CreateUpdateGLAccountDto
{
    public string No { get; set; }
    public string Name { get; set; }
    public GLAccountType AccountType { get; set; }
    public GLAccountCategory AccountCategory { get; set; }
    public string Subcategory { get; set; }
    public IncomeBalanceType IncomeBalance { get; set; }
    public bool DirectPosting { get; set; }
    public string VatProdPostingGroup { get; set; }
    public GeneralPostingType GenPostingType { get; set; }
    public string VatBusPostingGroup { get; set; }
    public string SearchName { get; set; }
    public GLAccountDebitCredit DebitCredit { get; set; }
    public bool ReconciliationAccount { get; set; }
    public string Totaling { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string GenProdPostingGroup { get; set; }
    public bool AutomaticExtTexts { get; set; }
    public string TaxAreaCode { get; set; }
    public bool TaxLiable { get; set; }
    public string TaxGroupCode { get; set; }
    public ConsolidationTranslationMethod ConsolTranslationMethod { get; set; }
    public string ConsolDebitAcc { get; set; }
    public string ConsolCreditAcc { get; set; }
    public string CostTypeNo { get; set; }
    public string DefaultDeferralTemplateCode { get; set; }
    public bool OmitDefaultDescrInJnl { get; set; }

    [StringLength(20)]
    public string GlobalDimension1Code { get; set; }

    [StringLength(20)]
    public string GlobalDimension2Code { get; set; }

    public int Indentation { get; set; }

    public int NoOfBlankLines { get; set; }

    public bool NewPage { get; set; }
}

public class GetGLAccountListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public GLAccountCategory? AccountCategory { get; set; }
    public GLAccountType? AccountType { get; set; }
}
