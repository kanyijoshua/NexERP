using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Companies;

/// <summary>A row of a code table: payment terms, causes of absence, posting groups and the like.</summary>
public class CodeTableDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string Description { get; set; }
}

public class CreateUpdateCodeTableDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class GetCodeTableListInput : ErpPagedListInput
{
    /// <summary>Matches code or description, ignoring case.</summary>
    public string Filter { get; set; }
}
