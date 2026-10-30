using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Quality;

public class QualityControlPointDto : CodeTableDto
{
    public QualityOperation Operation { get; set; }
    public string ItemNo { get; set; }
    public string ItemCategoryCode { get; set; }
    public QualityTestType TestType { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public string UnitOfMeasure { get; set; }
    public string Instructions { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateQualityControlPointDto : CreateUpdateCodeTableDto
{
    public QualityOperation Operation { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ItemCategoryCode { get; set; }

    public QualityTestType TestType { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string UnitOfMeasure { get; set; }

    [StringLength(2000)]
    public string Instructions { get; set; }

    public bool Blocked { get; set; }
}

public interface IQualityControlPointAppService
    : ICrudAppService<QualityControlPointDto, Guid, GetCodeTableListInput, CreateUpdateQualityControlPointDto> { }

public class QualityCheckDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string ControlPointCode { get; set; }
    public QualityOperation Operation { get; set; }
    public string ItemNo { get; set; }
    public string ItemDescription { get; set; }
    public string SourceDocumentNo { get; set; }
    public string LotNo { get; set; }
    public decimal Quantity { get; set; }
    public QualityTestType TestType { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public decimal? MeasuredValue { get; set; }
    public QualityCheckResult Result { get; set; }
    public string CheckedBy { get; set; }
    public DateTime? CheckedDate { get; set; }
    public string Note { get; set; }
}

/// <summary>A check made by hand, at a control point, for an item.</summary>
public class CreateQualityCheckDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ControlPointCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string SourceDocumentNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string LotNo { get; set; }

    public decimal Quantity { get; set; }
}

public class GetQualityCheckListInput : ErpPagedListInput
{
    public string SourceDocumentNo { get; set; }
    public QualityCheckResult? Result { get; set; }
}

public class RecordQualityCheckInput
{
    /// <summary>A pass/fail test: whether it passed.</summary>
    public bool? Passed { get; set; }

    /// <summary>A measurement: the value measured.</summary>
    public decimal? MeasuredValue { get; set; }

    [StringLength(2000)]
    public string Note { get; set; }
}

public interface IQualityCheckAppService : IReadOnlyAppService<QualityCheckDto, Guid, GetQualityCheckListInput>
{
    Task<QualityCheckDto> CreateAsync(CreateQualityCheckDto input);

    Task<QualityCheckDto> RecordAsync(Guid id, RecordQualityCheckInput input);

    Task<QualityCheckDto> ResetAsync(Guid id);

    Task DeleteAsync(Guid id);
}
