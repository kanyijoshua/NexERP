using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Kanban;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Crm;

[Authorize(ErpPermissions.CrmSetup.Default)]
public class MarketingSetupAppService : ErpAppService, IMarketingSetupAppService
{
    private readonly IRepository<MarketingSetup, Guid> _repository;

    public MarketingSetupAppService(IRepository<MarketingSetup, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<MarketingSetupDto> GetAsync() => Map(await GetOrCreateAsync());

    [Authorize(ErpPermissions.CrmSetup.Update)]
    public async Task<MarketingSetupDto> UpdateAsync(MarketingSetupDto input)
    {
        await LazyServiceProvider.LazyGetRequiredService<NoSeriesCodeValidator>().EnsureExistAsync(input.ContactNos, input.OpportunityNos, input.CampaignNos);
        var setup = await GetOrCreateAsync();
        setup.SetNumbering(input.ContactNos, input.OpportunityNos, input.CampaignNos);
        setup.SetCustomerDefaults(input.CustomerPostingGroup, input.GenBusPostingGroup, input.VatBusPostingGroup, input.PaymentTermsCode);
        await _repository.UpdateAsync(setup, autoSave: true);
        return Map(setup);
    }

    private async Task<MarketingSetup> GetOrCreateAsync() =>
        await _repository.FirstOrDefaultAsync() ?? await _repository.InsertAsync(new MarketingSetup(GuidGenerator.Create()), autoSave: true);

    private static MarketingSetupDto Map(MarketingSetup s) => new()
    {
        ContactNos = s.ContactNos,
        OpportunityNos = s.OpportunityNos,
        CampaignNos = s.CampaignNos,
        CustomerPostingGroup = s.CustomerPostingGroup,
        GenBusPostingGroup = s.GenBusPostingGroup,
        VatBusPostingGroup = s.VatBusPostingGroup,
        PaymentTermsCode = s.PaymentTermsCode,
    };
}

[Authorize(ErpPermissions.Crm.Default)]
public class ContactAppService
    : ErpCrudAppService<Contact, ContactDto, Guid, GetContactListInput, CreateUpdateContactDto, CreateUpdateContactDto>,
        IContactAppService
{
    private OpportunityManager Manager => LazyServiceProvider.LazyGetRequiredService<OpportunityManager>();

    public ContactAppService(IRepository<Contact, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Crm.Default;
        GetListPolicyName = ErpPermissions.Crm.Default;
        CreatePolicyName = ErpPermissions.Crm.Create;
        UpdatePolicyName = ErpPermissions.Crm.Update;
        DeletePolicyName = ErpPermissions.Crm.Delete;
    }

    public override async Task<ContactDto> CreateAsync(CreateUpdateContactDto input)
    {
        await CheckCreatePolicyAsync();
        var contact = await Manager.NewContactAsync(input.Name, input.Type, input.Email, input.PhoneNo, input.SalespersonCode, input.No);
        await ApplyAsync(contact, input);
        await Repository.UpdateAsync(contact, autoSave: true);
        return await MapToGetOutputDtoAsync(contact);
    }

    public override async Task<ContactDto> UpdateAsync(Guid id, CreateUpdateContactDto input)
    {
        await CheckUpdatePolicyAsync();
        var contact = await GetEntityByIdAsync(id);
        contact.SetName(input.Name);
        contact.SetType(input.Type);
        await ApplyAsync(contact, input);
        await Repository.UpdateAsync(contact, autoSave: true);
        return await MapToGetOutputDtoAsync(contact);
    }

    [Authorize(ErpPermissions.Crm.Update)]
    public async Task<ContactDto> MakeCustomerAsync(Guid id)
    {
        var contact = await GetEntityByIdAsync(id);
        await Manager.EnsureCustomerAsync(contact);
        return await MapToGetOutputDtoAsync(contact);
    }

    private async Task ApplyAsync(Contact contact, CreateUpdateContactDto input)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        await codes.EnsureExistsAsync<SalespersonPurchaser>(input.SalespersonCode);

        Contact company = null;
        if (input.Type == ContactType.Person && !input.CompanyNo.IsNullOrWhiteSpace())
        {
            company = await Manager.GetContactAsync(input.CompanyNo);
        }

        contact.SetCompany(company?.No, company?.Name);
        contact.SetCommunication(input.JobTitle, input.Email, input.PhoneNo, input.MobilePhoneNo);
        contact.SetAddress(input.Address, input.City, input.CountryRegionCode);
        contact.SetSalesperson(input.SalespersonCode);
        contact.SetVendor(input.VendorNo);
    }

    protected override async Task<IQueryable<Contact>> CreateFilteredQueryAsync(GetContactListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        var company = input.CompanyNo?.Trim().ToUpperInvariant();
        return query
            .WhereIf(input.Type.HasValue, c => c.Type == input.Type.Value)
            .WhereIf(!company.IsNullOrEmpty(), c => c.CompanyNo == company)
            .WhereIf(!filter.IsNullOrEmpty(), c => c.No.ToLower().Contains(filter) || c.Name.ToLower().Contains(filter) || (c.Email != null && c.Email.ToLower().Contains(filter)));
    }

    protected override IQueryable<Contact> ApplyDefaultSorting(IQueryable<Contact> query) => query.OrderBy(c => c.No);
}

[Authorize(ErpPermissions.Crm.Default)]
public class OpportunityAppService
    : ErpCrudAppService<Opportunity, OpportunityDto, Guid, GetOpportunityListInput, CreateUpdateOpportunityDto, CreateUpdateOpportunityDto>,
        IOpportunityAppService
{
    private OpportunityManager Manager => LazyServiceProvider.LazyGetRequiredService<OpportunityManager>();

    public OpportunityAppService(IRepository<Opportunity, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Crm.Default;
        GetListPolicyName = ErpPermissions.Crm.Default;
        CreatePolicyName = ErpPermissions.Crm.Create;
        UpdatePolicyName = ErpPermissions.Crm.Update;
        DeletePolicyName = ErpPermissions.Crm.Delete;
    }

    public override async Task<OpportunityDto> CreateAsync(CreateUpdateOpportunityDto input)
    {
        await CheckCreatePolicyAsync();
        var opportunity = await Manager.CreateAsync(input.No, input.Kind, input.Description, input.ContactNo);
        await ApplyAsync(opportunity, input);
        await Repository.UpdateAsync(opportunity, autoSave: true);
        return await MapToGetOutputDtoAsync(opportunity);
    }

    public override async Task<OpportunityDto> UpdateAsync(Guid id, CreateUpdateOpportunityDto input)
    {
        await CheckUpdatePolicyAsync();
        var opportunity = await GetEntityByIdAsync(id);
        opportunity.SetDescription(input.Description);
        opportunity.SetContact(input.ContactNo.IsNullOrWhiteSpace() ? null : await Manager.GetContactAsync(input.ContactNo));
        await ApplyAsync(opportunity, input);
        await Repository.UpdateAsync(opportunity, autoSave: true);
        return await MapToGetOutputDtoAsync(opportunity);
    }

    [Authorize(ErpPermissions.Crm.Update)]
    public async Task<OpportunityDto> MoveAsync(Guid id, MoveOpportunityInput input) => await MapToGetOutputDtoAsync(await Manager.MoveToStageAsync(id, input.StageId, input.CloseCode));

    [Authorize(ErpPermissions.Crm.Update)]
    public async Task<OpportunityDto> ConvertAsync(Guid id) => await MapToGetOutputDtoAsync(await Manager.ConvertLeadAsync(id));

    [Authorize(ErpPermissions.Crm.Update)]
    public async Task<OpportunityDto> WinAsync(Guid id, CloseOpportunityInput input) => await MapToGetOutputDtoAsync(await Manager.WinAsync(id, input.CloseCode));

    [Authorize(ErpPermissions.Crm.Update)]
    public async Task<OpportunityDto> LoseAsync(Guid id, CloseOpportunityInput input) => await MapToGetOutputDtoAsync(await Manager.LoseAsync(id, input.CloseCode));

    [Authorize(ErpPermissions.Crm.Update)]
    public async Task<OpportunityDto> ReopenAsync(Guid id) => await MapToGetOutputDtoAsync(await Manager.ReopenAsync(id));

    [Authorize(ErpPermissions.Crm.Update)]
    public async Task<OpportunityDto> MakeSalesQuoteAsync(Guid id)
    {
        var opportunity = await GetEntityByIdAsync(id);
        if (opportunity.ContactNo == null)
        {
            throw new BusinessException(ErpErrorCodes.Crm.NoContact).WithData("no", opportunity.No);
        }

        var customer = await Manager.EnsureCustomerAsync(await Manager.GetContactAsync(opportunity.ContactNo));
        var quote = await LazyServiceProvider.LazyGetRequiredService<ISalesDocumentAppService>().CreateAsync(new CreateUpdateSalesHeaderDto
        {
            DocumentType = SalesDocumentType.Quote,
            CustomerId = customer.Id,
            PostingDate = Clock.Now.Date,
            ExternalDocumentNo = opportunity.No,
        });
        await Manager.LinkQuoteAsync(id, quote.No);
        return await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id));
    }

    public async Task<List<OpportunityEntryDto>> GetHistoryAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        var entries = await LazyServiceProvider.LazyGetRequiredService<IRepository<OpportunityEntry, Guid>>().GetListAsync(e => e.OpportunityId == id);
        return entries.OrderBy(e => e.Date).Select(e => new OpportunityEntryDto
        {
            Id = e.Id, Date = e.Date, StageName = e.StageName, Status = e.Status, Probability = e.Probability, ExpectedRevenue = e.ExpectedRevenue, Note = e.Note,
        }).ToList();
    }

    public async Task<PipelineDto> GetPipelineAsync(GetPipelineInput input)
    {
        await CheckGetListPolicyAsync();
        var pipeline = input.Pipeline == OpportunityManager.LeadPipeline ? OpportunityManager.LeadPipeline : OpportunityManager.OpportunityPipeline;
        var stages = await Manager.GetPipelineAsync(pipeline);
        var kind = pipeline == OpportunityManager.LeadPipeline ? OpportunityKind.Lead : OpportunityKind.Opportunity;
        var cards = await Repository.GetListAsync(o => o.Kind == kind);
        return new PipelineDto
        {
            Pipeline = pipeline,
            Stages = stages.Select(s => new PipelineStageDto
            {
                StageId = s.StageId, Name = s.Name, Sequence = s.Sequence, IsWon = s.IsWon, IsLost = s.IsLost, Count = s.Count, ExpectedRevenue = s.ExpectedRevenue, WeightedRevenue = s.WeightedRevenue,
            }).ToList(),
            Cards = await MapToGetListOutputDtosAsync(cards.OrderByDescending(o => o.Priority).ThenBy(o => o.ExpectedCloseDate).ToList()),
        };
    }

    private async Task ApplyAsync(Opportunity opportunity, CreateUpdateOpportunityDto input)
    {
        var codes = LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();
        await codes.EnsureExistsAsync<SalespersonPurchaser>(input.SalespersonCode);
        if (!input.CampaignNo.IsNullOrWhiteSpace())
        {
            var no = input.CampaignNo.Trim().ToUpperInvariant();
            if (!await LazyServiceProvider.LazyGetRequiredService<IRepository<Campaign, Guid>>().AnyAsync(c => c.No == no))
            {
                throw new BusinessException(ErpErrorCodes.Crm.CampaignNotFound).WithData("no", no);
            }
        }

        opportunity.SetProspect(input.ContactCompanyName, input.Email, input.PhoneNo);
        opportunity.SetPlanning(input.SalespersonCode, input.CampaignNo, input.Priority, input.ExpectedRevenue, input.ExpectedCloseDate);
        if (input.Probability.HasValue)
        {
            opportunity.SetProbability(input.Probability.Value);
        }
    }

    protected override async Task<IQueryable<Opportunity>> CreateFilteredQueryAsync(GetOpportunityListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(input.Kind.HasValue, o => o.Kind == input.Kind.Value)
            .WhereIf(input.Status.HasValue, o => o.Status == input.Status.Value)
            .WhereIf(!input.ContactNo.IsNullOrWhiteSpace(), o => o.ContactNo == input.ContactNo)
            .WhereIf(!input.SalespersonCode.IsNullOrWhiteSpace(), o => o.SalespersonCode == input.SalespersonCode)
            .WhereIf(!filter.IsNullOrEmpty(), o => o.No.ToLower().Contains(filter) || o.Description.ToLower().Contains(filter) || (o.ContactName != null && o.ContactName.ToLower().Contains(filter)));
    }

    protected override IQueryable<Opportunity> ApplyDefaultSorting(IQueryable<Opportunity> query) => query.OrderByDescending(o => o.CreationTime);
}

public class CloseOpportunityCodeAppService : CodeTableAppServiceBase<CloseOpportunityCode, CloseOpportunityCodeDto, CreateUpdateCloseOpportunityCodeDto>, ICloseOpportunityCodeAppService
{
    public CloseOpportunityCodeAppService(IRepository<CloseOpportunityCode, Guid> repository)
        : base(repository, ErpPermissions.CrmSetup.Default) { }

    protected override CloseOpportunityCode NewEntity(Guid id, CreateUpdateCloseOpportunityCodeDto input) => new(id, input.Code, input.Description, input.Type);

    protected override Task ApplyAsync(CloseOpportunityCode entity, CreateUpdateCloseOpportunityCodeDto input)
    {
        entity.SetType(input.Type);
        return Task.CompletedTask;
    }
}

public class ActivityTypeAppService : CodeTableAppServiceBase<ActivityType, ActivityTypeDto, CreateUpdateActivityTypeDto>, IActivityTypeAppService
{
    public ActivityTypeAppService(IRepository<ActivityType, Guid> repository)
        : base(repository, ErpPermissions.CrmSetup.Default) { }

    protected override ActivityType NewEntity(Guid id, CreateUpdateActivityTypeDto input) => new(id, input.Code, input.Description, input.Icon);

    protected override Task ApplyAsync(ActivityType entity, CreateUpdateActivityTypeDto input)
    {
        entity.SetIcon(input.Icon);
        return Task.CompletedTask;
    }
}

/// <summary>The stages of the lead and opportunity pipelines.</summary>
[Authorize(ErpPermissions.CrmSetup.Default)]
public class PipelineStageAppService
    : ErpCrudAppService<KanbanStage, PipelineStageSetupDto, Guid, GetPipelineStageListInput, CreateUpdatePipelineStageDto, CreateUpdatePipelineStageDto>,
        IPipelineStageAppService
{
    public PipelineStageAppService(IRepository<KanbanStage, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.CrmSetup.Default;
        GetListPolicyName = ErpPermissions.CrmSetup.Default;
        CreatePolicyName = ErpPermissions.CrmSetup.Create;
        UpdatePolicyName = ErpPermissions.CrmSetup.Update;
        DeletePolicyName = ErpPermissions.CrmSetup.Delete;
    }

    public override async Task<PipelineStageSetupDto> CreateAsync(CreateUpdatePipelineStageDto input)
    {
        await CheckCreatePolicyAsync();
        var stage = new KanbanStage(GuidGenerator.Create(), input.PipelineType, input.Sequence, input.Name);
        stage.Update(input.PipelineType, input.Sequence, input.Name, input.FoldedInKanban, input.IsWonStage, input.IsLostStage, input.DefaultProbability);
        await Repository.InsertAsync(stage, autoSave: true);
        return await MapToGetOutputDtoAsync(stage);
    }

    public override async Task<PipelineStageSetupDto> UpdateAsync(Guid id, CreateUpdatePipelineStageDto input)
    {
        await CheckUpdatePolicyAsync();
        var stage = await GetEntityByIdAsync(id);
        stage.Update(input.PipelineType, input.Sequence, input.Name, input.FoldedInKanban, input.IsWonStage, input.IsLostStage, input.DefaultProbability);
        await Repository.UpdateAsync(stage, autoSave: true);
        return await MapToGetOutputDtoAsync(stage);
    }

    protected override async Task<IQueryable<KanbanStage>> CreateFilteredQueryAsync(GetPipelineStageListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(!input.PipelineType.IsNullOrWhiteSpace(), s => s.PipelineType == input.PipelineType);
    }

    protected override IQueryable<KanbanStage> ApplyDefaultSorting(IQueryable<KanbanStage> query) => query.OrderBy(s => s.PipelineType).ThenBy(s => s.Sequence);
}

[Authorize(ErpPermissions.Crm.Default)]
public class CampaignAppService
    : ErpCrudAppService<Campaign, CampaignDto, Guid, GetCodeTableListInput, CreateUpdateCampaignDto, CreateUpdateCampaignDto>,
        ICampaignAppService
{
    public CampaignAppService(IRepository<Campaign, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Crm.Default;
        GetListPolicyName = ErpPermissions.Crm.Default;
        CreatePolicyName = ErpPermissions.Crm.Create;
        UpdatePolicyName = ErpPermissions.Crm.Update;
        DeletePolicyName = ErpPermissions.Crm.Delete;
    }

    public override async Task<CampaignDto> CreateAsync(CreateUpdateCampaignDto input)
    {
        await CheckCreatePolicyAsync();
        var setup = await LazyServiceProvider.LazyGetRequiredService<IRepository<MarketingSetup, Guid>>().FirstOrDefaultAsync();
        var no = await LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>().ResolveNoAsync(setup?.CampaignNos, input.No, Clock.Now);
        var campaign = new Campaign(GuidGenerator.Create(), no, input.Description);
        campaign.Set(input.Description, input.StartingDate, input.EndingDate, input.SalespersonCode, input.Budget);
        await Repository.InsertAsync(campaign, autoSave: true);
        return await MapToGetOutputDtoAsync(campaign);
    }

    public override async Task<CampaignDto> UpdateAsync(Guid id, CreateUpdateCampaignDto input)
    {
        await CheckUpdatePolicyAsync();
        var campaign = await GetEntityByIdAsync(id);
        campaign.Set(input.Description, input.StartingDate, input.EndingDate, input.SalespersonCode, input.Budget);
        await Repository.UpdateAsync(campaign, autoSave: true);
        return await MapToGetOutputDtoAsync(campaign);
    }

    protected override async Task<IQueryable<Campaign>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), c => c.No.ToLower().Contains(filter) || (c.Description != null && c.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<Campaign> ApplyDefaultSorting(IQueryable<Campaign> query) => query.OrderBy(c => c.No);
}

[Authorize(ErpPermissions.Crm.Default)]
public class CampaignTargetAppService
    : ErpCrudAppService<CampaignTarget, CampaignTargetDto, Guid, GetCampaignTargetListInput, CreateUpdateCampaignTargetDto, CreateUpdateCampaignTargetDto>,
        ICampaignTargetAppService
{
    public CampaignTargetAppService(IRepository<CampaignTarget, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Crm.Default;
        GetListPolicyName = ErpPermissions.Crm.Default;
        CreatePolicyName = ErpPermissions.Crm.Update;
        UpdatePolicyName = ErpPermissions.Crm.Update;
        DeletePolicyName = ErpPermissions.Crm.Update;
    }

    public override async Task<CampaignTargetDto> CreateAsync(CreateUpdateCampaignTargetDto input)
    {
        await CheckCreatePolicyAsync();
        var campaignNo = input.CampaignNo.Trim().ToUpperInvariant();
        if (!await LazyServiceProvider.LazyGetRequiredService<IRepository<Campaign, Guid>>().AnyAsync(c => c.No == campaignNo))
        {
            throw new BusinessException(ErpErrorCodes.Crm.CampaignNotFound).WithData("no", campaignNo);
        }

        var contact = await LazyServiceProvider.LazyGetRequiredService<OpportunityManager>().GetContactAsync(input.ContactNo);
        if (await Repository.AnyAsync(t => t.CampaignNo == campaignNo && t.ContactNo == contact.No))
        {
            throw new BusinessException(ErpErrorCodes.Crm.AlreadyTargeted).WithData("contactNo", contact.No).WithData("campaignNo", campaignNo);
        }

        var target = new CampaignTarget(GuidGenerator.Create(), campaignNo, contact);
        await Repository.InsertAsync(target, autoSave: true);
        return await MapToGetOutputDtoAsync(target);
    }

    public override Task<CampaignTargetDto> UpdateAsync(Guid id, CreateUpdateCampaignTargetDto input) =>
        throw new BusinessException(ErpErrorCodes.Crm.AlreadyTargeted).WithData("contactNo", input.ContactNo ?? "").WithData("campaignNo", input.CampaignNo ?? "");

    protected override async Task<IQueryable<CampaignTarget>> CreateFilteredQueryAsync(GetCampaignTargetListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var no = input.CampaignNo?.Trim().ToUpperInvariant();
        return query.WhereIf(!no.IsNullOrEmpty(), t => t.CampaignNo == no);
    }

    protected override IQueryable<CampaignTarget> ApplyDefaultSorting(IQueryable<CampaignTarget> query) => query.OrderBy(t => t.ContactNo);
}

[Authorize(ErpPermissions.Crm.Default)]
public class InteractionLogEntryAppService
    : ErpCrudAppService<InteractionLogEntry, InteractionLogEntryDto, Guid, GetInteractionLogEntryListInput, CreateUpdateInteractionLogEntryDto, CreateUpdateInteractionLogEntryDto>,
        IInteractionLogEntryAppService
{
    public InteractionLogEntryAppService(IRepository<InteractionLogEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Crm.Default;
        GetListPolicyName = ErpPermissions.Crm.Default;
        CreatePolicyName = ErpPermissions.Crm.Create;
        UpdatePolicyName = ErpPermissions.Crm.Update;
        DeletePolicyName = ErpPermissions.Crm.Delete;
    }

    public override async Task<InteractionLogEntryDto> CreateAsync(CreateUpdateInteractionLogEntryDto input)
    {
        await CheckCreatePolicyAsync();
        var contact = await LazyServiceProvider.LazyGetRequiredService<OpportunityManager>().GetContactAsync(input.ContactNo);
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<ActivityType>(input.ActivityTypeCode);
        var entry = new InteractionLogEntry(GuidGenerator.Create(), input.Date == default ? Clock.Now : input.Date, contact, input.ActivityTypeCode, input.Description);
        entry.SetDetails(input.OpportunityNo, input.CampaignNo, input.DurationMinutes, input.Cost, input.SalespersonCode);
        await Repository.InsertAsync(entry, autoSave: true);
        return await MapToGetOutputDtoAsync(entry);
    }

    public override async Task<InteractionLogEntryDto> UpdateAsync(Guid id, CreateUpdateInteractionLogEntryDto input)
    {
        await CheckUpdatePolicyAsync();
        var entry = await GetEntityByIdAsync(id);
        entry.SetDetails(input.OpportunityNo, input.CampaignNo, input.DurationMinutes, input.Cost, input.SalespersonCode);
        await Repository.UpdateAsync(entry, autoSave: true);
        return await MapToGetOutputDtoAsync(entry);
    }

    protected override async Task<IQueryable<InteractionLogEntry>> CreateFilteredQueryAsync(GetInteractionLogEntryListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query
            .WhereIf(!input.ContactNo.IsNullOrWhiteSpace(), e => e.ContactNo == input.ContactNo)
            .WhereIf(!input.OpportunityNo.IsNullOrWhiteSpace(), e => e.OpportunityNo == input.OpportunityNo)
            .WhereIf(!filter.IsNullOrEmpty(), e => e.Description.ToLower().Contains(filter) || e.ContactNo.ToLower().Contains(filter));
    }

    protected override IQueryable<InteractionLogEntry> ApplyDefaultSorting(IQueryable<InteractionLogEntry> query) => query.OrderByDescending(e => e.Date);
}
