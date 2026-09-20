using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// The general journal. Mirrors Business Central page 39 "General Journal" together with the
/// check and post-batch codeunits behind its Post action.
/// </summary>
[Authorize(ErpPermissions.Journals.Default)]
public class GeneralJournalAppService : ErpAppService, IGeneralJournalAppService
{
    private readonly IRepository<GenJournalBatch, Guid> _batchRepository;
    private readonly IRepository<GenJournalLine, Guid> _lineRepository;
    private readonly IRepository<GenJournalTemplate, Guid> _templateRepository;
    private readonly GenJnlPostBatch _postBatch;
    private readonly RegisterEntryReader _entryReader;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public GeneralJournalAppService(
        IRepository<GenJournalBatch, Guid> batchRepository,
        IRepository<GenJournalLine, Guid> lineRepository,
        IRepository<GenJournalTemplate, Guid> templateRepository,
        GenJnlPostBatch postBatch,
        RegisterEntryReader entryReader,
        NoSeriesManager noSeriesManager,
        IUnitOfWorkManager unitOfWorkManager
    )
    {
        _batchRepository = batchRepository;
        _lineRepository = lineRepository;
        _templateRepository = templateRepository;
        _postBatch = postBatch;
        _entryReader = entryReader;
        _noSeriesManager = noSeriesManager;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task<ListResultDto<GenJournalBatchDto>> GetBatchesAsync(GetGenJournalBatchesInput input)
    {
        var templateName = input?.JournalTemplateName?.Trim().ToUpperInvariant();

        var batches = (await _batchRepository.GetListAsync())
            .Where(b => templateName.IsNullOrWhiteSpace() || b.JournalTemplateName == templateName)
            .OrderBy(b => b.JournalTemplateName)
            .ThenBy(b => b.Name)
            .ToList();

        var templates = await _templateRepository.GetListAsync();
        var lines = await _lineRepository.GetListAsync();

        var dtos = ObjectMapper.Map<List<GenJournalBatch>, List<GenJournalBatchDto>>(batches);

        for (var i = 0; i < batches.Count; i++)
        {
            var batchLines = lines.Where(l => l.GenJournalBatchId == batches[i].Id).ToList();

            dtos[i].LineCount = batchLines.Count;
            dtos[i].Balance = batchLines.Where(l => l.BalAccountNo == null).Sum(l => l.Amount);
            dtos[i].Recurring = templates.Any(t => t.Name == batches[i].JournalTemplateName && t.Recurring);
        }

        return new ListResultDto<GenJournalBatchDto>(dtos);
    }

    [Authorize(ErpPermissions.Journals.Manage)]
    public async Task<GenJournalBatchDto> CreateBatchAsync(CreateGenJournalBatchDto input)
    {
        var template = await GetTemplateAsync(input.JournalTemplateName);
        var name = input.Name.Trim().ToUpperInvariant();

        if (await _batchRepository.AnyAsync(b => b.JournalTemplateName == template.Name && b.Name == name))
        {
            throw new BusinessException(ErpErrorCodes.Journals.BatchNameAlreadyExists).WithData("name", name);
        }

        var batch = new GenJournalBatch(
            GuidGenerator.Create(),
            template.Name,
            input.Name,
            input.Description,
            input.ReasonCode,
            input.NoSeriesCode ?? template.NoSeriesCode,
            input.BalAccountType,
            input.BalAccountNo
        );

        await _batchRepository.InsertAsync(batch, autoSave: true);
        return await ToBatchDtoAsync(batch);
    }

    [Authorize(ErpPermissions.Journals.Manage)]
    public async Task<GenJournalBatchDto> UpdateBatchAsync(Guid id, UpdateGenJournalBatchDto input)
    {
        var batch = await _batchRepository.GetAsync(id);

        batch.Update(input.Description, input.ReasonCode, input.NoSeriesCode, input.BalAccountType, input.BalAccountNo);

        await _batchRepository.UpdateAsync(batch, autoSave: true);
        return await ToBatchDtoAsync(batch);
    }

    [Authorize(ErpPermissions.Journals.Manage)]
    public async Task DeleteBatchAsync(Guid id)
    {
        var batch = await _batchRepository.GetAsync(id);

        // Unposted lines are work in progress; deleting the batch around them loses it silently.
        if (await _lineRepository.AnyAsync(l => l.GenJournalBatchId == id))
        {
            throw new BusinessException(ErpErrorCodes.Journals.BatchNotEmpty).WithData("name", batch.Name);
        }

        await _batchRepository.DeleteAsync(batch);
    }

    public async Task<ListResultDto<GenJournalLineDto>> GetLinesAsync(Guid batchId)
    {
        var lines = (await _lineRepository.GetListAsync(l => l.GenJournalBatchId == batchId))
            .OrderBy(l => l.LineNo)
            .ToList();

        return new ListResultDto<GenJournalLineDto>(
            ObjectMapper.Map<List<GenJournalLine>, List<GenJournalLineDto>>(lines)
        );
    }

    [Authorize(ErpPermissions.Journals.Create)]
    public async Task<GenJournalLineDto> CreateLineAsync(CreateUpdateGenJournalLineDto input)
    {
        // Fails with a 404 rather than leaving an orphan line if the batch is not in this company.
        var batch = await _batchRepository.GetAsync(input.GenJournalBatchId);
        await EnsureRecurringIsAllowedAsync(batch, input.RecurringMethod);

        // Max + 1, not Count + 1: deleting a line must not make the next number collide.
        var existing = await _lineRepository.GetListAsync(l => l.GenJournalBatchId == input.GenJournalBatchId);
        var nextLineNo = existing.Count == 0 ? 1 : existing.Max(l => l.LineNo) + 1;

        var documentNo = await ResolveDocumentNoAsync(batch, input);

        var line = new GenJournalLine(
            GuidGenerator.Create(),
            input.GenJournalBatchId,
            nextLineNo,
            input.PostingDate,
            input.DocumentType,
            documentNo,
            input.AccountType,
            input.AccountNo,
            input.Description,
            input.Amount,
            input.BalAccountType ?? batch.BalAccountType,
            input.BalAccountNo ?? batch.BalAccountNo
        );

        ApplyOptionalFields(line, input, documentNo);

        await _lineRepository.InsertAsync(line, autoSave: true);
        return ObjectMapper.Map<GenJournalLine, GenJournalLineDto>(line);
    }

    [Authorize(ErpPermissions.Journals.Update)]
    public async Task<GenJournalLineDto> UpdateLineAsync(Guid id, CreateUpdateGenJournalLineDto input)
    {
        var line = await _lineRepository.GetAsync(id);
        var batch = await _batchRepository.GetAsync(line.GenJournalBatchId);
        await EnsureRecurringIsAllowedAsync(batch, input.RecurringMethod);

        ApplyOptionalFields(line, input, input.DocumentNo);

        await _lineRepository.UpdateAsync(line, autoSave: true);
        return ObjectMapper.Map<GenJournalLine, GenJournalLineDto>(line);
    }

    [Authorize(ErpPermissions.Journals.Delete)]
    public async Task DeleteLineAsync(Guid id)
    {
        await _lineRepository.DeleteAsync(id);
    }

    public async Task<JournalCheckResultDto> CheckAsync(Guid batchId)
    {
        await _batchRepository.GetAsync(batchId);

        return new JournalCheckResultDto { Messages = await _postBatch.CheckBatchAsync(batchId) };
    }

    /// <summary>
    /// Posts inside a transaction that is never committed, then reads back what it wrote.
    /// Mirrors Business Central's Preview Posting, and costs nothing but the rollback.
    /// </summary>
    [Authorize(ErpPermissions.Journals.Post)]
    public async Task<PostingPreviewDto> PreviewAsync(Guid batchId)
    {
        await _batchRepository.GetAsync(batchId);

        using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

        // A preview is only a preview because the transaction it runs in is thrown away. Without
        // one it would post for real, so it is refused rather than risked.
        if (unitOfWork.Options?.IsTransactional != true)
        {
            throw new BusinessException(ErpErrorCodes.Journals.PreviewNeedsATransaction);
        }

        var result = await _postBatch.PostAsync(batchId);
        var preview = await _entryReader.ReadAsync(result.RegisterNo);

        // Deliberately not completed: disposing rolls the whole preview back.
        return preview;
    }

    [Authorize(ErpPermissions.Journals.Post)]
    public async Task<GenJournalPostingResultDto> RunPostingAsync(Guid batchId)
    {
        await _batchRepository.GetAsync(batchId);

        var result = await _postBatch.PostAsync(batchId);
        return ObjectMapper.Map<GenJnlPostBatchResult, GenJournalPostingResultDto>(result);
    }

    private async Task<GenJournalTemplate> GetTemplateAsync(string name)
    {
        var normalized = name.Trim().ToUpperInvariant();

        return await _templateRepository.FirstOrDefaultAsync(t => t.Name == normalized)
            ?? throw new BusinessException(ErpErrorCodes.Journals.TemplateNotFound).WithData("name", normalized);
    }

    /// <summary>
    /// Recurring lines only make sense under a recurring template: elsewhere the posting run has
    /// nowhere to put the advanced date, and the line would be deleted after the first posting.
    /// </summary>
    private async Task EnsureRecurringIsAllowedAsync(GenJournalBatch batch, RecurringMethod method)
    {
        if (method == RecurringMethod.None)
        {
            return;
        }

        var template = await _templateRepository.FirstOrDefaultAsync(t => t.Name == batch.JournalTemplateName);
        if (template is not { Recurring: true })
        {
            throw new BusinessException(ErpErrorCodes.Journals.RecurringNotAllowedHere)
                .WithData("templateName", batch.JournalTemplateName);
        }
    }

    /// <summary>
    /// A blank document number is taken from the batch's number series, so a journal numbers
    /// itself the way a document does.
    /// </summary>
    private async Task<string> ResolveDocumentNoAsync(GenJournalBatch batch, CreateUpdateGenJournalLineDto input)
    {
        if (!input.DocumentNo.IsNullOrWhiteSpace())
        {
            return input.DocumentNo;
        }

        if (batch.NoSeriesCode.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.NoSeries.NumberRequired);
        }

        return await _noSeriesManager.GetNextNoAsync(batch.NoSeriesCode, input.PostingDate);
    }

    private static void ApplyOptionalFields(
        GenJournalLine line,
        CreateUpdateGenJournalLineDto input,
        string documentNo
    )
    {
        line.Update(
            input.PostingDate,
            input.DocumentType,
            documentNo,
            input.AccountType,
            input.AccountNo,
            input.Description,
            input.Amount,
            input.BalAccountType,
            input.BalAccountNo,
            input.DocumentDate,
            input.ExternalDocumentNo,
            input.AppliesToDocNo,
            input.Comment
        );

        line.SetRecurring(input.RecurringMethod, input.RecurringFrequency, input.ExpirationDate);
    }

    private async Task<GenJournalBatchDto> ToBatchDtoAsync(GenJournalBatch batch)
    {
        var dto = ObjectMapper.Map<GenJournalBatch, GenJournalBatchDto>(batch);
        var lines = await _lineRepository.GetListAsync(l => l.GenJournalBatchId == batch.Id);
        var template = await _templateRepository.FirstOrDefaultAsync(t => t.Name == batch.JournalTemplateName);

        dto.LineCount = lines.Count;
        dto.Balance = lines.Where(l => l.BalAccountNo == null).Sum(l => l.Amount);
        dto.Recurring = template?.Recurring ?? false;

        return dto;
    }
}
