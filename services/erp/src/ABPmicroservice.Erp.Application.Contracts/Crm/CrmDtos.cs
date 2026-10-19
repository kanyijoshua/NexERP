using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Crm;

public class MarketingSetupDto
{
    public string ContactNos { get; set; }
    public string OpportunityNos { get; set; }
    public string CampaignNos { get; set; }
    public string CustomerPostingGroup { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string VatBusPostingGroup { get; set; }
    public string PaymentTermsCode { get; set; }
}

public interface IMarketingSetupAppService : IApplicationService
{
    Task<MarketingSetupDto> GetAsync();

    Task<MarketingSetupDto> UpdateAsync(MarketingSetupDto input);
}

public class ContactDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public ContactType Type { get; set; }
    public string Name { get; set; }
    public string CompanyNo { get; set; }
    public string CompanyName { get; set; }
    public string JobTitle { get; set; }
    public string Email { get; set; }
    public string PhoneNo { get; set; }
    public string MobilePhoneNo { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string CountryRegionCode { get; set; }
    public string SalespersonCode { get; set; }
    public string CustomerNo { get; set; }
    public string VendorNo { get; set; }
}

public class CreateUpdateContactDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    public ContactType Type { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CompanyNo { get; set; }

    [StringLength(50)]
    public string JobTitle { get; set; }

    [StringLength(80)]
    public string Email { get; set; }

    [StringLength(30)]
    public string PhoneNo { get; set; }

    [StringLength(30)]
    public string MobilePhoneNo { get; set; }

    [StringLength(100)]
    public string Address { get; set; }

    [StringLength(30)]
    public string City { get; set; }

    [StringLength(10)]
    public string CountryRegionCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SalespersonCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string VendorNo { get; set; }
}

public class GetContactListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public ContactType? Type { get; set; }
    public string CompanyNo { get; set; }
}

public class OpportunityDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public OpportunityKind Kind { get; set; }
    public string Description { get; set; }
    public string ContactNo { get; set; }
    public string ContactName { get; set; }
    public string ContactCompanyName { get; set; }
    public string Email { get; set; }
    public string PhoneNo { get; set; }
    public string SalespersonCode { get; set; }
    public string CampaignNo { get; set; }
    public OpportunityPriority Priority { get; set; }
    public Guid? StageId { get; set; }
    public string StageName { get; set; }
    public decimal Probability { get; set; }
    public decimal ExpectedRevenue { get; set; }
    public decimal WeightedRevenue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public OpportunityStatus Status { get; set; }
    public string CloseOpportunityCode { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string SalesQuoteNo { get; set; }
}

public class CreateUpdateOpportunityDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    public OpportunityKind Kind { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ContactNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string ContactCompanyName { get; set; }

    [StringLength(80)]
    public string Email { get; set; }

    [StringLength(30)]
    public string PhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SalespersonCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CampaignNo { get; set; }

    public OpportunityPriority Priority { get; set; } = OpportunityPriority.Normal;

    /// <summary>Blank keeps the probability of the stage.</summary>
    public decimal? Probability { get; set; }

    public decimal ExpectedRevenue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
}

public class GetOpportunityListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public OpportunityKind? Kind { get; set; }
    public OpportunityStatus? Status { get; set; }
    public string ContactNo { get; set; }
    public string SalespersonCode { get; set; }
}

public class MoveOpportunityInput
{
    public Guid StageId { get; set; }

    /// <summary>Needed when the stage loses the opportunity.</summary>
    public string CloseCode { get; set; }
}

public class CloseOpportunityInput
{
    public string CloseCode { get; set; }
}

public class OpportunityEntryDto : EntityDto<Guid>
{
    public DateTime Date { get; set; }
    public string StageName { get; set; }
    public OpportunityStatus Status { get; set; }
    public decimal Probability { get; set; }
    public decimal ExpectedRevenue { get; set; }
    public string Note { get; set; }
}

public class GetPipelineInput
{
    /// <summary>"Lead" or "Opportunity".</summary>
    [Required]
    public string Pipeline { get; set; }
}

public class PipelineStageDto
{
    public Guid StageId { get; set; }
    public string Name { get; set; }
    public int Sequence { get; set; }
    public bool IsWon { get; set; }
    public bool IsLost { get; set; }
    public int Count { get; set; }
    public decimal ExpectedRevenue { get; set; }
    public decimal WeightedRevenue { get; set; }
}

public class PipelineDto
{
    public string Pipeline { get; set; }
    public List<PipelineStageDto> Stages { get; set; } = new();
    public List<OpportunityDto> Cards { get; set; } = new();
}

public interface IContactAppService : ICrudAppService<ContactDto, Guid, GetContactListInput, CreateUpdateContactDto>
{
    /// <summary>The customer the contact is; made from the contact when there is none yet.</summary>
    Task<ContactDto> MakeCustomerAsync(Guid id);
}

public interface IOpportunityAppService : ICrudAppService<OpportunityDto, Guid, GetOpportunityListInput, CreateUpdateOpportunityDto>
{
    Task<OpportunityDto> MoveAsync(Guid id, MoveOpportunityInput input);

    Task<OpportunityDto> ConvertAsync(Guid id);

    Task<OpportunityDto> WinAsync(Guid id, CloseOpportunityInput input);

    Task<OpportunityDto> LoseAsync(Guid id, CloseOpportunityInput input);

    Task<OpportunityDto> ReopenAsync(Guid id);

    /// <summary>Makes a sales quote for the opportunity's contact (as a customer) and links it.</summary>
    Task<OpportunityDto> MakeSalesQuoteAsync(Guid id);

    Task<List<OpportunityEntryDto>> GetHistoryAsync(Guid id);

    Task<PipelineDto> GetPipelineAsync(GetPipelineInput input);
}

public class CloseOpportunityCodeDto : CodeTableDto
{
    public CloseOpportunityType Type { get; set; }
}

public class CreateUpdateCloseOpportunityCodeDto : CreateUpdateCodeTableDto
{
    public CloseOpportunityType Type { get; set; }
}

public interface ICloseOpportunityCodeAppService : ICrudAppService<CloseOpportunityCodeDto, Guid, GetCodeTableListInput, CreateUpdateCloseOpportunityCodeDto> { }

public class ActivityTypeDto : CodeTableDto
{
    public string Icon { get; set; }
}

public class CreateUpdateActivityTypeDto : CreateUpdateCodeTableDto
{
    [StringLength(50)]
    public string Icon { get; set; }
}

public interface IActivityTypeAppService : ICrudAppService<ActivityTypeDto, Guid, GetCodeTableListInput, CreateUpdateActivityTypeDto> { }

public class PipelineStageSetupDto : EntityDto<Guid>
{
    public string PipelineType { get; set; }
    public int Sequence { get; set; }
    public string Name { get; set; }
    public bool FoldedInKanban { get; set; }
    public bool IsWonStage { get; set; }
    public bool IsLostStage { get; set; }
    public decimal DefaultProbability { get; set; }
}

public class CreateUpdatePipelineStageDto
{
    [Required]
    [StringLength(50)]
    public string PipelineType { get; set; }

    public int Sequence { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    public bool FoldedInKanban { get; set; }
    public bool IsWonStage { get; set; }
    public bool IsLostStage { get; set; }
    public decimal DefaultProbability { get; set; }
}

public class GetPipelineStageListInput : ErpPagedListInput
{
    public string PipelineType { get; set; }
}

public interface IPipelineStageAppService : ICrudAppService<PipelineStageSetupDto, Guid, GetPipelineStageListInput, CreateUpdatePipelineStageDto> { }

public class CampaignDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public DateTime? StartingDate { get; set; }
    public DateTime? EndingDate { get; set; }
    public string SalespersonCode { get; set; }
    public decimal Budget { get; set; }
}

public class CreateUpdateCampaignDto
{
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public DateTime? StartingDate { get; set; }
    public DateTime? EndingDate { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SalespersonCode { get; set; }

    public decimal Budget { get; set; }
}

public interface ICampaignAppService : ICrudAppService<CampaignDto, Guid, GetCodeTableListInput, CreateUpdateCampaignDto> { }

public class CampaignTargetDto : EntityDto<Guid>
{
    public string CampaignNo { get; set; }
    public string ContactNo { get; set; }
    public string ContactName { get; set; }
}

public class CreateUpdateCampaignTargetDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CampaignNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ContactNo { get; set; }
}

public class GetCampaignTargetListInput : ErpPagedListInput
{
    public string CampaignNo { get; set; }
}

public interface ICampaignTargetAppService : ICrudAppService<CampaignTargetDto, Guid, GetCampaignTargetListInput, CreateUpdateCampaignTargetDto> { }

public class InteractionLogEntryDto : FullAuditedEntityDto<Guid>
{
    public DateTime Date { get; set; }
    public string ContactNo { get; set; }
    public string ContactName { get; set; }
    public string OpportunityNo { get; set; }
    public string CampaignNo { get; set; }
    public string ActivityTypeCode { get; set; }
    public string Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Cost { get; set; }
    public string SalespersonCode { get; set; }
}

public class CreateUpdateInteractionLogEntryDto
{
    public DateTime Date { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ContactNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string OpportunityNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CampaignNo { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ActivityTypeCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public int DurationMinutes { get; set; }
    public decimal Cost { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string SalespersonCode { get; set; }
}

public class GetInteractionLogEntryListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string ContactNo { get; set; }
    public string OpportunityNo { get; set; }
}

public interface IInteractionLogEntryAppService : ICrudAppService<InteractionLogEntryDto, Guid, GetInteractionLogEntryListInput, CreateUpdateInteractionLogEntryDto> { }
