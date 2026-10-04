using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>Report Selections.</summary>
public class ReportSelectionAppService : ErpTableAppService<ReportSelection, ReportSelectionDto, GetReportSelectionListInput, CreateUpdateReportSelectionDto>, IReportSelectionAppService
{
    public ReportSelectionAppService(IRepository<ReportSelection, Guid> repository)
        : base(repository, ErpPermissions.ReportSelections.Default) { }

    public override async Task<ReportSelectionDto> CreateAsync(CreateUpdateReportSelectionDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(input.Usage, CodeTableEntity.NormalizeCode(input.Sequence), null);

        var entity = new ReportSelection(GuidGenerator.Create(), input.Usage, CodeTableEntity.NormalizeCode(input.Sequence));
        entity.Set(
            input.ReportId,
            input.CustomReportLayoutCode,
            input.UseForEmailAttachment,
            input.UseForEmailBody,
            input.EmailBodyLayoutCode,
            input.ReportLayoutName
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<ReportSelectionDto> UpdateAsync(Guid id, CreateUpdateReportSelectionDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(input.Usage, CodeTableEntity.NormalizeCode(input.Sequence), id);

        entity.SetKey(input.Usage, CodeTableEntity.NormalizeCode(input.Sequence));
        entity.Set(
            input.ReportId,
            input.CustomReportLayoutCode,
            input.UseForEmailAttachment,
            input.UseForEmailBody,
            input.EmailBodyLayoutCode,
            input.ReportLayoutName
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<ReportSelection>> CreateFilteredQueryAsync(GetReportSelectionListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.Sequence.ToLower().Contains(filter)
                    || (x.CustomReportLayoutCode != null && x.CustomReportLayoutCode.ToLower().Contains(filter))
                    || (x.EmailBodyLayoutCode != null && x.EmailBodyLayoutCode.ToLower().Contains(filter))
                    || (x.ReportLayoutName != null && x.ReportLayoutName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<ReportSelection> ApplyDefaultSorting(IQueryable<ReportSelection> query) =>
        query.OrderBy(x => x.Usage).ThenBy(x => x.Sequence);

    private static Task ValidateAsync(CreateUpdateReportSelectionDto input) => Task.CompletedTask;

    private async Task EnsureKeyIsUniqueAsync(ReportSelectionUsage usage, string sequence, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.Usage == usage && x.Sequence == sequence && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Report Selection").WithData("key", usage.ToString() + " " + sequence);
        }
    }
}

/// <summary>Custom Report Selections.</summary>
public class CustomReportSelectionAppService : ErpTableAppService<CustomReportSelection, CustomReportSelectionDto, GetCustomReportSelectionListInput, CreateUpdateCustomReportSelectionDto>, ICustomReportSelectionAppService
{
    public CustomReportSelectionAppService(IRepository<CustomReportSelection, Guid> repository)
        : base(repository, ErpPermissions.ReportSelections.Default) { }

    public override async Task<CustomReportSelectionDto> CreateAsync(CreateUpdateCustomReportSelectionDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(input.SourceType, CodeTableEntity.NormalizeCode(input.SourceNo), input.Usage, input.Sequence, null);

        var entity = new CustomReportSelection(GuidGenerator.Create(), input.SourceType, CodeTableEntity.NormalizeCode(input.SourceNo), input.Usage, input.Sequence);
        entity.Set(
            input.ReportId,
            input.CustomReportLayoutCode,
            input.SendToEmail,
            input.UseForEmailAttachment,
            input.UseForEmailBody,
            input.EmailBodyLayoutCode,
            input.UseEmailFromContact
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<CustomReportSelectionDto> UpdateAsync(Guid id, CreateUpdateCustomReportSelectionDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(input.SourceType, CodeTableEntity.NormalizeCode(input.SourceNo), input.Usage, input.Sequence, id);

        entity.SetKey(input.SourceType, CodeTableEntity.NormalizeCode(input.SourceNo), input.Usage, input.Sequence);
        entity.Set(
            input.ReportId,
            input.CustomReportLayoutCode,
            input.SendToEmail,
            input.UseForEmailAttachment,
            input.UseForEmailBody,
            input.EmailBodyLayoutCode,
            input.UseEmailFromContact
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<CustomReportSelection>> CreateFilteredQueryAsync(GetCustomReportSelectionListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var sourceNo = input.SourceNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!sourceNo.IsNullOrEmpty(), x => x.SourceNo == sourceNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.SourceNo.ToLower().Contains(filter)
                    || (x.CustomReportLayoutCode != null && x.CustomReportLayoutCode.ToLower().Contains(filter))
                    || (x.SendToEmail != null && x.SendToEmail.ToLower().Contains(filter))
                    || (x.EmailBodyLayoutCode != null && x.EmailBodyLayoutCode.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<CustomReportSelection> ApplyDefaultSorting(IQueryable<CustomReportSelection> query) =>
        query.OrderBy(x => x.SourceType).ThenBy(x => x.SourceNo).ThenBy(x => x.Usage).ThenBy(x => x.Sequence);

    private static Task ValidateAsync(CreateUpdateCustomReportSelectionDto input) => Task.CompletedTask;

    private async Task EnsureKeyIsUniqueAsync(int sourceType, string sourceNo, ReportSelectionUsage usage, int sequence, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.SourceType == sourceType && x.SourceNo == sourceNo && x.Usage == usage && x.Sequence == sequence && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Custom Report Selection").WithData("key", sourceType.ToString() + " " + sourceNo + " " + usage.ToString() + " " + sequence.ToString());
        }
    }
}
