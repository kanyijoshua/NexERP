using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>Outcome of posting one journal batch.</summary>
public class GenJnlPostBatchResult
{
    public long RegisterNo { get; set; }

    public long TransactionNo { get; set; }

    public int PostedLineCount { get; set; }

    public int PostedDocumentCount { get; set; }

    /// <summary>Recurring lines that stayed behind with their date moved on.</summary>
    public int RecurringLineCount { get; set; }

    /// <summary>Reversing entries written on the day after the posting date.</summary>
    public int ReversingLineCount { get; set; }
}

/// <summary>
/// Posts a whole journal batch. Mirrors Business Central codeunit 13 "Gen. Jnl.-Post Batch".
/// <para>
/// Every line is checked before anything is written, so a batch posts whole or not at all.
/// Ordinary lines are removed afterwards; recurring lines stay and move to their next period.
/// </para>
/// </summary>
public class GenJnlPostBatch : DomainService
{
    private readonly IRepository<GenJournalLine, Guid> _lineRepository;
    private readonly IRepository<GenJournalBatch, Guid> _batchRepository;
    private readonly IRepository<GenJournalTemplate, Guid> _templateRepository;
    private readonly GLRegisterManager _registerManager;
    private readonly GenJnlCheckLine _checkLine;
    private readonly GenJnlPostLine _postLine;

    public GenJnlPostBatch(
        IRepository<GenJournalLine, Guid> lineRepository,
        IRepository<GenJournalBatch, Guid> batchRepository,
        IRepository<GenJournalTemplate, Guid> templateRepository,
        GLRegisterManager registerManager,
        GenJnlCheckLine checkLine,
        GenJnlPostLine postLine
    )
    {
        _lineRepository = lineRepository;
        _batchRepository = batchRepository;
        _templateRepository = templateRepository;
        _registerManager = registerManager;
        _checkLine = checkLine;
        _postLine = postLine;
    }

    /// <summary>
    /// Checks every line of the batch without writing anything, and returns the messages found.
    /// An empty list means the batch is ready to post.
    /// </summary>
    public async Task<List<string>> CheckBatchAsync(Guid batchId)
    {
        var lines = await GetLinesAsync(batchId);
        var messages = new List<string>();

        if (lines.Count == 0)
        {
            messages.Add(ErpErrorCodes.Journals.NothingToPost);
            return messages;
        }

        foreach (var line in lines)
        {
            try
            {
                await _checkLine.CheckAsync(line);
            }
            catch (BusinessException exception)
            {
                messages.Add(exception.Code);
            }
        }

        var outOfBalance = FindOutOfBalanceDocument(lines);
        if (outOfBalance != null)
        {
            messages.Add(ErpErrorCodes.Journals.DocumentOutOfBalance);
        }

        return messages;
    }

    public async Task<GenJnlPostBatchResult> PostAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        var template = await _templateRepository.FirstOrDefaultAsync(t =>
            t.Name == batch.JournalTemplateName
        );

        var lines = await GetLinesAsync(batchId);
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Journals.NothingToPost);
        }

        // Recurring lines past their expiration date are dropped rather than posted.
        var expired = lines.Where(l => l.IsExpiredOn(l.PostingDate)).ToList();
        lines = lines.Except(expired).ToList();

        if (lines.Count == 0)
        {
            await _lineRepository.DeleteManyAsync(expired, autoSave: true);
            throw new BusinessException(ErpErrorCodes.Journals.NothingToPost);
        }

        foreach (var line in lines)
        {
            await _checkLine.CheckAsync(line);
        }

        var outOfBalance = FindOutOfBalanceDocument(lines);
        if (outOfBalance != null)
        {
            throw new BusinessException(ErpErrorCodes.Journals.DocumentOutOfBalance)
                .WithData("documentNo", outOfBalance.DocumentNo)
                .WithData("balance", outOfBalance.Balance);
        }

        var register = await _registerManager.OpenAsync(
            lines.Min(l => l.PostingDate),
            template?.SourceCode,
            $"{batch.JournalTemplateName}/{batch.Name}"
        );

        var context = new GLPostingContext(register, template?.SourceCode, batch.ReasonCode);

        var result = new GenJnlPostBatchResult
        {
            RegisterNo = register.No,
            TransactionNo = register.TransactionNo,
            PostedLineCount = lines.Count(l => l.Amount != 0m),
            PostedDocumentCount = lines.Where(l => l.Amount != 0m).Select(l => l.DocumentNo).Distinct().Count(),
        };

        foreach (var line in lines)
        {
            await _postLine.PostLineAsync(line, context);

            if (line.IsReversing && line.Amount != 0m)
            {
                await PostReversingCounterpartAsync(line, context);
                result.ReversingLineCount++;
            }
        }

        await CloseBatchAsync(lines, expired, result);
        await _registerManager.CloseAsync(register);

        return result;
    }

    /// <summary>
    /// An accrual reverses itself: the same line is posted again the next day with the opposite
    /// sign. Mirrors what BC's reversing recurring methods do inside one posting run.
    /// </summary>
    private async Task PostReversingCounterpartAsync(GenJournalLine line, GLPostingContext context)
    {
        var reversal = new GenJournalLine(
            GuidGenerator.Create(),
            line.GenJournalBatchId,
            line.LineNo,
            line.PostingDate.AddDays(1),
            line.DocumentType,
            line.DocumentNo,
            line.AccountType,
            line.AccountNo,
            line.Description,
            -line.Amount,
            line.BalAccountType,
            line.BalAccountNo,
            line.DimensionSetId
        );

        await _postLine.PostLineAsync(reversal, context);
    }

    private async Task CloseBatchAsync(
        List<GenJournalLine> lines,
        List<GenJournalLine> expired,
        GenJnlPostBatchResult result
    )
    {
        var recurring = lines.Where(l => l.RecurringMethod != RecurringMethod.None).ToList();

        foreach (var line in recurring)
        {
            line.AdvanceRecurring();
            await _lineRepository.UpdateAsync(line);
        }

        result.RecurringLineCount = recurring.Count;

        var spent = lines.Except(recurring).Concat(expired).ToList();
        if (spent.Count > 0)
        {
            await _lineRepository.DeleteManyAsync(spent);
        }
    }

    private async Task<List<GenJournalLine>> GetLinesAsync(Guid batchId)
    {
        var lines = await _lineRepository.GetListAsync(l => l.GenJournalBatchId == batchId);
        return lines.OrderBy(l => l.LineNo).ToList();
    }

    /// <summary>
    /// Business Central refuses a journal unless every document balances. A line with a balancing
    /// account balances itself; the rest must net to zero per document number.
    /// </summary>
    private static DocumentBalance FindOutOfBalanceDocument(IEnumerable<GenJournalLine> lines)
    {
        return lines
            .Where(l => l.BalAccountNo == null)
            .GroupBy(l => l.DocumentNo)
            .Select(g => new DocumentBalance(g.Key, g.Sum(l => l.Amount)))
            .FirstOrDefault(d => d.Balance != 0m);
    }

    private sealed record DocumentBalance(string DocumentNo, decimal Balance);
}
