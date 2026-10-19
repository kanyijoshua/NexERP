using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Creates reminders and finance charge memos from overdue customer entries and issues them.
/// Issuing posts the interest and fees the terms say to post to the customer, against the fee and
/// interest accounts of the customer posting group, and raises the reminder level of the entries.
/// </summary>
public class CollectionDocumentManager : DomainService
{
    private readonly IRepository<ReminderHeader, Guid> _reminderRepository;
    private readonly IRepository<FinanceChargeMemoHeader, Guid> _memoRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<CustomerLedgerEntry, Guid> _entryRepository;

    private T Repo<T>() where T : class => LazyServiceProvider.LazyGetRequiredService<T>();

    public CollectionDocumentManager(
        IRepository<ReminderHeader, Guid> reminderRepository,
        IRepository<FinanceChargeMemoHeader, Guid> memoRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<CustomerLedgerEntry, Guid> entryRepository
    )
    {
        _reminderRepository = reminderRepository;
        _memoRepository = memoRepository;
        _customerRepository = customerRepository;
        _entryRepository = entryRepository;
    }

    // --- reminders ------------------------------------------------------------------------------

    /// <summary>
    /// Creates a reminder for every customer (or the one given) with reminder terms and entries
    /// overdue past the grace period of their next level. Returns the reminders created.
    /// </summary>
    public async Task<List<ReminderHeader>> CreateRemindersAsync(DateTime documentDate, string customerNo = null)
    {
        var created = new List<ReminderHeader>();
        foreach (var customer in await CustomersWithAsync(c => c.ReminderTermsCode != null, customerNo))
        {
            var reminder = await NewReminderAsync(customer, documentDate);
            if (reminder != null)
            {
                await _reminderRepository.InsertAsync(reminder, autoSave: true);
                created.Add(reminder);
            }
        }

        return created;
    }

    /// <summary>Builds the reminder a customer would get on <paramref name="documentDate"/>; null when nothing is due.</summary>
    public async Task<ReminderHeader> NewReminderAsync(Customer customer, DateTime documentDate)
    {
        var terms = await Repo<IRepository<ReminderTerms, Guid>>().FirstOrDefaultAsync(t => t.Code == customer.ReminderTermsCode)
            ?? throw new BusinessException(ErpErrorCodes.FinanceOperations.ReminderTermsNotFound).WithData("code", customer.ReminderTermsCode ?? "");
        var levels = (await Repo<IRepository<ReminderLevel, Guid>>().GetListAsync(l => l.ReminderTermsCode == terms.Code)).OrderBy(l => l.No).ToList();
        if (levels.Count == 0)
        {
            return null;
        }

        var entries = await OverdueEntriesAsync(customer, documentDate);
        var due = new List<(CustomerLedgerEntry Entry, ReminderLevel Level)>();
        foreach (var entry in entries)
        {
            var nextLevel = entry.LastIssuedReminderLevel + 1;
            if (terms.MaxNoOfReminders > 0 && nextLevel > terms.MaxNoOfReminders)
            {
                continue;
            }

            var level = levels.LastOrDefault(l => l.No <= nextLevel) ?? levels[0];
            var graceEnds = level.GracePeriod == null ? entry.DueDate.Date : DateFormula.Parse(level.GracePeriod).Apply(entry.DueDate.Date);
            if (documentDate.Date > graceEnds)
            {
                due.Add((entry, level));
            }
        }

        if (due.Count == 0 || due.Sum(d => d.Entry.RemainingAmountLcy) < terms.MinimumAmountLcy)
        {
            return null;
        }

        var reminderLevel = due.Max(d => d.Level.No);
        var headerLevel = levels.First(l => l.No == reminderLevel);
        var no = await NextNoAsync(s => s.ReminderNos, documentDate) ?? FallbackNo("REM", documentDate, GuidGenerator.Create());
        var reminder = new ReminderHeader(GuidGenerator.Create(), no, customer, documentDate, documentDate, terms.Code, reminderLevel);
        reminder.SetDates(documentDate, documentDate, headerLevel.DueDateCalculation == null ? null : DateFormula.Parse(headerLevel.DueDateCalculation).Apply(documentDate.Date));

        var financeTerms = await FinanceTermsAsync(customer);
        var lineNo = 0;
        foreach (var (entry, level) in due)
        {
            lineNo += 10000;
            var line = new ReminderLine(GuidGenerator.Create(), reminder.Id, lineNo, CollectionLineType.CustomerLedgerEntry, $"{entry.DocumentType} {entry.DocumentNo}");
            var days = (documentDate.Date - entry.DueDate.Date).Days;
            var interest = level.CalculateInterest && financeTerms != null ? financeTerms.Interest(entry.RemainingAmountLcy, days) : 0m;
            line.SetEntry(entry, interest, days);
            reminder.Lines.Add(line);
        }

        if (headerLevel.AdditionalFeeLcy > 0m)
        {
            var accounts = await AccountsAsync(customer);
            lineNo += 10000;
            var fee = new ReminderLine(GuidGenerator.Create(), reminder.Id, lineNo, CollectionLineType.GLAccount, $"Additional fee, reminder level {reminderLevel}");
            fee.SetFee(accounts.AdditionalFeeAccountNo, headerLevel.AdditionalFeeLcy);
            reminder.Lines.Add(fee);
        }

        return reminder;
    }

    /// <summary>Issues a reminder: posts what its terms say to post and raises the level of its entries.</summary>
    public async Task IssueReminderAsync(Guid id, DateTime? postingDate = null)
    {
        var reminder = await _reminderRepository.GetAsync(id);
        reminder.EnsureOpen();
        if (!reminder.Lines.Any())
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.CollectionDocumentEmpty).WithData("documentNo", reminder.No);
        }

        var terms = await Repo<IRepository<ReminderTerms, Guid>>().FirstOrDefaultAsync(t => t.Code == reminder.ReminderTermsCode);
        var date = (postingDate ?? reminder.PostingDate).Date;
        var issuedNo = await NextNoAsync(s => s.IssuedReminderNos, date) ?? reminder.No;

        await PostAsync(
            reminder,
            issuedNo,
            date,
            GLEntryDocumentType.Reminder,
            GLRegister.ReminderSourceCode,
            postInterest: terms?.PostInterest ?? false,
            postFees: terms?.PostAdditionalFee ?? false
        );

        foreach (var line in reminder.Lines.Where(l => l.Type == CollectionLineType.CustomerLedgerEntry))
        {
            var entry = await _entryRepository.FirstOrDefaultAsync(e => e.EntryNo == line.EntryNo);
            if (entry != null && entry.LastIssuedReminderLevel < reminder.ReminderLevel)
            {
                entry.LastIssuedReminderLevel = reminder.ReminderLevel;
                await _entryRepository.UpdateAsync(entry);
            }
        }

        reminder.MarkIssued(issuedNo, date);
        await _reminderRepository.UpdateAsync(reminder, autoSave: true);
    }

    // --- finance charge memos -------------------------------------------------------------------

    /// <summary>Creates a finance charge memo for every customer (or the one given) with finance charge terms and overdue entries.</summary>
    public async Task<List<FinanceChargeMemoHeader>> CreateFinanceChargeMemosAsync(DateTime documentDate, string customerNo = null)
    {
        var created = new List<FinanceChargeMemoHeader>();
        foreach (var customer in await CustomersWithAsync(c => c.FinChargeTermsCode != null, customerNo))
        {
            var memo = await NewFinanceChargeMemoAsync(customer, documentDate);
            if (memo != null)
            {
                await _memoRepository.InsertAsync(memo, autoSave: true);
                created.Add(memo);
            }
        }

        return created;
    }

    public async Task<FinanceChargeMemoHeader> NewFinanceChargeMemoAsync(Customer customer, DateTime documentDate)
    {
        var terms = await FinanceTermsAsync(customer)
            ?? throw new BusinessException(ErpErrorCodes.FinanceOperations.FinanceChargeTermsNotFound).WithData("code", customer.FinChargeTermsCode ?? "");

        var lines = new List<(CustomerLedgerEntry Entry, decimal Interest, int Days)>();
        foreach (var entry in await OverdueEntriesAsync(customer, documentDate))
        {
            var start = terms.GracePeriod == null ? entry.DueDate.Date : DateFormula.Parse(terms.GracePeriod).Apply(entry.DueDate.Date);
            var days = (documentDate.Date - start).Days;
            var interest = terms.Interest(entry.RemainingAmountLcy, days);
            if (interest > 0m)
            {
                lines.Add((entry, interest, days));
            }
        }

        if (lines.Count == 0 || lines.Sum(l => l.Interest) + terms.AdditionalFeeLcy < terms.MinimumAmountLcy)
        {
            return null;
        }

        var no = await NextNoAsync(s => s.FinChargeMemoNos, documentDate) ?? FallbackNo("FIN", documentDate, GuidGenerator.Create());
        var memo = new FinanceChargeMemoHeader(GuidGenerator.Create(), no, customer, documentDate, documentDate, terms.Code);
        memo.SetDates(documentDate, documentDate, terms.DueDateCalculation == null ? null : DateFormula.Parse(terms.DueDateCalculation).Apply(documentDate.Date));

        var lineNo = 0;
        foreach (var (entry, interest, days) in lines)
        {
            lineNo += 10000;
            var line = new FinanceChargeMemoLine(GuidGenerator.Create(), memo.Id, lineNo, CollectionLineType.CustomerLedgerEntry, $"{entry.DocumentType} {entry.DocumentNo}");
            line.SetEntry(entry, interest, days);
            memo.Lines.Add(line);
        }

        if (terms.AdditionalFeeLcy > 0m)
        {
            var accounts = await AccountsAsync(customer);
            lineNo += 10000;
            var fee = new FinanceChargeMemoLine(GuidGenerator.Create(), memo.Id, lineNo, CollectionLineType.GLAccount, "Additional fee");
            fee.SetFee(accounts.AdditionalFeeAccountNo, terms.AdditionalFeeLcy);
            memo.Lines.Add(fee);
        }

        return memo;
    }

    public async Task IssueFinanceChargeMemoAsync(Guid id, DateTime? postingDate = null)
    {
        var memo = await _memoRepository.GetAsync(id);
        memo.EnsureOpen();
        if (!memo.Lines.Any())
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.CollectionDocumentEmpty).WithData("documentNo", memo.No);
        }

        var terms = await Repo<IRepository<FinanceChargeTerms, Guid>>().FirstOrDefaultAsync(t => t.Code == memo.FinChargeTermsCode);
        var date = (postingDate ?? memo.PostingDate).Date;
        var issuedNo = await NextNoAsync(s => s.IssuedFinChargeMemoNos, date) ?? memo.No;

        await PostAsync(
            memo,
            issuedNo,
            date,
            GLEntryDocumentType.FinanceChargeMemo,
            GLRegister.FinanceChargeSourceCode,
            postInterest: terms?.PostInterest ?? true,
            postFees: terms?.PostAdditionalFee ?? true
        );

        memo.MarkIssued(issuedNo, date);
        await _memoRepository.UpdateAsync(memo, autoSave: true);
    }

    // --- shared ---------------------------------------------------------------------------------

    /// <summary>
    /// Posts the interest and fees of an issued document: one customer entry for the total, and the
    /// income on the interest and fee accounts. Nothing is posted when nothing is to be posted.
    /// </summary>
    private async Task PostAsync(
        CollectionDocument document,
        string issuedNo,
        DateTime postingDate,
        GLEntryDocumentType documentType,
        string sourceCode,
        bool postInterest,
        bool postFees
    )
    {
        var interest = postInterest ? document.CollectionLines.Where(l => l.Type == CollectionLineType.CustomerLedgerEntry).Sum(l => l.Amount) : 0m;
        var fees = postFees ? document.CollectionLines.Where(l => l.Type == CollectionLineType.GLAccount && l.Amount != 0m).ToList() : [];
        var total = interest + fees.Sum(l => l.Amount);
        if (total == 0m)
        {
            return;
        }

        await Repo<GeneralLedgerSetupManager>().CheckPostingDateAsync(postingDate);
        var customer = await _customerRepository.GetAsync(document.CustomerId);
        var accounts = await AccountsAsync(customer);
        var registerManager = Repo<GLRegisterManager>();
        var register = await registerManager.OpenAsync(postingDate, sourceCode, issuedNo);
        var context = new GLPostingContext(register, sourceCode);
        var post = Repo<GenJnlPostLine>();

        var customerLine = new GenJournalLine(GuidGenerator.Create(), Guid.Empty, 1, postingDate, documentType, issuedNo, GenJournalAccountType.Customer, customer.No, $"{documentType} {issuedNo}", total);
        await post.PostLineAsync(customerLine, context, document.DueDate);

        if (interest != 0m)
        {
            if (accounts.InterestAccountNo.IsNullOrWhiteSpace())
            {
                throw AccountMissing(accounts, "interest");
            }

            await post.PostGLDirectAsync(accounts.InterestAccountNo, postingDate, documentType, issuedNo, "Interest", -interest, customer.No, context: context);
        }

        foreach (var fee in fees)
        {
            var accountNo = fee.AccountNo ?? accounts.AdditionalFeeAccountNo;
            if (accountNo.IsNullOrWhiteSpace())
            {
                throw AccountMissing(accounts, "additional fee");
            }

            await post.PostGLDirectAsync(accountNo, postingDate, documentType, issuedNo, fee.Description, -fee.Amount, customer.No, context: context);
        }

        await registerManager.CloseAsync(register);
    }

    private static BusinessException AccountMissing(CustomerPostingGroup group, string account) =>
        new BusinessException(ErpErrorCodes.FinanceOperations.CollectionAccountMissing).WithData("postingGroup", group.Code).WithData("account", account);

    private async Task<List<Customer>> CustomersWithAsync(System.Linq.Expressions.Expression<Func<Customer, bool>> predicate, string customerNo)
    {
        var no = customerNo?.Trim();
        var customers = await _customerRepository.GetListAsync(predicate);
        return customers.Where(c => no.IsNullOrEmpty() || c.No == no).OrderBy(c => c.No).ToList();
    }

    /// <summary>Open invoices and finance charges of the customer that were due before the date.</summary>
    private async Task<List<CustomerLedgerEntry>> OverdueEntriesAsync(Customer customer, DateTime documentDate)
    {
        var date = documentDate.Date;
        var entries = await _entryRepository.GetListAsync(e => e.CustomerId == customer.Id && e.Open && e.RemainingAmount > 0m && e.DueDate < date);
        return entries.OrderBy(e => e.DueDate).ThenBy(e => e.EntryNo).ToList();
    }

    private async Task<FinanceChargeTerms> FinanceTermsAsync(Customer customer)
    {
        return customer.FinChargeTermsCode == null
            ? null
            : await Repo<IRepository<FinanceChargeTerms, Guid>>().FirstOrDefaultAsync(t => t.Code == customer.FinChargeTermsCode);
    }

    private async Task<CustomerPostingGroup> AccountsAsync(Customer customer)
    {
        var groups = Repo<IRepository<CustomerPostingGroup, Guid>>();
        return (customer.CustomerPostingGroup.IsNullOrWhiteSpace() ? null : await groups.FirstOrDefaultAsync(g => g.Code == customer.CustomerPostingGroup))
            ?? throw new BusinessException(ErpErrorCodes.Customers.PostingGroupNotFound)
                .WithData("customerNo", customer.No)
                .WithData("postingGroup", customer.CustomerPostingGroup ?? "");
    }

    /// <summary>The next number of a series of the Sales &amp; Receivables Setup; null with no series.</summary>
    private async Task<string> NextNoAsync(Func<SalesReceivablesSetup, string> series, DateTime date)
    {
        var setup = await Repo<IRepository<SalesReceivablesSetup, Guid>>().FirstOrDefaultAsync();
        var code = setup == null ? null : series(setup);
        return code.IsNullOrWhiteSpace() ? null : await Repo<NoSeriesManager>().GetNextNoAsync(code, date);
    }

    /// <summary>A number for a new document when the setup has no series for it.</summary>
    internal static string FallbackNo(string prefix, DateTime date, Guid id) => $"{prefix}{date:yyMMdd}-{id.ToString("N")[..4].ToUpperInvariant()}";
}
