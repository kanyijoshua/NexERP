using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.CashManagement;

/// <summary>
/// Reconciles a bank account against its statement: statement lines are imported, matched to the
/// bank ledger entries they stand for (automatically by amount and date, or by hand), lines with
/// nothing to match are posted as payments, and the statement is posted once every line is
/// matched and the ending balance agrees. Posting closes the matched entries and keeps the
/// statement as a bank account statement.
/// </summary>
public class BankReconciliationManager : DomainService
{
    private readonly IRepository<BankAccReconciliation, Guid> _recRepository;
    private readonly IRepository<BankAccReconciliationLine, Guid> _lineRepository;
    private readonly IRepository<BankAccountLedgerEntry, Guid> _entryRepository;
    private readonly IRepository<BankAccount, Guid> _bankRepository;

    private IRepository<BankAccountStatement, Guid> StatementRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<BankAccountStatement, Guid>>();
    private IRepository<BankAccountStatementLine, Guid> StatementLineRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<BankAccountStatementLine, Guid>>();
    private GenJnlPostLine GenJnlPostLine => LazyServiceProvider.LazyGetRequiredService<GenJnlPostLine>();
    private GLRegisterManager RegisterManager => LazyServiceProvider.LazyGetRequiredService<GLRegisterManager>();
    private GeneralLedgerSetupManager GLSetupManager => LazyServiceProvider.LazyGetRequiredService<GeneralLedgerSetupManager>();

    public BankReconciliationManager(
        IRepository<BankAccReconciliation, Guid> recRepository,
        IRepository<BankAccReconciliationLine, Guid> lineRepository,
        IRepository<BankAccountLedgerEntry, Guid> entryRepository,
        IRepository<BankAccount, Guid> bankRepository
    )
    {
        _recRepository = recRepository;
        _lineRepository = lineRepository;
        _entryRepository = entryRepository;
        _bankRepository = bankRepository;
    }

    public async Task<List<BankAccReconciliationLine>> GetLinesAsync(BankAccReconciliation rec)
    {
        var lines = await _lineRepository.GetListAsync(l => l.BankAccountNo == rec.BankAccountNo && l.StatementNo == rec.StatementNo && l.StatementType == rec.StatementType);
        return lines.OrderBy(l => l.StatementLineNo).ToList();
    }

    /// <summary>The bank entries still waiting for a statement, and those matched on this one.</summary>
    public async Task<List<BankAccountLedgerEntry>> GetCandidateEntriesAsync(BankAccReconciliation rec)
    {
        var entries = await _entryRepository.GetListAsync(e =>
            e.BankAccountNo == rec.BankAccountNo
            && !e.Reversed
            && (
                (e.Open && e.StatementStatus == BankEntryStatementStatus.Open)
                || (e.StatementStatus == BankEntryStatementStatus.BankAccEntryApplied && e.StatementNo == rec.StatementNo)
            )
        );
        return entries.Where(e => e.Amount != 0m).OrderBy(e => e.PostingDate).ThenBy(e => e.EntryNo).ToList();
    }

    /// <summary>
    /// Adds the lines of a statement file: date, description, amount and optionally the document
    /// no., one line per row. A first row that is not a date is taken as a header.
    /// </summary>
    public async Task<int> ImportStatementAsync(Guid recId, string content)
    {
        var rec = await _recRepository.GetAsync(recId);
        var rows = DelimitedReader.Read(content ?? string.Empty);
        var lineNo = (await GetLinesAsync(rec)).Select(l => l.StatementLineNo).DefaultIfEmpty(0).Max();
        var added = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Count == 0 || row.All(c => c.IsNullOrWhiteSpace()))
            {
                continue;
            }

            if (!TryDate(row[0], out var date))
            {
                if (i == 0)
                {
                    continue;
                }

                throw ImportError(i + 1, "date");
            }

            if (row.Count < 3 || !decimal.TryParse(row[2]?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                throw ImportError(i + 1, "amount");
            }

            lineNo += 10000;
            var line = new BankAccReconciliationLine(GuidGenerator.Create(), rec.StatementType, rec.BankAccountNo, rec.StatementNo, lineNo);
            line.Set(
                row.Count > 3 ? Truncate(row[3], 20) : null,
                date,
                Truncate(row[1], 100),
                amount,
                amount,
                0m,
                date,
                false,
                null,
                null,
                null,
                GenJournalAccountType.GLAccount,
                null,
                Truncate(row[1], 140),
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );
            await _lineRepository.InsertAsync(line);
            added++;
        }

        return added;
    }

    /// <summary>
    /// Matches every unmatched statement line to one open entry of the same amount whose date is
    /// within <paramref name="dateTolerance"/> days, preferring the same document no. Returns how
    /// many lines it matched.
    /// </summary>
    public async Task<int> MatchAutomaticallyAsync(Guid recId, int dateTolerance)
    {
        var rec = await _recRepository.GetAsync(recId);
        var lines = await GetLinesAsync(rec);
        var free = (await GetCandidateEntriesAsync(rec)).Where(e => e.StatementStatus == BankEntryStatementStatus.Open).ToList();
        var matched = 0;

        foreach (var line in lines.Where(l => l.AppliedAmount == 0m && l.StatementAmount != 0m))
        {
            var date = line.TransactionDate ?? rec.StatementDate ?? DateTime.Today;
            var candidates = free
                .Where(e => e.Amount == line.StatementAmount && Math.Abs((e.PostingDate.Date - date.Date).TotalDays) <= Math.Max(0, dateTolerance))
                .OrderByDescending(e => line.DocumentNo != null && string.Equals(e.DocumentNo, line.DocumentNo, StringComparison.OrdinalIgnoreCase))
                .ThenBy(e => Math.Abs((e.PostingDate.Date - date.Date).TotalDays))
                .ThenBy(e => e.EntryNo)
                .ToList();
            if (candidates.Count == 0)
            {
                continue;
            }

            var entry = candidates[0];
            free.Remove(entry);
            await MatchAsync(rec, line, [entry]);
            matched++;
        }

        return matched;
    }

    /// <summary>Matches one statement line to one or more open entries.</summary>
    public async Task MatchManuallyAsync(Guid recId, Guid lineId, IReadOnlyCollection<Guid> entryIds)
    {
        var rec = await _recRepository.GetAsync(recId);
        var line = await GetLineAsync(rec, lineId);
        var entries = await _entryRepository.GetListAsync(e => entryIds.Contains(e.Id));
        foreach (var entry in entries)
        {
            if (entry.BankAccountNo != rec.BankAccountNo)
            {
                throw new BusinessException(ErpErrorCodes.FinanceOperations.BankEntryWrongAccount)
                    .WithData("entryNo", entry.EntryNo)
                    .WithData("bankAccountNo", rec.BankAccountNo);
            }
            if (!entry.Open || entry.StatementStatus != BankEntryStatementStatus.Open)
            {
                throw new BusinessException(ErpErrorCodes.FinanceOperations.BankEntryAlreadyMatched).WithData("entryNo", entry.EntryNo);
            }
        }

        await MatchAsync(rec, line, entries);
    }

    /// <summary>Takes every entry off a statement line.</summary>
    public async Task RemoveMatchAsync(Guid recId, Guid lineId)
    {
        var rec = await _recRepository.GetAsync(recId);
        var line = await GetLineAsync(rec, lineId);
        var entries = await _entryRepository.GetListAsync(e =>
            e.BankAccountNo == rec.BankAccountNo && e.StatementNo == rec.StatementNo && e.StatementLineNo == line.StatementLineNo && e.StatementStatus == BankEntryStatementStatus.BankAccEntryApplied);
        foreach (var entry in entries)
        {
            entry.SetStatement(BankEntryStatementStatus.Open, null, 0);
            await _entryRepository.UpdateAsync(entry);
        }

        line.SetApplied(0m);
        await _lineRepository.UpdateAsync(line);
    }

    /// <summary>
    /// Posts the difference of every line that names an account to post it to (bank charges,
    /// interest, a customer payment not yet recorded) and matches the bank entry that writes.
    /// </summary>
    public async Task<int> PostDifferencesAsync(Guid recId)
    {
        var rec = await _recRepository.GetAsync(recId);
        var lines = (await GetLinesAsync(rec)).Where(l => l.Difference != 0m && l.AccountNo != null).ToList();
        if (lines.Count == 0)
        {
            return 0;
        }

        var postingDate = rec.StatementDate ?? DateTime.Today;
        await GLSetupManager.CheckPostingDateAsync(postingDate);
        var register = await RegisterManager.OpenAsync(postingDate, GLRegister.BankReconciliationSourceCode, rec.StatementNo);
        var context = new GLPostingContext(register, GLRegister.BankReconciliationSourceCode);

        foreach (var line in lines)
        {
            var documentNo = line.DocumentNo ?? $"{rec.StatementNo}-{line.StatementLineNo}";
            var journalLine = new GenJournalLine(
                GuidGenerator.Create(),
                Guid.Empty,
                line.StatementLineNo,
                line.TransactionDate ?? postingDate,
                line.Difference > 0m ? GLEntryDocumentType.Payment : GLEntryDocumentType.None,
                documentNo,
                GenJournalAccountType.BankAccount,
                rec.BankAccountNo,
                line.Description,
                line.Difference,
                line.AccountType,
                line.AccountNo
            );
            await GenJnlPostLine.PostLineAsync(journalLine, context);
            await LazyServiceProvider.LazyGetRequiredService<Volo.Abp.Uow.IUnitOfWorkManager>().Current!.SaveChangesAsync();

            // The entry it wrote is what the statement line shows.
            var entry = await _entryRepository.FirstAsync(e => e.BankAccountNo == rec.BankAccountNo && e.TransactionNo == context.TransactionNo && e.DocumentNo == documentNo && e.Amount == line.Difference);
            await MatchAsync(rec, line, [entry]);
        }

        await RegisterManager.CloseAsync(register);
        return lines.Count;
    }

    /// <summary>
    /// Posts the statement: every line matched in full, and the ending balance equal to the last
    /// statement's balance plus the lines. The matched entries close and the statement is kept.
    /// </summary>
    public async Task<BankAccountStatement> PostAsync(Guid recId)
    {
        var rec = await _recRepository.GetAsync(recId);
        var lines = await GetLinesAsync(rec);

        var unmatched = lines.FirstOrDefault(l => l.Difference != 0m);
        if (unmatched != null)
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.BankRecLineNotFullyApplied)
                .WithData("lineNo", unmatched.StatementLineNo)
                .WithData("difference", unmatched.Difference);
        }

        var difference = rec.StatementEndingBalance - (rec.BalanceLastStatement + lines.Sum(l => l.StatementAmount));
        if (difference != 0m)
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.BankRecDifference).WithData("difference", difference);
        }

        var bank = await _bankRepository.FirstOrDefaultAsync(b => b.No == rec.BankAccountNo)
            ?? throw new BusinessException(ErpErrorCodes.CashManagement.BankAccountNotFound).WithData("accountNo", rec.BankAccountNo);

        var entries = await _entryRepository.GetListAsync(e =>
            e.BankAccountNo == rec.BankAccountNo && e.StatementNo == rec.StatementNo && e.StatementStatus == BankEntryStatementStatus.BankAccEntryApplied);
        var allEntries = await _entryRepository.GetListAsync(e => e.BankAccountNo == rec.BankAccountNo);
        var statementDate = rec.StatementDate ?? DateTime.Today;
        var glBalance = allEntries.Where(e => e.PostingDate.Date <= statementDate.Date).Sum(e => e.Amount);
        var outstanding = allEntries.Where(e => e.Open && e.StatementStatus == BankEntryStatementStatus.Open && e.PostingDate.Date <= statementDate.Date).ToList();

        var statement = new BankAccountStatement(GuidGenerator.Create())
        {
            BankAccountNo = rec.BankAccountNo,
            StatementNo = rec.StatementNo,
            StatementEndingBalance = rec.StatementEndingBalance,
            StatementDate = rec.StatementDate,
            BalanceLastStatement = rec.BalanceLastStatement,
            GLBalanceAtPostingDate = glBalance,
            OutstdPaymentsAtPosting = outstanding.Where(e => e.Amount < 0m).Sum(e => e.Amount),
            OutstdTransactAtPosting = outstanding.Where(e => e.Amount > 0m).Sum(e => e.Amount),
        };
        await StatementRepository.InsertAsync(statement);

        foreach (var line in lines)
        {
            await StatementLineRepository.InsertAsync(new BankAccountStatementLine(GuidGenerator.Create())
            {
                BankAccountNo = rec.BankAccountNo,
                StatementNo = rec.StatementNo,
                StatementLineNo = line.StatementLineNo,
                DocumentNo = line.DocumentNo,
                TransactionDate = line.TransactionDate,
                Description = line.Description,
                StatementAmount = line.StatementAmount,
                Difference = line.Difference,
                AppliedAmount = line.AppliedAmount,
                AppliedEntries = entries.Count(e => e.StatementLineNo == line.StatementLineNo),
                ValueDate = line.ValueDate,
                CheckNo = line.CheckNo,
                TransactionId = line.TransactionId,
            });
            await _lineRepository.DeleteAsync(line);
        }

        foreach (var entry in entries)
        {
            entry.CloseOnStatement();
            await _entryRepository.UpdateAsync(entry);
        }

        bank.RecordStatement(rec.StatementNo, rec.StatementEndingBalance);
        await _bankRepository.UpdateAsync(bank);
        await _recRepository.DeleteAsync(rec);

        return statement;
    }

    private async Task MatchAsync(BankAccReconciliation rec, BankAccReconciliationLine line, IEnumerable<BankAccountLedgerEntry> entries)
    {
        var applied = line.AppliedAmount;
        foreach (var entry in entries)
        {
            entry.SetStatement(BankEntryStatementStatus.BankAccEntryApplied, rec.StatementNo, line.StatementLineNo);
            await _entryRepository.UpdateAsync(entry);
            applied += entry.Amount;
        }

        line.SetApplied(applied);
        await _lineRepository.UpdateAsync(line);
    }

    private async Task<BankAccReconciliationLine> GetLineAsync(BankAccReconciliation rec, Guid lineId)
    {
        var line = await _lineRepository.GetAsync(lineId);
        if (line.BankAccountNo != rec.BankAccountNo || line.StatementNo != rec.StatementNo)
        {
            throw new EntityNotFoundException(typeof(BankAccReconciliationLine), lineId);
        }

        return line;
    }

    private static bool TryDate(string text, out DateTime date) =>
        DateTime.TryParseExact(text?.Trim(), ["yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd.MM.yyyy", "yyyyMMdd"], CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    private static string Truncate(string text, int length)
    {
        var trimmed = text?.Trim();
        return trimmed.IsNullOrEmpty() ? null : trimmed.Length <= length ? trimmed : trimmed[..length];
    }

    private static BusinessException ImportError(int row, string field) =>
        new BusinessException(ErpErrorCodes.FinanceOperations.StatementImportInvalid).WithData("line", row).WithData("field", field);
}
