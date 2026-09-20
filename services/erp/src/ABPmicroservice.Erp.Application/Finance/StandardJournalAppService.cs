using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Saved sets of journal lines that can be dropped into a batch again.
/// Mirrors Business Central's Standard General Journals (tables 750 and 751).
/// </summary>
[Authorize(ErpPermissions.Journals.Default)]
public class StandardJournalAppService : ErpAppService, IStandardJournalAppService
{
    private readonly IRepository<StandardGeneralJournal, Guid> _standardJournalRepository;
    private readonly IRepository<StandardGeneralJournalLine, Guid> _standardLineRepository;
    private readonly IRepository<GenJournalBatch, Guid> _batchRepository;
    private readonly IRepository<GenJournalLine, Guid> _lineRepository;

    public StandardJournalAppService(
        IRepository<StandardGeneralJournal, Guid> standardJournalRepository,
        IRepository<StandardGeneralJournalLine, Guid> standardLineRepository,
        IRepository<GenJournalBatch, Guid> batchRepository,
        IRepository<GenJournalLine, Guid> lineRepository
    )
    {
        _standardJournalRepository = standardJournalRepository;
        _standardLineRepository = standardLineRepository;
        _batchRepository = batchRepository;
        _lineRepository = lineRepository;
    }

    public async Task<ListResultDto<StandardJournalDto>> GetListAsync(GetStandardJournalsInput input)
    {
        var templateName = input?.JournalTemplateName?.Trim().ToUpperInvariant();

        var journals = (await _standardJournalRepository.GetListAsync())
            .Where(j => templateName.IsNullOrWhiteSpace() || j.JournalTemplateName == templateName)
            .OrderBy(j => j.JournalTemplateName)
            .ThenBy(j => j.Code)
            .ToList();

        var lines = await _standardLineRepository.GetListAsync();
        var dtos = ObjectMapper.Map<List<StandardGeneralJournal>, List<StandardJournalDto>>(journals);

        for (var i = 0; i < journals.Count; i++)
        {
            dtos[i].LineCount = lines.Count(l => l.StandardGeneralJournalId == journals[i].Id);
        }

        return new ListResultDto<StandardJournalDto>(dtos);
    }

    [Authorize(ErpPermissions.Journals.Manage)]
    public async Task<StandardJournalDto> SaveFromBatchAsync(SaveStandardJournalInput input)
    {
        var batch = await _batchRepository.GetAsync(input.BatchId);

        var lines = (await _lineRepository.GetListAsync(l => l.GenJournalBatchId == batch.Id))
            .OrderBy(l => l.LineNo)
            .ToList();

        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Journals.NothingToPost);
        }

        var code = input.Code.Trim().ToUpperInvariant();

        // Saving again under the same code replaces the stored lines, as BC's Save does.
        var journal = await _standardJournalRepository.FindAsync(j =>
            j.JournalTemplateName == batch.JournalTemplateName && j.Code == code
        );

        if (journal == null)
        {
            journal = new StandardGeneralJournal(
                GuidGenerator.Create(),
                batch.JournalTemplateName,
                code,
                input.Description
            );
            journal.SaveFrom(lines, GuidGenerator.Create);
            await _standardJournalRepository.InsertAsync(journal, autoSave: true);
        }
        else
        {
            journal.SetDescription(input.Description);
            journal.SaveFrom(lines, GuidGenerator.Create);
            await _standardJournalRepository.UpdateAsync(journal, autoSave: true);
        }

        var dto = ObjectMapper.Map<StandardGeneralJournal, StandardJournalDto>(journal);
        dto.LineCount = journal.Lines.Count;
        return dto;
    }

    [Authorize(ErpPermissions.Journals.Create)]
    public async Task<ListResultDto<GenJournalLineDto>> ApplyToBatchAsync(ApplyStandardJournalInput input)
    {
        var journal = await _standardJournalRepository.GetAsync(input.StandardJournalId);
        var batch = await _batchRepository.GetAsync(input.BatchId);

        var savedLines = (await _standardLineRepository.GetListAsync(l => l.StandardGeneralJournalId == journal.Id))
            .OrderBy(l => l.LineNo)
            .ToList();

        if (savedLines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Journals.NothingToPost);
        }

        var existing = await _lineRepository.GetListAsync(l => l.GenJournalBatchId == batch.Id);
        var nextLineNo = existing.Count == 0 ? 1 : existing.Max(l => l.LineNo) + 1;

        var created = new List<GenJournalLine>();

        foreach (var saved in savedLines)
        {
            created.Add(
                new GenJournalLine(
                    GuidGenerator.Create(),
                    batch.Id,
                    nextLineNo++,
                    input.PostingDate,
                    saved.DocumentType,
                    input.DocumentNo,
                    saved.AccountType,
                    saved.AccountNo,
                    saved.Description,
                    saved.Amount,
                    saved.BalAccountType ?? batch.BalAccountType,
                    saved.BalAccountNo ?? batch.BalAccountNo
                )
            );
        }

        await _lineRepository.InsertManyAsync(created, autoSave: true);

        return new ListResultDto<GenJournalLineDto>(
            ObjectMapper.Map<List<GenJournalLine>, List<GenJournalLineDto>>(created)
        );
    }

    [Authorize(ErpPermissions.Journals.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _standardJournalRepository.DeleteAsync(id);
    }
}
