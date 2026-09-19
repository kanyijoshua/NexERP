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

[Authorize(ErpPermissions.Journals.Default)]
public class GeneralJournalAppService : ErpAppService, IGeneralJournalAppService
{
    private readonly IRepository<GenJournalBatch, Guid> _batchRepository;
    private readonly IRepository<GenJournalLine, Guid> _lineRepository;
    private readonly GenJnlPostLine _genJnlPostLine;

    public GeneralJournalAppService(
        IRepository<GenJournalBatch, Guid> batchRepository,
        IRepository<GenJournalLine, Guid> lineRepository,
        GenJnlPostLine genJnlPostLine
    )
    {
        _batchRepository = batchRepository;
        _lineRepository = lineRepository;
        _genJnlPostLine = genJnlPostLine;
    }

    public async Task<ListResultDto<GenJournalBatchDto>> GetBatchesAsync()
    {
        var batches = await _batchRepository.GetListAsync();

        return new ListResultDto<GenJournalBatchDto>(
            ObjectMapper.Map<List<GenJournalBatch>, List<GenJournalBatchDto>>(
                batches.OrderBy(b => b.JournalTemplateName).ThenBy(b => b.Name).ToList()
            )
        );
    }

    [Authorize(ErpPermissions.Journals.Create)]
    public async Task<GenJournalBatchDto> CreateBatchAsync(CreateGenJournalBatchDto input)
    {
        var batch = new GenJournalBatch(
            GuidGenerator.Create(),
            input.JournalTemplateName,
            input.Name,
            input.Description
        );

        await _batchRepository.InsertAsync(batch, autoSave: true);
        return ObjectMapper.Map<GenJournalBatch, GenJournalBatchDto>(batch);
    }

    public async Task<ListResultDto<GenJournalLineDto>> GetLinesAsync(Guid batchId)
    {
        var lines = await _lineRepository.GetListAsync(l => l.GenJournalBatchId == batchId);

        return new ListResultDto<GenJournalLineDto>(
            ObjectMapper.Map<List<GenJournalLine>, List<GenJournalLineDto>>(lines.OrderBy(l => l.LineNo).ToList())
        );
    }

    [Authorize(ErpPermissions.Journals.Create)]
    public async Task<GenJournalLineDto> CreateLineAsync(CreateGenJournalLineDto input)
    {
        // Fails with a 404 rather than leaving an orphan line if the batch is not in this company.
        await _batchRepository.GetAsync(input.GenJournalBatchId);

        // Max + 1, not Count + 1: deleting a line must not make the next number collide.
        var existing = await _lineRepository.GetListAsync(l => l.GenJournalBatchId == input.GenJournalBatchId);
        var nextLineNo = existing.Count == 0 ? 1 : existing.Max(l => l.LineNo) + 1;

        var line = new GenJournalLine(
            GuidGenerator.Create(),
            input.GenJournalBatchId,
            nextLineNo,
            input.PostingDate,
            input.DocumentType,
            input.DocumentNo,
            input.AccountType,
            input.AccountNo,
            input.Description,
            input.Amount,
            input.BalAccountType,
            input.BalAccountNo
        );

        await _lineRepository.InsertAsync(line, autoSave: true);
        return ObjectMapper.Map<GenJournalLine, GenJournalLineDto>(line);
    }

    [Authorize(ErpPermissions.Journals.Delete)]
    public async Task DeleteLineAsync(Guid lineId)
    {
        await _lineRepository.DeleteAsync(lineId);
    }

    // Not named "PostBatchAsync": ABP's conventional routing strips the HTTP-verb prefix.
    [Authorize(ErpPermissions.Journals.Post)]
    public async Task<GenJournalPostingResultDto> RunPostingAsync(Guid batchId)
    {
        var lines = (await _lineRepository.GetListAsync(l => l.GenJournalBatchId == batchId))
            .OrderBy(l => l.LineNo)
            .ToList();

        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Journals.NothingToPost);
        }

        // Business Central refuses a journal unless every document balances.
        // A line with a balancing account balances itself; the rest must net to zero per document.
        var outOfBalance = lines
            .Where(l => l.BalAccountNo.IsNullOrWhiteSpace())
            .GroupBy(l => l.DocumentNo)
            .Select(g => new { DocumentNo = g.Key, Balance = g.Sum(l => l.Amount) })
            .FirstOrDefault(d => d.Balance != 0m);

        if (outOfBalance != null)
        {
            throw new BusinessException(ErpErrorCodes.Journals.DocumentOutOfBalance)
                .WithData("documentNo", outOfBalance.DocumentNo)
                .WithData("balance", outOfBalance.Balance);
        }

        foreach (var line in lines)
        {
            await _genJnlPostLine.PostLineAsync(line);
        }

        await _lineRepository.DeleteManyAsync(lines);

        return new GenJournalPostingResultDto
        {
            PostedLineCount = lines.Count,
            PostedDocumentCount = lines.Select(l => l.DocumentNo).Distinct().Count(),
        };
    }
}
