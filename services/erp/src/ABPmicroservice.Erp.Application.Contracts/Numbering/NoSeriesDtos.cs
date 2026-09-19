using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Numbering;

public class NoSeriesDto : EntityDto<Guid>
{
    public string Code { get; set; }
    public string Description { get; set; }
    public bool DefaultNos { get; set; }
    public bool ManualNos { get; set; }
    public bool DateOrder { get; set; }

    /// <summary>From the line in force today; shown in the list like BC's No. Series page.</summary>
    public string StartingNo { get; set; }
    public string EndingNo { get; set; }
    public string LastNoUsed { get; set; }

    /// <summary>The number the next record would get today; null when none is available.</summary>
    public string NextNo { get; set; }

    /// <summary>The warning number has been reached: time to extend the series.</summary>
    public bool Warning { get; set; }

    public List<NoSeriesLineDto> Lines { get; set; } = new();
}

public class NoSeriesLineDto : EntityDto<Guid>
{
    public int LineNo { get; set; }
    public DateTime? StartingDate { get; set; }
    public string StartingNo { get; set; }
    public string EndingNo { get; set; }
    public string WarningNo { get; set; }
    public int IncrementByNo { get; set; }
    public string LastNoUsed { get; set; }
    public DateTime? LastDateUsed { get; set; }
    public bool Open { get; set; }
}

public class CreateUpdateNoSeriesDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public bool DefaultNos { get; set; } = true;
    public bool ManualNos { get; set; }
    public bool DateOrder { get; set; }

    public List<NoSeriesLineInputDto> Lines { get; set; } = new();
}

public class NoSeriesLineInputDto
{
    /// <summary>Set for an existing line. Its Last No. Used is kept; it is never taken from the client.</summary>
    public Guid? Id { get; set; }

    public DateTime? StartingDate { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string StartingNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string EndingNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string WarningNo { get; set; }

    [Range(1, 1_000_000)]
    public int IncrementByNo { get; set; } = 1;
}

public class GetNoSeriesListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}

public class NextNoPreviewInput
{
    [Required]
    public string Code { get; set; }

    /// <summary>Defaults to today.</summary>
    public DateTime? Date { get; set; }
}

public class NextNoPreviewDto
{
    public string Code { get; set; }
    public string NextNo { get; set; }
    public bool ManualNos { get; set; }
    public bool DefaultNos { get; set; }
}

public interface INoSeriesAppService
    : ICrudAppService<NoSeriesDto, Guid, GetNoSeriesListInput, CreateUpdateNoSeriesDto, CreateUpdateNoSeriesDto>
{
    /// <summary>
    /// What the next number would be, without taking it. Lets a form show "SI-00012" as a hint
    /// and decide whether the number field is editable. Routed as GET /api/erp/no-series/next-no-preview.
    /// </summary>
    Task<NextNoPreviewDto> GetNextNoPreviewAsync(NextNoPreviewInput input);
}
