using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Crm;

[Authorize(ErpPermissions.Marketing.Default)]
public class SegmentAppService : ErpTableAppService<Segment, SegmentDto, GetSegmentListInput, CreateUpdateSegmentDto>, ISegmentAppService
{
    private MarketingManager Manager => LazyServiceProvider.LazyGetRequiredService<MarketingManager>();

    public SegmentAppService(IRepository<Segment, Guid> repository)
        : base(repository, ErpPermissions.Marketing.Default) { }

    protected override async Task<Segment> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    public override async Task<SegmentDto> CreateAsync(CreateUpdateSegmentDto input)
    {
        await CheckCreatePolicyAsync();
        var no = input.No.IsNullOrWhiteSpace() ? await Manager.NextNoAsync() : input.No.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(s => s.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", no);
        }

        var segment = new Segment(GuidGenerator.Create(), no, input.Description);
        await Repository.InsertAsync(segment, autoSave: true);
        return await MapToGetOutputDtoAsync(segment);
    }

    public override async Task<SegmentDto> UpdateAsync(Guid id, CreateUpdateSegmentDto input)
    {
        await CheckUpdatePolicyAsync();
        var segment = await GetEntityByIdAsync(id);
        segment.SetDescription(input.Description);
        await Repository.UpdateAsync(segment, autoSave: true);
        return await MapToGetOutputDtoAsync(segment);
    }

    [Authorize(ErpPermissions.Marketing.Update)]
    public async Task<SegmentResultDto> PickContactsAsync(Guid id, PickSegmentContactsInput input) =>
        new() { Count = await Manager.AddContactsAsync(id, input.City, input.SalespersonCode, input.Type, input.OnlyWithEmail) };

    [Authorize(ErpPermissions.Marketing.Update)]
    public async Task<SegmentResultDto> ApplyToCampaignAsync(Guid id, ApplySegmentInput input) =>
        new() { Count = await Manager.ApplyToCampaignAsync(id, input.CampaignNo) };

    protected override async Task<IQueryable<Segment>> CreateFilteredQueryAsync(GetSegmentListInput input)
    {
        var query = await Repository.WithDetailsAsync(s => s.Lines);
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), s => s.No.ToLower().Contains(filter) || (s.Description != null && s.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<Segment> ApplyDefaultSorting(IQueryable<Segment> query) => query.OrderByDescending(s => s.No);
}

[Authorize(ErpPermissions.Marketing.Default)]
public class SegmentLineAppService : ErpReadOnlyAppService<SegmentLine, SegmentLineDto, Guid, GetSegmentLineListInput>, ISegmentLineAppService
{
    private readonly IRepository<SegmentLine, Guid> _lines;

    public SegmentLineAppService(IRepository<SegmentLine, Guid> repository)
        : base(repository)
    {
        _lines = repository;
        GetPolicyName = ErpPermissions.Marketing.Default;
        GetListPolicyName = ErpPermissions.Marketing.Default;
    }

    [Authorize(ErpPermissions.Marketing.Update)]
    public async Task DeleteAsync(Guid id) => await _lines.DeleteAsync(id, autoSave: true);

    protected override async Task<IQueryable<SegmentLine>> CreateFilteredQueryAsync(GetSegmentLineListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        return query.WhereIf(input.SegmentId.HasValue, l => l.SegmentId == input.SegmentId.Value);
    }

    protected override IQueryable<SegmentLine> ApplyDefaultSorting(IQueryable<SegmentLine> query) => query.OrderBy(l => l.ContactNo);
}

[Authorize(ErpPermissions.Marketing.Default)]
public class CampaignMailingAppService : ErpReadOnlyAppService<CampaignMailingEntry, CampaignMailingEntryDto, Guid, GetCampaignMailingListInput>, ICampaignMailingAppService
{
    public CampaignMailingAppService(IRepository<CampaignMailingEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.Marketing.Default;
        GetListPolicyName = ErpPermissions.Marketing.Default;
    }

    [Authorize(ErpPermissions.Marketing.Send)]
    public async Task<MailingResultDto> SendAsync(SendMailingInput input)
    {
        var result = await LazyServiceProvider.LazyGetRequiredService<MarketingManager>().SendMailingAsync(input.CampaignNo, input.Subject, input.BodyHtml);
        return new MailingResultDto { Sent = result.Sent, Failed = result.Failed, WithoutEmail = result.WithoutEmail };
    }

    protected override async Task<IQueryable<CampaignMailingEntry>> CreateFilteredQueryAsync(GetCampaignMailingListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var campaign = input.CampaignNo?.Trim().ToUpperInvariant();
        return query.WhereIf(!campaign.IsNullOrEmpty(), e => e.CampaignNo == campaign);
    }

    protected override IQueryable<CampaignMailingEntry> ApplyDefaultSorting(IQueryable<CampaignMailingEntry> query) => query.OrderByDescending(e => e.SentAt);
}
