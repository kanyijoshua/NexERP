using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>Relatives.</summary>
public interface IRelativeAppService : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

/// <summary>Misc. Articles.</summary>
public interface IMiscArticleAppService : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

/// <summary>Confidential.</summary>
public interface IConfidentialAppService : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

/// <summary>Employee Statistics Groups.</summary>
public interface IEmployeeStatisticsGroupAppService : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

public class EmployeeRelativeDto : FullAuditedEntityDto<Guid>
{
    public string EmployeeNo { get; set; }
    public int LineNo { get; set; }
    public string RelativeCode { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string PhoneNo { get; set; }
    public string RelativesEmployeeNo { get; set; }
}

public class CreateUpdateEmployeeRelativeDto
{
    [Required]
    [StringLength(20)]
    public string EmployeeNo { get; set; }

    /// <summary>Zero takes the next free number.</summary>
    public int LineNo { get; set; }

    [StringLength(10)]
    public string RelativeCode { get; set; }

    [StringLength(30)]
    public string FirstName { get; set; }

    [StringLength(30)]
    public string MiddleName { get; set; }

    [StringLength(30)]
    public string LastName { get; set; }

    public DateTime? BirthDate { get; set; }

    [StringLength(30)]
    public string PhoneNo { get; set; }

    [StringLength(20)]
    public string RelativesEmployeeNo { get; set; }
}

public class GetEmployeeRelativeListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string EmployeeNo { get; set; }
}

/// <summary>Employee Relatives.</summary>
public interface IEmployeeRelativeAppService : ICrudAppService<EmployeeRelativeDto, Guid, GetEmployeeRelativeListInput, CreateUpdateEmployeeRelativeDto, CreateUpdateEmployeeRelativeDto> { }

public class EmployeeQualificationDto : FullAuditedEntityDto<Guid>
{
    public string EmployeeNo { get; set; }
    public int LineNo { get; set; }
    public string QualificationCode { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public EmployeeQualificationType Type { get; set; }
    public string Description { get; set; }
    public string InstitutionCompany { get; set; }
    public decimal Cost { get; set; }
    public string CourseGrade { get; set; }
    public EmployeeStatus EmployeeStatus { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

public class CreateUpdateEmployeeQualificationDto
{
    [Required]
    [StringLength(20)]
    public string EmployeeNo { get; set; }

    /// <summary>Zero takes the next free number.</summary>
    public int LineNo { get; set; }

    [StringLength(10)]
    public string QualificationCode { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public EmployeeQualificationType Type { get; set; }

    [StringLength(100)]
    public string Description { get; set; }

    [StringLength(100)]
    public string InstitutionCompany { get; set; }

    public decimal Cost { get; set; }

    [StringLength(50)]
    public string CourseGrade { get; set; }

    public EmployeeStatus EmployeeStatus { get; set; }

    public DateTime? ExpirationDate { get; set; }
}

public class GetEmployeeQualificationListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string EmployeeNo { get; set; }
}

/// <summary>Employee Qualifications.</summary>
public interface IEmployeeQualificationAppService : ICrudAppService<EmployeeQualificationDto, Guid, GetEmployeeQualificationListInput, CreateUpdateEmployeeQualificationDto, CreateUpdateEmployeeQualificationDto> { }

public class MiscArticleInformationDto : FullAuditedEntityDto<Guid>
{
    public string EmployeeNo { get; set; }
    public string MiscArticleCode { get; set; }
    public int LineNo { get; set; }
    public string Description { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool InUse { get; set; }
    public string SerialNo { get; set; }
}

public class CreateUpdateMiscArticleInformationDto
{
    [Required]
    [StringLength(20)]
    public string EmployeeNo { get; set; }

    [Required]
    [StringLength(10)]
    public string MiscArticleCode { get; set; }

    /// <summary>Zero takes the next free number.</summary>
    public int LineNo { get; set; }

    [StringLength(100)]
    public string Description { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public bool InUse { get; set; }

    [StringLength(50)]
    public string SerialNo { get; set; }
}

public class GetMiscArticleInformationListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string EmployeeNo { get; set; }
}

/// <summary>Misc. Article Information.</summary>
public interface IMiscArticleInformationAppService : ICrudAppService<MiscArticleInformationDto, Guid, GetMiscArticleInformationListInput, CreateUpdateMiscArticleInformationDto, CreateUpdateMiscArticleInformationDto> { }

public class ConfidentialInformationDto : FullAuditedEntityDto<Guid>
{
    public string EmployeeNo { get; set; }
    public string ConfidentialCode { get; set; }
    public int LineNo { get; set; }
    public string Description { get; set; }
}

public class CreateUpdateConfidentialInformationDto
{
    [Required]
    [StringLength(20)]
    public string EmployeeNo { get; set; }

    [Required]
    [StringLength(10)]
    public string ConfidentialCode { get; set; }

    /// <summary>Zero takes the next free number.</summary>
    public int LineNo { get; set; }

    [StringLength(100)]
    public string Description { get; set; }
}

public class GetConfidentialInformationListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string EmployeeNo { get; set; }
}

/// <summary>Confidential Information.</summary>
public interface IConfidentialInformationAppService : ICrudAppService<ConfidentialInformationDto, Guid, GetConfidentialInformationListInput, CreateUpdateConfidentialInformationDto, CreateUpdateConfidentialInformationDto> { }

public class AlternativeAddressDto : FullAuditedEntityDto<Guid>
{
    public string EmployeeNo { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Name2 { get; set; }
    public string Address { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string County { get; set; }
    public string PhoneNo { get; set; }
    public string FaxNo { get; set; }
    public string Email { get; set; }
    public string CountryRegionCode { get; set; }
}

public class CreateUpdateAlternativeAddressDto
{
    [Required]
    [StringLength(20)]
    public string EmployeeNo { get; set; }

    [Required]
    [StringLength(10)]
    public string Code { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(50)]
    public string Name2 { get; set; }

    [StringLength(100)]
    public string Address { get; set; }

    [StringLength(50)]
    public string Address2 { get; set; }

    [StringLength(30)]
    public string City { get; set; }

    [StringLength(20)]
    public string PostCode { get; set; }

    [StringLength(30)]
    public string County { get; set; }

    [StringLength(30)]
    public string PhoneNo { get; set; }

    [StringLength(30)]
    public string FaxNo { get; set; }

    [StringLength(80)]
    public string Email { get; set; }

    [StringLength(10)]
    public string CountryRegionCode { get; set; }
}

public class GetAlternativeAddressListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string EmployeeNo { get; set; }
}

/// <summary>Alternative Addresses.</summary>
public interface IAlternativeAddressAppService : ICrudAppService<AlternativeAddressDto, Guid, GetAlternativeAddressListInput, CreateUpdateAlternativeAddressDto, CreateUpdateAlternativeAddressDto> { }

public class HumanResourceCommentLineDto : FullAuditedEntityDto<Guid>
{
    public HumanResourcesCommentTableName TableName { get; set; }
    public string No { get; set; }
    public int TableLineNo { get; set; }
    public int LineNo { get; set; }
    public string AlternativeAddressCode { get; set; }
    public DateTime? Date { get; set; }
    public string Code { get; set; }
    public string Comment { get; set; }
}

public class CreateUpdateHumanResourceCommentLineDto
{
    public HumanResourcesCommentTableName TableName { get; set; }

    [Required]
    [StringLength(20)]
    public string No { get; set; }

    public int TableLineNo { get; set; }

    /// <summary>Zero takes the next free number.</summary>
    public int LineNo { get; set; }

    [StringLength(10)]
    public string AlternativeAddressCode { get; set; }

    public DateTime? Date { get; set; }

    [StringLength(10)]
    public string Code { get; set; }

    [StringLength(80)]
    public string Comment { get; set; }
}

public class GetHumanResourceCommentLineListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string No { get; set; }
}

/// <summary>Human Resource Comment Lines.</summary>
public interface IHumanResourceCommentLineAppService : ICrudAppService<HumanResourceCommentLineDto, Guid, GetHumanResourceCommentLineListInput, CreateUpdateHumanResourceCommentLineDto, CreateUpdateHumanResourceCommentLineDto> { }
