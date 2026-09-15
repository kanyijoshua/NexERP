using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

public class CreateGenJournalLineDto
{
    public Guid GenJournalBatchId { get; set; }
    public DateTime PostingDate { get; set; }
    public GLEntryDocumentType DocumentType { get; set; }
    public string DocumentNo { get; set; }
    public string AccountType { get; set; }
    public string AccountNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string BalAccountType { get; set; }
    public string BalAccountNo { get; set; }
}

public class GeneralJournalAppService : ApplicationService
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

    [Authorize(ErpPermissions.Journals.Default)]
    public async Task<List<GenJournalBatch>> GetBatchesAsync()
    {
        return await _batchRepository.GetListAsync();
    }

    [Authorize(ErpPermissions.Journals.Default)]
    public async Task<List<GenJournalLine>> GetLinesAsync(Guid batchId)
    {
        return await _lineRepository.GetListAsync(l => l.GenJournalBatchId == batchId);
    }

    [Authorize(ErpPermissions.Journals.Default)]
    public async Task<GenJournalLine> CreateLineAsync(CreateGenJournalLineDto input)
    {
        var count = await _lineRepository.CountAsync(l => l.GenJournalBatchId == input.GenJournalBatchId);
        var line = new GenJournalLine(
            GuidGenerator.Create(),
            input.GenJournalBatchId,
            (int)count + 1,
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
        return await _lineRepository.InsertAsync(line, autoSave: true);
    }

    [Authorize(ErpPermissions.Journals.Post)]
    public async Task PostBatchAsync(Guid batchId)
    {
        var lines = await _lineRepository.GetListAsync(l => l.GenJournalBatchId == batchId);
        foreach (var line in lines)
        {
            await _genJnlPostLine.PostLineAsync(line);
            await _lineRepository.DeleteAsync(line);
        }
    }
}
