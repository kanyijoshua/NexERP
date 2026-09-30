using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>Human Resources Setup (BC page 5233).</summary>
public class HumanResourcesSetupDto
{
    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string EmployeeNos { get; set; }

    [StringLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength)]
    public string BaseUnitOfMeasure { get; set; }
}

public class HumanResourceUnitOfMeasureDto : CodeTableDto
{
    public decimal QtyPerUnitOfMeasure { get; set; }
}

public class CreateUpdateHumanResourceUnitOfMeasureDto : CreateUpdateCodeTableDto
{
    public decimal QtyPerUnitOfMeasure { get; set; } = 1m;
}

public class EmployeePostingGroupDto : CodeTableDto
{
    public string PayablesAccountNo { get; set; }
}

public class CreateUpdateEmployeePostingGroupDto : CreateUpdateCodeTableDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PayablesAccountNo { get; set; }
}

public class CauseOfAbsenceDto : CodeTableDto
{
    public string UnitOfMeasureCode { get; set; }
}

public class CreateUpdateCauseOfAbsenceDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength)]
    public string UnitOfMeasureCode { get; set; }
}

public class EmployeeDto : FullAuditedEntityDto<Guid>
{
    /// <summary>What the company owes the employee (negative) or is owed back.</summary>
    public decimal Balance { get; set; }

    public string No { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string JobTitle { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string CountryRegionCode { get; set; }
    public string PhoneNo { get; set; }
    public string MobilePhoneNo { get; set; }
    public string Email { get; set; }
    public string CompanyEmail { get; set; }
    public DateTime? BirthDate { get; set; }
    public string SocialSecurityNo { get; set; }
    public DateTime? EmploymentDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public DateTime? InactiveDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string GroundsForTermCode { get; set; }
    public string EmplymtContractCode { get; set; }
    public string UnionCode { get; set; }
    public string EmployeePostingGroup { get; set; }
    public string BankAccountNo { get; set; }
    public string Iban { get; set; }
    public string SalespersPurchCode { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateEmployeeDto
{
    /// <summary>Blank takes the next number of the Human Resources Setup's Employee Nos.</summary>
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength / 2)]
    public string FirstName { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength / 2)]
    public string MiddleName { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength / 2)]
    public string LastName { get; set; }

    [StringLength(ErpDomainConsts.MaxJobTitleLength)]
    public string JobTitle { get; set; }

    [StringLength(ErpDomainConsts.MaxAddressLength)]
    public string Address { get; set; }

    [StringLength(ErpDomainConsts.MaxCityLength)]
    public string City { get; set; }

    [StringLength(ErpDomainConsts.MaxPostCodeLength)]
    public string PostCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCountryRegionCodeLength)]
    public string CountryRegionCode { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string PhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string MobilePhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailLength)]
    public string Email { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailLength)]
    public string CompanyEmail { get; set; }

    public DateTime? BirthDate { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength * 2)]
    public string SocialSecurityNo { get; set; }

    public DateTime? EmploymentDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public DateTime? InactiveDate { get; set; }
    public DateTime? TerminationDate { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string GroundsForTermCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string EmplymtContractCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string UnionCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string EmployeePostingGroup { get; set; }

    [StringLength(ErpDomainConsts.MaxBankAccountNoLength)]
    public string BankAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxIbanLength)]
    public string Iban { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SalespersPurchCode { get; set; }
}

public class GetEmployeeListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public EmployeeStatus? Status { get; set; }
}

public class EmployeeAbsenceDto : FullAuditedEntityDto<Guid>
{
    public Guid EmployeeId { get; set; }
    public string EmployeeNo { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string CauseOfAbsenceCode { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasureCode { get; set; }
}

public class CreateUpdateEmployeeAbsenceDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string EmployeeNo { get; set; }

    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string CauseOfAbsenceCode { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    /// <summary>Zero counts one per calendar day.</summary>
    public decimal Quantity { get; set; }

    [StringLength(ErpDomainConsts.MaxUnitOfMeasureCodeLength)]
    public string UnitOfMeasureCode { get; set; }
}

public class GetEmployeeAbsenceListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public Guid? EmployeeId { get; set; }
}

public interface IHumanResourcesSetupAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/human-resources-setup.</summary>
    Task<HumanResourcesSetupDto> GetAsync();

    Task<HumanResourcesSetupDto> UpdateAsync(HumanResourcesSetupDto input);
}

public interface IHumanResourceUnitOfMeasureAppService
    : ICrudAppService<HumanResourceUnitOfMeasureDto, Guid, GetCodeTableListInput, CreateUpdateHumanResourceUnitOfMeasureDto, CreateUpdateHumanResourceUnitOfMeasureDto> { }

public interface IEmployeePostingGroupAppService
    : ICrudAppService<EmployeePostingGroupDto, Guid, GetCodeTableListInput, CreateUpdateEmployeePostingGroupDto, CreateUpdateEmployeePostingGroupDto> { }

public interface ICauseOfAbsenceAppService
    : ICrudAppService<CauseOfAbsenceDto, Guid, GetCodeTableListInput, CreateUpdateCauseOfAbsenceDto, CreateUpdateCauseOfAbsenceDto> { }

public interface IQualificationAppService
    : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

public interface IUnionAppService
    : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

public interface IEmploymentContractAppService
    : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

public interface IGroundsForTerminationAppService
    : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

public interface IEmployeeAppService
    : ICrudAppService<EmployeeDto, Guid, GetEmployeeListInput, CreateUpdateEmployeeDto, CreateUpdateEmployeeDto> { }

public interface IEmployeeAbsenceAppService
    : ICrudAppService<EmployeeAbsenceDto, Guid, GetEmployeeAbsenceListInput, CreateUpdateEmployeeAbsenceDto, CreateUpdateEmployeeAbsenceDto> { }

public class EmployeeLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeNo { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime DocumentDate { get; set; }
    public GLEntryDocumentType DocumentType { get; set; }
    public string DocumentNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public decimal RemainingAmount { get; set; }
    public bool Open { get; set; }
    public long ClosedByEntryNo { get; set; }
    public bool Reversed { get; set; }
}

public class GetEmployeeLedgerEntryListInput : ErpPagedListInput
{
    /// <summary>Matches the document number, the employee number or the description.</summary>
    public string Filter { get; set; }
    public string EmployeeNo { get; set; }
    public bool OnlyOpen { get; set; }
}

/// <summary>Employee Ledger Entries (BC page 5237): expense claims and the payouts that settle them.</summary>
public interface IEmployeeLedgerEntryAppService : IApplicationService
{
    Task<PagedResultDto<EmployeeLedgerEntryDto>> GetListAsync(GetEmployeeLedgerEntryListInput input);
}
