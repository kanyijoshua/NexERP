using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.FixedAssets;

/// <summary>FA journal batches: posting them and filling them with the depreciation of a period.</summary>
[Authorize(ErpPermissions.FAJournals.Default)]
public class FAJournalBatchAppService
    : ErpCrudAppService<FAJournalBatch, FAJournalBatchDto, Guid, GetCodeTableListInput, CreateUpdateFAJournalBatchDto, CreateUpdateFAJournalBatchDto>,
        IFAJournalBatchAppService
{
    private readonly FAJnlPostBatch _postBatch;

    public FAJournalBatchAppService(IRepository<FAJournalBatch, Guid> repository, FAJnlPostBatch postBatch)
        : base(repository)
    {
        _postBatch = postBatch;
        GetPolicyName = ErpPermissions.FAJournals.Default;
        GetListPolicyName = ErpPermissions.FAJournals.Default;
        CreatePolicyName = ErpPermissions.FAJournals.Default;
        UpdatePolicyName = ErpPermissions.FAJournals.Default;
        DeletePolicyName = ErpPermissions.FAJournals.Default;
    }

    public override async Task<FAJournalBatchDto> CreateAsync(CreateUpdateFAJournalBatchDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureUniqueAsync(input.Name, null);
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<DepreciationBook>(input.DepreciationBookCode);
        var batch = new FAJournalBatch(GuidGenerator.Create(), input.Name, input.Description, input.DepreciationBookCode);
        await Repository.InsertAsync(batch, autoSave: true);
        return await MapToGetOutputDtoAsync(batch);
    }

    public override async Task<FAJournalBatchDto> UpdateAsync(Guid id, CreateUpdateFAJournalBatchDto input)
    {
        await CheckUpdatePolicyAsync();
        var batch = await GetEntityByIdAsync(id);
        await EnsureUniqueAsync(input.Name, id);
        await LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<DepreciationBook>(input.DepreciationBookCode);
        batch.SetName(input.Name);
        batch.Set(input.Description, input.DepreciationBookCode);
        await Repository.UpdateAsync(batch, autoSave: true);
        return await MapToGetOutputDtoAsync(batch);
    }

    [Authorize(ErpPermissions.FAJournals.Post)]
    public async Task<FAJournalPostingResultDto> RunPostingAsync(Guid id) => new() { LineCount = await _postBatch.PostAsync(id) };

    public async Task<int> CalculateDepreciationAsync(Guid id, CalculateDepreciationInput input)
    {
        await CheckUpdatePolicyAsync();
        var until = input.UntilDate == default ? Clock.Now.Date : input.UntilDate.Date;
        return await _postBatch.CalculateDepreciationAsync(id, until, input.PostingDate ?? until, input.DocumentNo.IsNullOrWhiteSpace() ? $"DEPR-{until:yyyyMM}" : input.DocumentNo, input.FANo);
    }

    private async Task EnsureUniqueAsync(string name, Guid? id)
    {
        var normalized = name?.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(b => b.Name == normalized && b.Id != id))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "FA Journal Batch").WithData("key", normalized);
        }
    }

    protected override async Task<IQueryable<FAJournalBatch>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), b => b.Name.ToLower().Contains(filter) || (b.Description != null && b.Description.ToLower().Contains(filter)));
    }

    protected override IQueryable<FAJournalBatch> ApplyDefaultSorting(IQueryable<FAJournalBatch> query) => query.OrderBy(b => b.Name);
}

[Authorize(ErpPermissions.FAJournals.Default)]
public class FAJournalLineAppService
    : ErpCrudAppService<FAJournalLine, FAJournalLineDto, Guid, GetFAJournalLineListInput, CreateUpdateFAJournalLineDto, CreateUpdateFAJournalLineDto>,
        IFAJournalLineAppService
{
    public FAJournalLineAppService(IRepository<FAJournalLine, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.FAJournals.Default;
        GetListPolicyName = ErpPermissions.FAJournals.Default;
        CreatePolicyName = ErpPermissions.FAJournals.Default;
        UpdatePolicyName = ErpPermissions.FAJournals.Default;
        DeletePolicyName = ErpPermissions.FAJournals.Default;
    }

    public override async Task<FAJournalLineDto> CreateAsync(CreateUpdateFAJournalLineDto input)
    {
        await CheckCreatePolicyAsync();
        await LazyServiceProvider.LazyGetRequiredService<IRepository<FAJournalBatch, Guid>>().GetAsync(input.FAJournalBatchId);
        var lines = await Repository.GetListAsync(l => l.FAJournalBatchId == input.FAJournalBatchId);
        var line = new FAJournalLine(GuidGenerator.Create(), input.FAJournalBatchId, (lines.Count == 0 ? 0 : lines.Max(l => l.LineNo)) + 10000, Date(input), input.DocumentNo, input.FANo, input.FAPostingType, input.Amount);
        await ApplyAsync(line, input);
        await Repository.InsertAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<FAJournalLineDto> UpdateAsync(Guid id, CreateUpdateFAJournalLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        await ApplyAsync(line, input);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    private async Task ApplyAsync(FAJournalLine line, CreateUpdateFAJournalLineDto input)
    {
        var post = LazyServiceProvider.LazyGetRequiredService<FAJnlPostLine>();
        var asset = await post.GetAssetAsync(input.FANo);
        await post.GetAssetBookAsync(asset.No, input.DepreciationBookCode);
        line.Update(Date(input), input.DocumentNo, asset.No, input.FAPostingType, input.Amount, input.Description ?? asset.Description);
        line.SetBook(input.DepreciationBookCode);
        line.SetBalancing(input.BalAccountType, input.BalAccountNo);
    }

    private DateTime Date(CreateUpdateFAJournalLineDto input) => input.PostingDate == default ? Clock.Now.Date : input.PostingDate;

    protected override async Task<IQueryable<FAJournalLine>> CreateFilteredQueryAsync(GetFAJournalLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.FAJournalBatchId.HasValue, l => l.FAJournalBatchId == input.FAJournalBatchId.Value);
    }

    protected override IQueryable<FAJournalLine> ApplyDefaultSorting(IQueryable<FAJournalLine> query) => query.OrderBy(l => l.LineNo);
}

/// <summary>What a fixed asset is carried at in each of its depreciation books.</summary>
[Authorize(ErpPermissions.FixedAssets.Default)]
public class FixedAssetValueAppService : ErpAppService, IFixedAssetValueAppService
{
    private readonly FAJnlPostLine _postLine;
    private readonly IRepository<FADepreciationBook, Guid> _assetBooks;

    public FixedAssetValueAppService(FAJnlPostLine postLine, IRepository<FADepreciationBook, Guid> assetBooks)
    {
        _postLine = postLine;
        _assetBooks = assetBooks;
    }

    public async Task<List<FABookValueDto>> GetBookValuesAsync(GetFABookValuesInput input)
    {
        var asset = await _postLine.GetAssetAsync(input.FANo);
        var result = new List<FABookValueDto>();
        foreach (var book in (await _assetBooks.GetListAsync(b => b.FANo == asset.No)).OrderBy(b => b.DepreciationBookCode))
        {
            var values = await _postLine.GetValuesAsync(asset.No, book.DepreciationBookCode);
            result.Add(new FABookValueDto
            {
                DepreciationBookCode = book.DepreciationBookCode,
                AcquisitionDate = book.AcquisitionDate,
                LastDepreciationDate = book.LastDepreciationDate,
                DisposalDate = book.DisposalDate,
                AcquisitionCost = values.AcquisitionCost,
                Depreciation = values.Depreciation,
                WriteDown = values.WriteDown,
                Appreciation = values.Appreciation,
                BookValue = values.BookValue,
                Proceeds = values.Proceeds,
                GainLoss = values.GainLoss,
            });
        }

        return result;
    }
}
