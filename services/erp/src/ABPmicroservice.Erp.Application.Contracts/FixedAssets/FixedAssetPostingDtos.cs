using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.FixedAssets;

public class FAJournalBatchDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string DepreciationBookCode { get; set; }
}

public class CreateUpdateFAJournalBatchDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxJournalTemplateNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(10)]
    public string DepreciationBookCode { get; set; }
}

public class FAJournalLineDto : EntityDto<Guid>
{
    public Guid FAJournalBatchId { get; set; }
    public int LineNo { get; set; }
    public DateTime PostingDate { get; set; }
    public string DocumentNo { get; set; }
    public string FANo { get; set; }
    public string DepreciationBookCode { get; set; }
    public FAJournalPostingType FAPostingType { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public GenJournalAccountType? BalAccountType { get; set; }
    public string BalAccountNo { get; set; }
    public int NoOfDepreciationDays { get; set; }
}

public class CreateUpdateFAJournalLineDto
{
    public Guid FAJournalBatchId { get; set; }
    public DateTime PostingDate { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string FANo { get; set; }

    [StringLength(10)]
    public string DepreciationBookCode { get; set; }

    public FAJournalPostingType FAPostingType { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public decimal Amount { get; set; }
    public GenJournalAccountType? BalAccountType { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string BalAccountNo { get; set; }
}

public class GetFAJournalLineListInput : ErpPagedListInput
{
    public Guid? FAJournalBatchId { get; set; }
}

public class CalculateDepreciationInput
{
    public DateTime UntilDate { get; set; }

    /// <summary>Blank posts on the until date.</summary>
    public DateTime? PostingDate { get; set; }

    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    /// <summary>Blank calculates every asset of the book.</summary>
    public string FANo { get; set; }
}

public class FAJournalPostingResultDto
{
    public int LineCount { get; set; }
}

/// <summary>An asset in one depreciation book: its dates and what it is carried at.</summary>
public class FABookValueDto
{
    public string DepreciationBookCode { get; set; }
    public DateTime? AcquisitionDate { get; set; }
    public DateTime? LastDepreciationDate { get; set; }
    public DateTime? DisposalDate { get; set; }
    public decimal AcquisitionCost { get; set; }
    public decimal Depreciation { get; set; }
    public decimal WriteDown { get; set; }
    public decimal Appreciation { get; set; }
    public decimal BookValue { get; set; }
    public decimal Proceeds { get; set; }
    public decimal GainLoss { get; set; }
}

public class GetFABookValuesInput
{
    [Required]
    public string FANo { get; set; }
}

public interface IFAJournalBatchAppService : ICrudAppService<FAJournalBatchDto, Guid, GetCodeTableListInput, CreateUpdateFAJournalBatchDto>
{
    Task<FAJournalPostingResultDto> RunPostingAsync(Guid id);

    Task<int> CalculateDepreciationAsync(Guid id, CalculateDepreciationInput input);
}

public interface IFAJournalLineAppService : ICrudAppService<FAJournalLineDto, Guid, GetFAJournalLineListInput, CreateUpdateFAJournalLineDto> { }

public interface IFixedAssetValueAppService : IApplicationService
{
    Task<List<FABookValueDto>> GetBookValuesAsync(GetFABookValuesInput input);
}
